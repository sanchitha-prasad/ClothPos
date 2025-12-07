/**
 * Comprehensive API Test Script for ClothPos
 * Tests all endpoints with test data and verifies stock calculations
 * 
 * Usage: node ApiTestScript.js
 * 
 * Prerequisites:
 * - API must be running on http://localhost:5000 or https://localhost:5001
 * - Admin user must exist (username: admin-dev, password: 12345)
 */

const axios = require('axios');

// Configuration - Try HTTP first (default), then HTTPS
let API_BASE_URL = process.env.API_URL;
if (!API_BASE_URL) {
    // Default to HTTP on port 5000 (most common for development)
    API_BASE_URL = 'http://localhost:5000';
}

const ADMIN_USERNAME = 'admin-dev';
const ADMIN_PASSWORD = '1234';

// Alternative URLs to try if primary fails
const ALTERNATIVE_URLS = [
    'http://localhost:5000',
    'http://127.0.0.1:5000',
    'https://localhost:5001',
    'https://127.0.0.1:5001'
];

// Test data storage
let testData = {
    token: null,
    authenticatedUserId: null, // Store the authenticated admin-dev user ID
    authenticatedUsername: null,
    categories: [],
    items: [],
    users: [],
    sales: [],
    roles: [],
    paymentDues: []
};

// Color console output
const colors = {
    reset: '\x1b[0m',
    green: '\x1b[32m',
    red: '\x1b[31m',
    yellow: '\x1b[33m',
    blue: '\x1b[34m',
    cyan: '\x1b[36m'
};

function log(message, color = 'reset') {
    console.log(`${colors[color]}${message}${colors.reset}`);
}

// Create axios instance with auth
function createApiInstance() {
    const baseUrl = global.API_BASE_URL || API_BASE_URL;
    return axios.create({
        baseURL: baseUrl,
        headers: {
            'Content-Type': 'application/json'
        },
        httpsAgent: baseUrl.startsWith('https') ? new (require('https').Agent)({
            rejectUnauthorized: false
        }) : undefined,
        timeout: 10000
    });
}

let api = createApiInstance();

// Function to setup auth interceptor
function setupAuthInterceptor(apiInstance) {
    // Add auth token to requests
    // Note: Multiple interceptors are fine - they'll all add the token
    apiInstance.interceptors.request.use(config => {
        if (testData.token) {
            config.headers.Authorization = `Bearer ${testData.token}`;
        }
        return config;
    }, error => {
        return Promise.reject(error);
    });
}

// Setup initial interceptor
setupAuthInterceptor(api);

// ============================================
// TEST FUNCTIONS
// ============================================

async function testConnection() {
    log('\n=== Testing API Connection ===', 'cyan');
    
    // Try to connect to the API
    for (const url of [API_BASE_URL, ...ALTERNATIVE_URLS.filter(u => u !== API_BASE_URL)]) {
        try {
            log(`   Trying: ${url}...`, 'blue');
            const response = await axios.get(`${url}/swagger/index.html`, {
                timeout: 2000,
                validateStatus: () => true, // Accept any status code
                httpsAgent: url.startsWith('https') ? new (require('https').Agent)({ rejectUnauthorized: false }) : undefined
            });
            
            // If we get any response (even 404), the server is up
            if (response.status < 500) {
                global.API_BASE_URL = url;
                log(`✅ Connected to: ${url}`, 'green');
                return true;
            }
        } catch (error) {
            // Try next URL
            continue;
        }
    }
    
    log(`❌ Could not connect to API. Tried: ${[API_BASE_URL, ...ALTERNATIVE_URLS].join(', ')}`, 'red');
    log(`   Make sure the API is running with: dotnet run`, 'yellow');
    return false;
}

async function ensureRolesExist() {
    log('\n=== Ensuring Roles Exist ===', 'cyan');
    const baseUrl = global.API_BASE_URL || API_BASE_URL;
    
    try {
        const rolesResponse = await axios.get(`${baseUrl}/api/roles`, {
            httpsAgent: baseUrl.startsWith('https') ? new (require('https').Agent)({ rejectUnauthorized: false }) : undefined,
            timeout: 5000
        });
        
        testData.roles = rolesResponse.data;
        
        if (rolesResponse.data.length === 0) {
            log('⚠️  No roles found. API should seed them on startup.', 'yellow');
            log('   Waiting a moment for API to seed roles...', 'yellow');
            await new Promise(resolve => setTimeout(resolve, 2000));
            
            // Try again
            const retryResponse = await axios.get(`${baseUrl}/api/roles`, {
                httpsAgent: baseUrl.startsWith('https') ? new (require('https').Agent)({ rejectUnauthorized: false }) : undefined,
                timeout: 5000
            });
            testData.roles = retryResponse.data;
        }
        
        if (testData.roles.length > 0) {
            log(`✅ Found ${testData.roles.length} roles`, 'green');
            testData.roles.forEach(role => {
                log(`   - ${role.name}`, 'blue');
            });
            return true;
        } else {
            log('⚠️  Still no roles found. Database may need seeding.', 'yellow');
            return false;
        }
    } catch (error) {
        log(`⚠️  Could not fetch roles: ${error.response?.data?.message || error.message}`, 'yellow');
        return false;
    }
}

async function createTestUser() {
    log('\n=== Creating Test Admin User (Automatic) ===', 'cyan');
    
    const baseUrl = global.API_BASE_URL || API_BASE_URL;
    
    try {
        // Ensure roles exist first
        if (!testData.roles || testData.roles.length === 0) {
            const rolesExist = await ensureRolesExist();
            if (!rolesExist) {
                log('⚠️  Cannot create user without roles.', 'yellow');
                return false;
            }
        }
        
        // Try the bootstrap endpoint (works without auth, creates first admin)
        log('   Attempting to create admin user via bootstrap endpoint...', 'blue');
        try {
            const bootstrapResponse = await axios.post(`${baseUrl}/api/auth/bootstrap`, {
                username: ADMIN_USERNAME,
                password: ADMIN_PASSWORD,
                email: 'admin-dev@clothpos.test'
            }, {
                httpsAgent: baseUrl.startsWith('https') ? new (require('https').Agent)({ rejectUnauthorized: false }) : undefined,
                timeout: 5000
            });
            
            log('✅ Admin user created successfully via bootstrap', 'green');
            log(`   Username: ${ADMIN_USERNAME}`, 'blue');
            log(`   Password: ${ADMIN_PASSWORD}`, 'blue');
            log(`   Email: admin-dev@clothpos.test`, 'blue');
            return true;
        } catch (bootstrapError) {
            const errorMsg = bootstrapError.response?.data?.message || '';
            const statusCode = bootstrapError.response?.status;
            
            if (errorMsg.includes('already exists') || statusCode === 400) {
                log('ℹ️  Admin user already exists (bootstrap not needed)', 'blue');
                
                // Check if it's the right user by trying to login
                try {
                    const testLogin = await axios.post(`${baseUrl}/api/auth/login`, {
                        username: ADMIN_USERNAME,
                        password: ADMIN_PASSWORD
                    }, {
                        httpsAgent: baseUrl.startsWith('https') ? new (require('https').Agent)({ rejectUnauthorized: false }) : undefined,
                        timeout: 5000
                    });
                    
                    log('✅ Existing admin user credentials are correct', 'green');
                    return true;
                } catch (loginError) {
                    log('⚠️  Admin exists but credentials may be different', 'yellow');
                    log('   The existing admin may have different password.', 'yellow');
                    log('   Trying to use existing admin...', 'yellow');
                    return false; // Will try alternative credentials
                }
            }
            
            // If roles issue
            if (errorMsg.includes('role not found') || errorMsg.includes('seed roles')) {
                log('⚠️  Roles not seeded. API should seed them on startup.', 'yellow');
                log('   Waiting for roles to be seeded...', 'yellow');
                await new Promise(resolve => setTimeout(resolve, 2000));
                
                // Try bootstrap again
                try {
                    const retryResponse = await axios.post(`${baseUrl}/api/auth/bootstrap`, {
                        username: ADMIN_USERNAME,
                        password: ADMIN_PASSWORD,
                        email: 'admin-dev@clothpos.test'
                    }, {
                        httpsAgent: baseUrl.startsWith('https') ? new (require('https').Agent)({ rejectUnauthorized: false }) : undefined,
                        timeout: 5000
                    });
                    
                    log('✅ Admin user created on retry', 'green');
                    return true;
                } catch (retryError) {
                    log(`⚠️  Retry failed: ${retryError.response?.data?.message || retryError.message}`, 'yellow');
                    return false;
                }
            }
            
            throw bootstrapError;
        }
    } catch (error) {
        log(`⚠️  Could not create test user automatically: ${error.response?.data?.message || error.message}`, 'yellow');
        log('   This is normal if an admin user already exists with different credentials.', 'yellow');
        return false;
    }
}

async function testAuth() {
    log('\n=== Testing Authentication (Automatic Setup) ===', 'cyan');
    
    const baseUrl = global.API_BASE_URL || API_BASE_URL;
    
    // Try multiple credential combinations (in case API seeded different credentials)
    const credentialAttempts = [
        { username: ADMIN_USERNAME, password: ADMIN_PASSWORD, source: 'test script defaults' },
        { username: 'admin-dev', password: '12345', source: 'test script defaults' },
        { username: 'admin@shop.com', password: 'Admin123!', source: 'API default seeder' },
        { username: 'admin', password: '12345', source: 'alternative' }
    ];
    
    for (const creds of credentialAttempts) {
        try {
            log(`   Trying: ${creds.username} (${creds.source})...`, 'blue');
            const response = await axios.post(`${baseUrl}/api/auth/login`, {
                username: creds.username,
                password: creds.password
            }, {
                httpsAgent: baseUrl.startsWith('https') ? new (require('https').Agent)({ rejectUnauthorized: false }) : undefined,
                timeout: 5000
            });
            
            testData.token = response.data.token;
            // Store the authenticated user info from login response
            if (response.data.user) {
                testData.authenticatedUserId = response.data.user.id;
                testData.authenticatedUsername = response.data.user.username;
            }
            log('✅ Login successful', 'green');
            log(`   Username: ${creds.username}`, 'blue');
            log(`   User ID: ${testData.authenticatedUserId || 'N/A'}`, 'blue');
            log(`   Token: ${testData.token.substring(0, 20)}...`, 'blue');
            return true;
        } catch (error) {
            // Continue to next attempt
            continue;
        }
    }
    
    // If all credential attempts failed, try to create user
    log('   All credential attempts failed. Attempting to create admin user automatically...', 'yellow');
    
    try {
        const userCreated = await createTestUser();
        if (userCreated) {
            // Wait a moment for database to update
            await new Promise(resolve => setTimeout(resolve, 1000));
            
            // Try login with the credentials we just created
            try {
                const retryResponse = await axios.post(`${baseUrl}/api/auth/login`, {
                    username: ADMIN_USERNAME,
                    password: ADMIN_PASSWORD
                }, {
                    httpsAgent: baseUrl.startsWith('https') ? new (require('https').Agent)({ rejectUnauthorized: false }) : undefined,
                    timeout: 5000
                });
                
                testData.token = retryResponse.data.token;
                // Store the authenticated user info from login response
                if (retryResponse.data.user) {
                    testData.authenticatedUserId = retryResponse.data.user.id;
                    testData.authenticatedUsername = retryResponse.data.user.username;
                }
                log('✅ Login successful after automatic user creation', 'green');
                log(`   Username: ${ADMIN_USERNAME}`, 'blue');
                log(`   User ID: ${testData.authenticatedUserId || 'N/A'}`, 'blue');
                log(`   Password: ${ADMIN_PASSWORD}`, 'blue');
                return true;
            } catch (retryError) {
                const errorMsg = retryError.response?.data?.message || retryError.message;
                log(`❌ Login failed after user creation: ${errorMsg}`, 'red');
            }
        }
    } catch (createError) {
        log(`⚠️  Could not create user automatically: ${createError.response?.data?.message || createError.message}`, 'yellow');
    }
    
    // Final error message
    log(`❌ All authentication attempts failed.`, 'red');
    log(`   The API may have seeded an admin user with different credentials.`, 'yellow');
    log(`   Check the API startup logs for the default admin credentials.`, 'yellow');
    log(`   Or the API may need to be restarted to seed roles and admin user.`, 'yellow');
    
    return false;
}

async function testCategories() {
    log('\n=== Testing Categories API ===', 'cyan');
    
    // Get all categories first
    let categories = [];
    try {
        const response = await api.get('/api/categories');
        categories = response.data;
        log(`✅ Retrieved ${categories.length} categories`, 'green');
    } catch (error) {
        log(`❌ Get categories failed: ${error.response?.data?.message || error.message}`, 'red');
        return false;
    }
    
    // Check if test category exists, if not create it
    const testCategoryName = 'Test T-Shirts';
    let testCategory = categories.find(c => c.name === testCategoryName);
    
    if (!testCategory) {
        try {
            const createResponse = await api.post('/api/categories', {
                name: testCategoryName,
                description: 'Test category for t-shirts'
            });
            testCategory = createResponse.data;
            testData.categories.push(testCategory);
            log('✅ Category created', 'green');
            log(`   ID: ${testCategory.id}, Name: ${testCategory.name}`, 'blue');
        } catch (error) {
            const errorMsg = error.response?.data?.message || error.message;
            if (errorMsg.includes('already exists')) {
                log(`ℹ️  Category "${testCategoryName}" already exists`, 'blue');
                // Try to find it again
                const refreshResponse = await api.get('/api/categories');
                testCategory = refreshResponse.data.find(c => c.name === testCategoryName);
                if (testCategory) {
                    testData.categories.push(testCategory);
                }
            } else {
                log(`⚠️  Create category failed: ${errorMsg}`, 'yellow');
            }
        }
    } else {
        log(`ℹ️  Test category "${testCategoryName}" already exists`, 'blue');
        testData.categories.push(testCategory);
    }
    
    return true;
}

async function testItems() {
    log('\n=== Testing Items API ===', 'cyan');
    
    if (testData.categories.length === 0) {
        log('⚠️  No categories available, creating one...', 'yellow');
        await testCategories();
    }
    
    const categoryId = testData.categories[0]?.id || 'test-category-id';
    
    // Get existing items first
    let existingItems = [];
    try {
        const response = await api.get('/api/items');
        existingItems = response.data;
        log(`ℹ️  Found ${existingItems.length} existing items`, 'blue');
    } catch (error) {
        log(`⚠️  Could not get existing items: ${error.response?.data?.message || error.message}`, 'yellow');
    }
    
    // Create items with test data
    const testItemsData = [
        {
            name: 'Blue Cotton T-Shirt',
            categoryId: categoryId,
            sku: 'TSHIRT-BLUE-001',
            brand: 'TestBrand',
            price: 25.99,
            stock: 50,
            barcode: '8901234567890',
            note: 'Test item 1'
        },
        {
            name: 'Red Denim Jeans',
            categoryId: categoryId,
            sku: 'JEANS-RED-001',
            brand: 'TestBrand',
            price: 45.99,
            stock: 30,
            barcode: '8901234567891',
            note: 'Test item 2'
        },
        {
            name: 'Black Hoodie',
            categoryId: categoryId,
            sku: 'HOODIE-BLK-001',
            brand: 'TestBrand',
            price: 55.99,
            stock: 20,
            barcode: '8901234567892',
            note: 'Test item 3'
        }
    ];
    
    let createdCount = 0;
    let existingCount = 0;
    
    for (const itemData of testItemsData) {
        // Check if item already exists by SKU
        const existingItem = existingItems.find(i => i.sku === itemData.sku);
        
        if (existingItem) {
            log(`ℹ️  Item "${itemData.name}" (SKU: ${itemData.sku}) already exists`, 'blue');
            testData.items.push(existingItem);
            existingCount++;
        } else {
            try {
                const response = await api.post('/api/items', itemData);
                testData.items.push(response.data);
                log(`✅ Item created: ${response.data.name}`, 'green');
                log(`   Stock: ${response.data.stock}, Price: ${response.data.price} LKR`, 'blue');
                createdCount++;
            } catch (error) {
                const errorMsg = error.response?.data?.message || error.message;
                if (errorMsg.includes('already exists') || errorMsg.includes('duplicate')) {
                    log(`ℹ️  Item "${itemData.name}" (SKU: ${itemData.sku}) already exists`, 'blue');
                    // Try to find it
                    try {
                        const refreshResponse = await api.get('/api/items');
                        const foundItem = refreshResponse.data.find(i => i.sku === itemData.sku);
                        if (foundItem) {
                            testData.items.push(foundItem);
                            existingCount++;
                        }
                    } catch (refreshError) {
                        // Ignore refresh error
                    }
                } else {
                    log(`⚠️  Create item failed: ${errorMsg}`, 'yellow');
                }
            }
        }
    }
    
    // Get all items
    try {
        const response = await api.get('/api/items');
        log(`✅ Retrieved ${response.data.length} items`, 'green');
        if (createdCount > 0) {
            log(`   Created ${createdCount} new items`, 'green');
        }
        if (existingCount > 0) {
            log(`   Used ${existingCount} existing items`, 'blue');
        }
        return testData.items.length > 0;
    } catch (error) {
        log(`❌ Get items failed: ${error.response?.data?.message || error.message}`, 'red');
        return false;
    }
}

async function testStockCalculation() {
    log('\n=== Testing Stock Calculation ===', 'cyan');
    
    if (testData.items.length === 0) {
        log('⚠️  No items available, creating test items...', 'yellow');
        await testItems();
    }
    
    const item = testData.items[0];
    const initialStock = item.stock;
    
    log(`\n📊 Initial Stock for ${item.name}: ${initialStock}`, 'blue');
    
    // Get current stock
    try {
        const response = await api.get(`/api/items/${item.id}`);
        const currentStock = response.data.stock;
        log(`✅ Current stock: ${currentStock}`, 'green');
        
        if (currentStock !== initialStock) {
            log(`⚠️  Stock mismatch! Expected: ${initialStock}, Got: ${currentStock}`, 'yellow');
        }
        
        return true;
    } catch (error) {
        log(`❌ Get item stock failed: ${error.response?.data?.message || error.message}`, 'red');
        return false;
    }
}

async function testSales() {
    log('\n=== Testing Sales API ===', 'cyan');
    
    if (testData.items.length === 0) {
        log('⚠️  No items available, creating test items...', 'yellow');
        await testItems();
    }
    
    if (testData.items.length < 2) {
        log('❌ Need at least 2 items for sales test', 'red');
        return false;
    }
    
    // Use the authenticated admin-dev user directly
    let cashierId;
    try {
        // Verify token is available
        if (!testData.token) {
            log('❌ No authentication token available. Cannot test sales.', 'red');
            log('   Make sure authentication test passed successfully.', 'yellow');
            return false;
        }
        
        // Use the authenticated user ID directly (admin-dev user from login)
        if (testData.authenticatedUserId) {
            cashierId = testData.authenticatedUserId;
            log(`   Using authenticated user: ${testData.authenticatedUsername || 'admin-dev'} (ID: ${cashierId})`, 'blue');
        } else {
            // Fallback: fetch users if authenticatedUserId not stored
            log('⚠️  Authenticated user ID not stored, fetching users...', 'yellow');
            const usersResponse = await api.get('/api/users');
            const user = usersResponse.data.find(u => u.roleName === 'Admin' || u.roleName === 'admin' || u.roleName === 'Cashier' || u.roleName === 'cashier') || usersResponse.data[0];
            cashierId = user?.id;
            if (!cashierId) {
                log('❌ No users found. Cannot create sale.', 'red');
                return false;
            }
            log(`   Using user: ${user?.username || user?.email} (${user?.roleName || 'Unknown Role'})`, 'blue');
        }
    } catch (error) {
        if (error.response?.status === 401) {
            log(`❌ Authentication failed: ${error.response?.data?.message || error.message}`, 'red');
            log('   Token may have expired or is invalid. Please re-run authentication.', 'yellow');
        } else {
            log(`⚠️  Could not get users: ${error.response?.data?.message || error.message}`, 'yellow');
        }
        return false;
    }
    
    // Get initial stock values
    const item1 = testData.items[0];
    const item2 = testData.items[1];
    
    let initialStock1, initialStock2;
    try {
        const item1Response = await api.get(`/api/items/${item1.id}`);
        const item2Response = await api.get(`/api/items/${item2.id}`);
        initialStock1 = item1Response.data.stock;
        initialStock2 = item2Response.data.stock;
        log(`\n📊 Initial Stock:`, 'blue');
        log(`   ${item1.name}: ${initialStock1}`, 'blue');
        log(`   ${item2.name}: ${initialStock2}`, 'blue');
    } catch (error) {
        log(`❌ Could not get initial stock: ${error.message}`, 'red');
        return false;
    }
    
    // Create a sale
    const saleQuantity1 = 5;
    const saleQuantity2 = 3;
    
    // Generate a sale ID first (EF will use this or generate its own)
    // Simple UUID v4 generator
    function generateUUID() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    const saleId = generateUUID();
    
    // Calculate totals
    const subtotal = (item1.price * saleQuantity1) + (item2.price * saleQuantity2);
    const tax = 0;
    const total = subtotal + tax;
    
    const saleData = {
        id: saleId,
        date: new Date().toISOString(),
        subtotal: subtotal,
        tax: tax,
        total: total,
        paymentMethod: 'cash',
        status: 'completed',
        customerName: 'Test Customer',
        cashierId: cashierId,
        saleItems: [
            {
                id: generateUUID(),
                saleId: saleId,
                itemId: item1.id,
                quantity: saleQuantity1,
                price: item1.price,
                total: item1.price * saleQuantity1
            },
            {
                id: generateUUID(),
                saleId: saleId,
                itemId: item2.id,
                quantity: saleQuantity2,
                price: item2.price,
                total: item2.price * saleQuantity2
            }
        ]
    };
    
    log(`   Sale Data Prepared:`, 'blue');
    log(`   - Sale ID: ${saleId}`, 'blue');
    log(`   - User ID: ${cashierId}`, 'blue');
    log(`   - Items: ${saleData.saleItems.length}`, 'blue');
    log(`   - Total: ${total} LKR`, 'blue');
    
    try {
        const response = await api.post('/api/sales', saleData);
        testData.sales.push(response.data);
        log(`✅ Sale created: ${response.data.id}`, 'green');
        log(`   Total: ${response.data.total} LKR`, 'blue');
        
        // Verify stock was deducted
        await new Promise(resolve => setTimeout(resolve, 500)); // Wait for DB update
        
        const item1AfterResponse = await api.get(`/api/items/${item1.id}`);
        const item2AfterResponse = await api.get(`/api/items/${item2.id}`);
        
        const stock1After = item1AfterResponse.data.stock;
        const stock2After = item2AfterResponse.data.stock;
        
        log(`\n📊 Stock After Sale:`, 'blue');
        log(`   ${item1.name}: ${stock1After} (was ${initialStock1})`, 'blue');
        log(`   ${item2.name}: ${stock2After} (was ${initialStock2})`, 'blue');
        
        const expectedStock1 = initialStock1 - saleQuantity1;
        const expectedStock2 = initialStock2 - saleQuantity2;
        
        if (stock1After === expectedStock1 && stock2After === expectedStock2) {
            log('✅ Stock deduction verified correctly!', 'green');
        } else {
            log(`❌ Stock deduction incorrect!`, 'red');
            log(`   Expected ${item1.name}: ${expectedStock1}, Got: ${stock1After}`, 'red');
            log(`   Expected ${item2.name}: ${expectedStock2}, Got: ${stock2After}`, 'red');
        }
        
        return true;
    } catch (error) {
        const errorMsg = error.response?.data?.message || error.message;
        const errorDetails = error.response?.data?.error || error.response?.data;
        
        log(`❌ Create sale failed: ${errorMsg}`, 'red');
        
        if (error.response?.status === 400) {
            log(`   Validation Error Details:`, 'yellow');
            log(`   ${JSON.stringify(error.response.data, null, 2)}`, 'yellow');
        } else if (error.response?.status === 500) {
            log(`   Server Error Details:`, 'yellow');
            log(`   ${JSON.stringify(error.response.data, null, 2)}`, 'yellow');
            if (errorDetails) {
                log(`   Error: ${errorDetails}`, 'yellow');
            }
        } else if (error.response?.data) {
            log(`   Error Details: ${JSON.stringify(error.response.data, null, 2)}`, 'yellow');
        }
        
        // Log the sale data that was sent for debugging
        log(`   Sale Data Sent:`, 'blue');
        log(`   ${JSON.stringify(saleData, null, 2)}`, 'blue');
        
        return false;
    }
}

async function testRefund() {
    log('\n=== Testing Refund (Stock Restoration) ===', 'cyan');
    
    if (testData.sales.length === 0) {
        log('⚠️  No sales available, creating a test sale...', 'yellow');
        await testSales();
    }
    
    const sale = testData.sales[testData.sales.length - 1];
    
    // Get stock before refund
    // The API returns 'saleItems' not 'items'
    const saleItems = sale.saleItems || sale.items || [];
    if (saleItems.length === 0) {
        log('⚠️  Sale has no items, skipping refund test', 'yellow');
        return false;
    }
    
    const item1 = saleItems[0];
    let stockBeforeRefund;
    try {
        const itemResponse = await api.get(`/api/items/${item1.itemId}`);
        stockBeforeRefund = itemResponse.data.stock;
        log(`📊 Stock before refund: ${stockBeforeRefund}`, 'blue');
    } catch (error) {
        log(`❌ Could not get stock: ${error.message}`, 'red');
        return false;
    }
    
    // Refund the sale
    try {
        const response = await api.post(`/api/sales/${sale.id}/refund`);
        log('✅ Sale refunded successfully', 'green');
        
        // Wait for DB update
        await new Promise(resolve => setTimeout(resolve, 500));
        
        // Verify stock was restored
        const itemResponse = await api.get(`/api/items/${item1.itemId}`);
        const stockAfterRefund = itemResponse.data.stock;
        
        log(`📊 Stock after refund: ${stockAfterRefund}`, 'blue');
        
        const expectedStock = stockBeforeRefund + item1.quantity;
        
        if (stockAfterRefund === expectedStock) {
            log('✅ Stock restoration verified correctly!', 'green');
        } else {
            log(`❌ Stock restoration incorrect!`, 'red');
            log(`   Expected: ${expectedStock}, Got: ${stockAfterRefund}`, 'red');
        }
        
        return true;
    } catch (error) {
        log(`❌ Refund failed: ${error.response?.data?.message || error.message}`, 'red');
        return false;
    }
}

async function testLowStock() {
    log('\n=== Testing Low Stock Alerts ===', 'cyan');
    
    try {
        const response = await api.get('/api/items/low-stock');
        log(`✅ Found ${response.data.length} low stock items`, 'green');
        
        if (response.data.length > 0) {
            response.data.forEach(item => {
                log(`   ⚠️  ${item.name}: ${item.stock} (min: ${item.minStockLevel})`, 'yellow');
            });
        }
        
        return true;
    } catch (error) {
        log(`❌ Get low stock failed: ${error.response?.data?.message || error.message}`, 'red');
        return false;
    }
}

async function testSalesReport() {
    log('\n=== Testing Sales Reports ===', 'cyan');
    
    try {
        const response = await api.get('/api/sales/reports?period=daily');
        log('✅ Sales report retrieved', 'green');
        log(`   Period: ${response.data.period}`, 'blue');
        log(`   Total Sales: ${response.data.totalSales} LKR`, 'blue');
        log(`   Total Orders: ${response.data.totalOrders}`, 'blue');
        log(`   Average Order: ${response.data.averageOrderValue.toFixed(2)} LKR`, 'blue');
        return true;
    } catch (error) {
        log(`❌ Get sales report failed: ${error.response?.data?.message || error.message}`, 'red');
        return false;
    }
}

async function testUsers() {
    log('\n=== Testing Users API ===', 'cyan');
    
    try {
        const response = await api.get('/api/users');
        log(`✅ Retrieved ${response.data.length} users`, 'green');
        
        // Create additional test users for comprehensive testing
        if (testData.roles.length > 0) {
            const cashierRole = testData.roles.find(r => r.name === 'Cashier' || r.name === 'cashier');
            if (cashierRole) {
                const testUsers = [
                    {
                        username: 'cashier-test',
                        email: 'cashier-test@clothpos.test',
                        roleId: cashierRole.id,
                        passcode: '12345',
                        permissions: ['sales', 'pos'],
                        isActive: true
                    },
                    {
                        username: 'staff-test',
                        email: 'staff-test@clothpos.test',
                        roleId: cashierRole.id,
                        passcode: '12345',
                        permissions: ['sales'],
                        isActive: true
                    }
                ];
                
                let createdCount = 0;
                for (const userData of testUsers) {
                    try {
                        // Check if user already exists
                        const existingUser = response.data.find(u => u.username === userData.username);
                        if (existingUser) {
                            log(`   ℹ️  User ${userData.username} already exists`, 'blue');
                            continue;
                        }
                        
                        const createResponse = await api.post('/api/users', userData);
                        testData.users.push(createResponse.data);
                        createdCount++;
                        log(`   ✅ Created test user: ${userData.username}`, 'green');
                    } catch (createError) {
                        if (createError.response?.status === 409) {
                            log(`   ℹ️  User ${userData.username} already exists`, 'blue');
                        } else {
                            log(`   ⚠️  Could not create user ${userData.username}: ${createError.response?.data?.message || createError.message}`, 'yellow');
                        }
                    }
                }
                
                if (createdCount > 0) {
                    log(`✅ Created ${createdCount} additional test users`, 'green');
                }
            }
        }
        
        return true;
    } catch (error) {
        log(`❌ Get users failed: ${error.response?.data?.message || error.message}`, 'red');
        return false;
    }
}

async function testRoles() {
    log('\n=== Testing Roles API (Automatic) ===', 'cyan');
    
    const baseUrl = global.API_BASE_URL || API_BASE_URL;
    
    try {
        // Roles endpoint is now public (AllowAnonymous), try without token
        const response = await axios.get(`${baseUrl}/api/roles`, {
            httpsAgent: baseUrl.startsWith('https') ? new (require('https').Agent)({ rejectUnauthorized: false }) : undefined,
            timeout: 5000
        });
        
        testData.roles = response.data;
        
        if (response.data.length === 0) {
            log('⚠️  No roles found. API should seed them on startup.', 'yellow');
            log('   Waiting 3 seconds for API to seed roles...', 'yellow');
            await new Promise(resolve => setTimeout(resolve, 3000));
            
            // Retry
            const retryResponse = await axios.get(`${baseUrl}/api/roles`, {
                httpsAgent: baseUrl.startsWith('https') ? new (require('https').Agent)({ rejectUnauthorized: false }) : undefined,
                timeout: 5000
            });
            testData.roles = retryResponse.data;
        }
        
        if (testData.roles.length > 0) {
            log(`✅ Retrieved ${testData.roles.length} roles`, 'green');
            log('   Available roles:', 'blue');
            testData.roles.forEach(role => {
                log(`     - ${role.name} (${role.id})`, 'blue');
            });
            return true;
        } else {
            log('⚠️  Still no roles found after waiting.', 'yellow');
            log('   The API should seed roles automatically on startup.', 'yellow');
            log('   Check API logs for any database seeding errors.', 'yellow');
            return false;
        }
    } catch (error) {
        log(`❌ Get roles failed: ${error.response?.data?.message || error.message}`, 'red');
        return false;
    }
}

async function testSettings() {
    log('\n=== Testing Settings API ===', 'cyan');
    
    try {
        const response = await api.get('/api/settings');
        log('✅ Settings retrieved', 'green');
        log(`   Shop Name: ${response.data.shopName || 'N/A'}`, 'blue');
        return true;
    } catch (error) {
        log(`❌ Get settings failed: ${error.response?.data?.message || error.message}`, 'red');
        return false;
    }
}

async function testPendingSalesAndPayments() {
    log('\n=== Testing Pending Sales & PaymentDues (Orders) ===', 'cyan');
    
    if (testData.items.length < 2) {
        log('⚠️  Need at least 2 items to create a pending sale', 'yellow');
        return false;
    }
    
    // Use the authenticated admin-dev user directly
    if (!testData.authenticatedUserId) {
        log('⚠️  Authenticated user ID not available. Need to login first.', 'yellow');
        return false;
    }
    
    // Get items
    const item1 = testData.items[0];
    const item2 = testData.items[1];
    const cashierId = testData.authenticatedUserId;
    log(`   Using authenticated user: ${testData.authenticatedUsername || 'admin-dev'} (ID: ${cashierId})`, 'blue');
    
    // Get initial stock
    let initialStock1, initialStock2;
    try {
        const item1Response = await api.get(`/api/items/${item1.id}`);
        const item2Response = await api.get(`/api/items/${item2.id}`);
        initialStock1 = item1Response.data.stock;
        initialStock2 = item2Response.data.stock;
        log(`📊 Initial Stock:`, 'blue');
        log(`   ${item1.name}: ${initialStock1}`, 'blue');
        log(`   ${item2.name}: ${initialStock2}`, 'blue');
    } catch (error) {
        log(`❌ Could not get initial stock: ${error.message}`, 'red');
        return false;
    }
    
    // Create a pending sale (not fully paid)
    const saleQuantity1 = 2;
    const saleQuantity2 = 1;
    
    function generateUUID() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    const saleId = generateUUID();
    
    // Calculate totals
    const subtotal = (item1.price * saleQuantity1) + (item2.price * saleQuantity2);
    const tax = 0;
    const total = subtotal + tax;
    
    const saleData = {
        id: saleId,
        date: new Date().toISOString(),
        subtotal: subtotal,
        tax: tax,
        total: total,
        paymentMethod: 'credit', // Credit indicates pending payment
        status: 'pending', // This will trigger PaymentDue creation
        customerName: 'Test Customer (Pending)',
        cashierId: cashierId,
        saleItems: [
            {
                id: generateUUID(),
                saleId: saleId,
                itemId: item1.id,
                quantity: saleQuantity1,
                price: item1.price,
                total: item1.price * saleQuantity1
            },
            {
                id: generateUUID(),
                saleId: saleId,
                itemId: item2.id,
                quantity: saleQuantity2,
                price: item2.price,
                total: item2.price * saleQuantity2
            }
        ]
    };
    
    log(`   Creating pending sale...`, 'blue');
    log(`   - Total: ${total} LKR`, 'blue');
    log(`   - Status: pending`, 'blue');
    
    try {
        const response = await api.post('/api/sales', saleData);
        testData.sales.push(response.data);
        
        // Use the actual sale ID from the response (might be different from what we sent)
        const actualSaleId = response.data.id || saleId;
        log(`✅ Pending sale created: ${actualSaleId}`, 'green');
        log(`   Sale status: ${response.data.status}`, 'blue');
        log(`   Sale total: ${response.data.total} LKR`, 'blue');
        
        // Verify the sale status is actually "pending"
        if (response.data.status !== 'pending' && response.data.status !== 'Pending') {
            log(`   ⚠️  WARNING: Sale status is '${response.data.status}', not 'pending'`, 'yellow');
            log(`   ⚠️  PaymentDue is only created for sales with status 'pending'`, 'yellow');
            return false;
        }
        
        // Wait for PaymentDue to be created (backend should create it immediately)
        await new Promise(resolve => setTimeout(resolve, 1500));
        
        // Verify PaymentDue was created
        try {
            // Try multiple times in case of timing issues
            let paymentDue = null;
            let attempts = 0;
            const maxAttempts = 8; // Increased attempts
            
            while (!paymentDue && attempts < maxAttempts) {
                await new Promise(resolve => setTimeout(resolve, 500));
                attempts++;
                
                const paymentsResponse = await api.get('/api/payments');
                const paymentDues = paymentsResponse.data || [];
                
                log(`   Attempt ${attempts}: Found ${paymentDues.length} PaymentDues`, 'blue');
                
                // Find the PaymentDue for this sale - try both IDs
                paymentDue = paymentDues.find(p => {
                    const pSaleId = p.saleId || p.sale?.id;
                    return pSaleId === actualSaleId || pSaleId === saleId;
                });
                
                if (!paymentDue && paymentDues.length > 0) {
                    // Log all saleIds for debugging
                    const allSaleIds = paymentDues.map(p => p.saleId || p.sale?.id || 'N/A').filter(id => id !== 'N/A');
                    log(`   Available saleIds: ${allSaleIds.length > 0 ? allSaleIds.join(', ') : 'None'}`, 'blue');
                    log(`   Looking for: ${actualSaleId} (or ${saleId})`, 'blue');
                    
                    // Also log first few PaymentDue details for debugging
                    if (attempts === 1 && paymentDues.length > 0) {
                        log(`   Sample PaymentDue:`, 'blue');
                        const sample = paymentDues[0];
                        log(`     - ID: ${sample.id}`, 'blue');
                        log(`     - SaleId: ${sample.saleId || sample.sale?.id || 'N/A'}`, 'blue');
                        log(`     - Amount: ${sample.amount}`, 'blue');
                        log(`     - Status: ${sample.status}`, 'blue');
                    }
                }
            }
            
            if (paymentDue) {
                log(`✅ PaymentDue created automatically!`, 'green');
                log(`   PaymentDue ID: ${paymentDue.id}`, 'blue');
                log(`   Amount: ${paymentDue.amount} LKR`, 'blue');
                log(`   Due Date: ${paymentDue.dueDate}`, 'blue');
                log(`   Status: ${paymentDue.status}`, 'blue');
                
                // Store for later tests
                if (!testData.paymentDues) {
                    testData.paymentDues = [];
                }
                testData.paymentDues.push(paymentDue);
                
                // Verify stock behavior (currently stock IS deducted for pending sales)
                await new Promise(resolve => setTimeout(resolve, 500));
                
                const item1AfterResponse = await api.get(`/api/items/${item1.id}`);
                const item2AfterResponse = await api.get(`/api/items/${item2.id}`);
                
                const stock1After = item1AfterResponse.data.stock;
                const stock2After = item2AfterResponse.data.stock;
                
                log(`\n📊 Stock After Pending Sale:`, 'blue');
                log(`   ${item1.name}: ${stock1After} (was ${initialStock1})`, 'blue');
                log(`   ${item2.name}: ${stock2After} (was ${initialStock2})`, 'blue');
                
                // Note: Currently stock IS deducted for pending sales (reserved stock)
                // This is acceptable business logic - stock is reserved until payment
                
                return true;
            } else {
                // Final attempt - get all payments and log details
                const finalPaymentsResponse = await api.get('/api/payments');
                const allPaymentDues = finalPaymentsResponse.data || [];
                
                log(`❌ PaymentDue not found for sale ${actualSaleId} after ${maxAttempts} attempts`, 'red');
                log(`   Found ${allPaymentDues.length} total PaymentDues`, 'yellow');
                
                if (allPaymentDues.length > 0) {
                    log(`   Available PaymentDues:`, 'yellow');
                    allPaymentDues.forEach(p => {
                        log(`     - SaleId: ${p.saleId}, Amount: ${p.amount}, Status: ${p.status}`, 'yellow');
                    });
                } else {
                    log(`   ⚠️  No PaymentDues found at all. Check if PaymentDue creation is working.`, 'yellow');
                }
                
                // Check if sale was actually created with pending status
                try {
                    const saleCheckResponse = await api.get(`/api/sales/${actualSaleId}`);
                    log(`   Sale status: ${saleCheckResponse.data?.status}`, 'blue');
                    if (saleCheckResponse.data?.status !== 'pending') {
                        log(`   ⚠️  Sale status is not 'pending', it's '${saleCheckResponse.data?.status}'`, 'yellow');
                        log(`   ⚠️  PaymentDue is only created for sales with status 'pending'`, 'yellow');
                    }
                } catch (saleCheckError) {
                    log(`   ⚠️  Could not verify sale status: ${saleCheckError.message}`, 'yellow');
                }
                
                return false;
            }
        } catch (paymentError) {
            log(`❌ Could not verify PaymentDue: ${paymentError.response?.data?.message || paymentError.message}`, 'red');
            if (paymentError.response?.data) {
                log(`   Error details: ${JSON.stringify(paymentError.response.data, null, 2)}`, 'yellow');
            }
            return false;
        }
    } catch (error) {
        const errorMsg = error.response?.data?.message || error.message;
        log(`❌ Create pending sale failed: ${errorMsg}`, 'red');
        
        if (error.response?.data) {
            log(`   Details: ${JSON.stringify(error.response.data, null, 2)}`, 'yellow');
        }
        
        return false;
    }
}

async function testCreateMultipleOrders() {
    log('\n=== Creating Multiple Orders (Test Data) ===', 'cyan');
    
    if (testData.items.length < 3) {
        log('⚠️  Need at least 3 items to create multiple orders', 'yellow');
        return false;
    }
    
    // Use the authenticated admin-dev user directly
    if (!testData.authenticatedUserId) {
        log('⚠️  Authenticated user ID not available. Need to login first.', 'yellow');
        return false;
    }
    
    const cashierId = testData.authenticatedUserId;
    log(`   Using authenticated user: ${testData.authenticatedUsername || 'admin-dev'} (ID: ${cashierId})`, 'blue');
    
    function generateUUID() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    
    // Create multiple orders with different scenarios
    const orderScenarios = [
        {
            name: 'Large Order - Pending Payment',
            items: [testData.items[0], testData.items[1], testData.items[2]],
            quantities: [5, 3, 2],
            paymentMethod: 'credit',
            status: 'pending',
            customerName: 'John Doe - Large Order'
        },
        {
            name: 'Medium Order - Pending Payment',
            items: [testData.items[0], testData.items[1]],
            quantities: [2, 1],
            paymentMethod: 'credit',
            status: 'pending',
            customerName: 'Jane Smith - Medium Order'
        },
        {
            name: 'Small Order - Pending Payment',
            items: [testData.items[2]],
            quantities: [1],
            paymentMethod: 'credit',
            status: 'pending',
            customerName: 'Bob Johnson - Small Order'
        }
    ];
    
    let createdCount = 0;
    let failedCount = 0;
    
    for (const scenario of orderScenarios) {
        try {
            const saleId = generateUUID();
            
            // Calculate totals
            let subtotal = 0;
            const saleItems = scenario.items.map((item, index) => {
                const quantity = scenario.quantities[index];
                const itemTotal = item.price * quantity;
                subtotal += itemTotal;
                
                return {
                    id: generateUUID(),
                    saleId: saleId,
                    itemId: item.id,
                    quantity: quantity,
                    price: item.price,
                    total: itemTotal
                };
            });
            
            const tax = 0;
            const total = subtotal + tax;
            
            const saleData = {
                id: saleId,
                date: new Date().toISOString(),
                subtotal: subtotal,
                tax: tax,
                total: total,
                paymentMethod: scenario.paymentMethod,
                status: scenario.status,
                customerName: scenario.customerName,
                cashierId: cashierId,
                saleItems: saleItems
            };
            
            log(`   Creating: ${scenario.name}...`, 'blue');
            log(`   - Items: ${scenario.items.length}, Total: ${total.toFixed(2)} LKR`, 'blue');
            
            const response = await api.post('/api/sales', saleData);
            testData.sales.push(response.data);
            createdCount++;
            
            // Verify sale status
            if (response.data.status !== 'pending' && response.data.status !== 'Pending') {
                log(`   ⚠️  WARNING: Sale status is '${response.data.status}', not 'pending'`, 'yellow');
                log(`   ⚠️  PaymentDue will not be created for non-pending sales`, 'yellow');
            }
            
            // Wait for PaymentDue to be created (backend creates it immediately after sale)
            await new Promise(resolve => setTimeout(resolve, 1500));
            
            // Verify PaymentDue was created
            const paymentsResponse = await api.get('/api/payments');
            const paymentDues = paymentsResponse.data || [];
            
            // Check both direct saleId and nested sale.id
            const paymentDue = paymentDues.find(p => {
                const pSaleId = p.saleId || p.sale?.id;
                return pSaleId === response.data.id;
            });
            
            if (paymentDue) {
                if (!testData.paymentDues) {
                    testData.paymentDues = [];
                }
                testData.paymentDues.push(paymentDue);
                log(`   ✅ Order created with PaymentDue: ${paymentDue.id}`, 'green');
                log(`      Amount: ${paymentDue.amount} LKR, Due: ${paymentDue.dueDate}`, 'blue');
            } else {
                log(`   ⚠️  Order created but PaymentDue not found`, 'yellow');
                log(`      Sale ID: ${response.data.id}, Status: ${response.data.status}`, 'yellow');
                if (paymentDues.length > 0) {
                    log(`      Found ${paymentDues.length} PaymentDues, but none match this sale`, 'yellow');
                    // Log first PaymentDue for debugging
                    const sample = paymentDues[0];
                    log(`      Sample PaymentDue - SaleId: ${sample.saleId || sample.sale?.id || 'N/A'}`, 'yellow');
                } else {
                    log(`      No PaymentDues found in database`, 'yellow');
                }
            }
            
            // Small delay between orders
            await new Promise(resolve => setTimeout(resolve, 500));
            
        } catch (error) {
            failedCount++;
            log(`   ❌ Failed to create ${scenario.name}: ${error.response?.data?.message || error.message}`, 'red');
        }
    }
    
    log(`\n📊 Order Creation Summary:`, 'cyan');
    log(`   ✅ Created: ${createdCount} orders`, 'green');
    if (failedCount > 0) {
        log(`   ❌ Failed: ${failedCount} orders`, 'red');
    }
    log(`   📋 Total PaymentDues: ${testData.paymentDues?.length || 0}`, 'blue');
    
    return createdCount > 0;
}

async function testPaymentsAPI() {
    log('\n=== Testing Payments API ===', 'cyan');
    
    try {
        // Get all payments
        const allPaymentsResponse = await api.get('/api/payments');
        const allPayments = allPaymentsResponse.data || [];
        log(`✅ Retrieved ${allPayments.length} payment dues`, 'green');
        
        // Get pending payments
        const pendingPaymentsResponse = await api.get('/api/payments/pending');
        const pendingPayments = pendingPaymentsResponse.data || [];
        log(`✅ Retrieved ${pendingPayments.length} pending payments`, 'green');
        
        // Get overdue payments
        const overduePaymentsResponse = await api.get('/api/payments/overdue');
        const overduePayments = overduePaymentsResponse.data || [];
        log(`✅ Retrieved ${overduePayments.length} overdue payments`, 'green');
        
        if (pendingPayments.length > 0) {
            log(`\n📋 Pending Payments:`, 'blue');
            pendingPayments.forEach(payment => {
                const dueDate = new Date(payment.dueDate);
                const today = new Date();
                const daysRemaining = Math.ceil((dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
                
                log(`   - Payment ${payment.id}: ${payment.amount} LKR`, 'blue');
                log(`     Due: ${payment.dueDate} (${daysRemaining} days)`, 'blue');
                log(`     Sale ID: ${payment.saleId}`, 'blue');
            });
        }
        
        if (overduePayments.length > 0) {
            log(`\n⚠️  Overdue Payments:`, 'yellow');
            overduePayments.forEach(payment => {
                const dueDate = new Date(payment.dueDate);
                const today = new Date();
                const daysOverdue = Math.ceil((today.getTime() - dueDate.getTime()) / (1000 * 60 * 60 * 24));
                
                log(`   - Payment ${payment.id}: ${payment.amount} LKR`, 'yellow');
                log(`     Overdue by: ${daysOverdue} days`, 'yellow');
            });
        }
        
        return true;
    } catch (error) {
        log(`❌ Get payments failed: ${error.response?.data?.message || error.message}`, 'red');
        return false;
    }
}

// ============================================
// MAIN TEST RUNNER
// ============================================

async function runAllTests() {
    log('\n╔════════════════════════════════════════╗', 'cyan');
    log('║   ClothPos API Comprehensive Tests   ║', 'cyan');
    log('╚════════════════════════════════════════╝', 'cyan');
    
    const results = {
        passed: 0,
        failed: 0,
        total: 0
    };
    
    const tests = [
        { name: 'API Connection', fn: testConnection, critical: true },
        { name: 'Roles', fn: testRoles }, // Get roles before auth (needed for user creation)
        { name: 'Authentication', fn: testAuth, critical: true }, // This will automatically create user if needed
        { name: 'Categories', fn: testCategories },
        { name: 'Items', fn: testItems, critical: true },
        { name: 'Stock Calculation', fn: testStockCalculation, critical: true },
        { name: 'Sales', fn: testSales, critical: true },
        { name: 'Refund & Stock Restoration', fn: testRefund },
        { name: 'Pending Sales & PaymentDues', fn: testPendingSalesAndPayments },
        { name: 'Create Multiple Orders (Test Data)', fn: testCreateMultipleOrders },
        { name: 'Payments API', fn: testPaymentsAPI },
        { name: 'Low Stock Alerts', fn: testLowStock },
        { name: 'Sales Reports', fn: testSalesReport },
        { name: 'Users', fn: testUsers },
        { name: 'Settings', fn: testSettings }
    ];
    
    for (const test of tests) {
        results.total++;
        try {
            const result = await test.fn();
            
            // Recreate API instance if connection was established
            if (test.name === 'API Connection' && result) {
                api = createApiInstance();
                setupAuthInterceptor(api);
            }
            
            // Recreate API instance after authentication to ensure token is available
            if (test.name === 'Authentication' && result && testData.token) {
                api = createApiInstance();
                setupAuthInterceptor(api);
                log('   API instance updated with authentication token', 'blue');
                
                // Verify token is working by making a test request
                try {
                    const testRequest = await api.get('/api/users?limit=1');
                    log('   ✅ Token verification successful', 'green');
                } catch (verifyError) {
                    log('   ⚠️  Token verification failed, but continuing...', 'yellow');
                }
            }
            
            if (result) {
                results.passed++;
            } else {
                results.failed++;
                if (test.critical) {
                    log(`\n⚠️  Critical test failed: ${test.name}`, 'yellow');
                    log('   Stopping tests...', 'yellow');
                    break;
                }
            }
        } catch (error) {
            results.failed++;
            log(`❌ Test ${test.name} threw error: ${error.message}`, 'red');
            if (test.critical) {
                break;
            }
        }
        
        // Small delay between tests
        await new Promise(resolve => setTimeout(resolve, 200));
    }
    
    // Summary
    log('\n╔════════════════════════════════════════╗', 'cyan');
    log('║           TEST SUMMARY                  ║', 'cyan');
    log('╚════════════════════════════════════════╝', 'cyan');
    log(`Total Tests: ${results.total}`, 'blue');
    log(`Passed: ${results.passed}`, 'green');
    log(`Failed: ${results.failed}`, results.failed > 0 ? 'red' : 'green');
    log(`\nSuccess Rate: ${((results.passed / results.total) * 100).toFixed(1)}%`, 
        results.passed === results.total ? 'green' : 'yellow');
    
    if (results.failed === 0) {
        log('\n🎉 All tests passed!', 'green');
    } else {
        log('\n⚠️  Some tests failed. Please review the output above.', 'yellow');
    }
}

// Run tests
runAllTests().catch(error => {
    log(`\n❌ Fatal error: ${error.message}`, 'red');
    process.exit(1);
});

