# How to Check MySQL User and Password

## Step 1: Connect to MySQL as Root

```bash
# SSH into your Ubuntu server
ssh root@your_server_ip

# Connect to MySQL as root
sudo mysql -u root -p
# Enter your root password when prompted
```

## Step 2: Check All MySQL Users

```sql
-- List all users and their hosts
SELECT User, Host FROM mysql.user;

-- List users with more details
SELECT User, Host, plugin, authentication_string FROM mysql.user;

-- Check specific user (clothpos_user)
SELECT User, Host, plugin FROM mysql.user WHERE User = 'clothpos_user';
```

## Step 3: Check User Privileges

```sql
-- Check privileges for clothpos_user on localhost
SHOW GRANTS FOR 'clothpos_user'@'localhost';

-- Check privileges for clothpos_user from any host
SHOW GRANTS FOR 'clothpos_user'@'%';

-- Check all grants for a user
SHOW GRANTS FOR 'clothpos_user'@'localhost';
SHOW GRANTS FOR 'clothpos_user'@'%';
```

## Step 4: Test Current Password

**Note:** MySQL stores passwords as hashes, so you cannot "see" the actual password. You can only test if a password works.

### Test from command line:

```bash
# Test connection with password
mysql -u clothpos_user -p ClothPosDB
# Enter the password when prompted
# If it connects successfully, the password is correct
```

### Test from MySQL prompt:

```sql
-- Connect as root first
sudo mysql -u root -p

-- Try to authenticate the user (this doesn't directly test password, but you can check if user exists)
SELECT User, Host, plugin FROM mysql.user WHERE User = 'clothpos_user';
```

## Step 5: Check Current User's Password Hash (for verification)

```sql
-- Connect as root
sudo mysql -u root -p

-- Check password hash (you can't see actual password, only the hash)
SELECT User, Host, authentication_string FROM mysql.user WHERE User = 'clothpos_user';
```

**Note:** The `authentication_string` is a hash, not the actual password. You cannot reverse it to get the original password.

## Step 6: Reset/Change Password (if needed)

If you need to change the password:

```sql
-- Connect as root
sudo mysql -u root -p

-- Change password for localhost
ALTER USER 'clothpos_user'@'localhost' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';

-- Change password for remote connections
ALTER USER 'clothpos_user'@'%' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';

-- Apply changes
FLUSH PRIVILEGES;
```

## Step 7: Verify User Can Connect

```bash
# Exit MySQL
EXIT;

# Test connection with new password
mysql -u clothpos_user -p ClothPosDB
# Enter: Cl0thP0s@2024#Secure!
```

## Quick Check Commands

### Check if user exists:
```sql
SELECT COUNT(*) FROM mysql.user WHERE User = 'clothpos_user';
```

### Check user hosts:
```sql
SELECT User, Host FROM mysql.user WHERE User = 'clothpos_user';
```

### Check user privileges:
```sql
SHOW GRANTS FOR 'clothpos_user'@'localhost';
```

### Check current database user:
```sql
SELECT USER(), CURRENT_USER();
```

## Complete Check Script

Run this complete script to check everything:

```sql
-- Connect as root first: sudo mysql -u root -p

-- Check if user exists
SELECT 
    'User Check' AS CheckType,
    User,
    Host,
    CASE 
        WHEN User = 'clothpos_user' THEN 'User exists ✓'
        ELSE 'User not found ✗'
    END AS Status
FROM mysql.user 
WHERE User = 'clothpos_user';

-- Check user privileges
SELECT 
    'Privileges Check' AS CheckType,
    CONCAT(User, '@', Host) AS UserHost,
    'Check grants below' AS Status
FROM mysql.user 
WHERE User = 'clothpos_user';

-- Show actual grants
SHOW GRANTS FOR 'clothpos_user'@'localhost';
SHOW GRANTS FOR 'clothpos_user'@'%';

-- Check if database exists
SELECT 
    'Database Check' AS CheckType,
    SCHEMA_NAME AS DatabaseName,
    CASE 
        WHEN SCHEMA_NAME = 'ClothPosDB' THEN 'Database exists ✓'
        ELSE 'Database not found ✗'
    END AS Status
FROM INFORMATION_SCHEMA.SCHEMATA 
WHERE SCHEMA_NAME = 'ClothPosDB';
```

## From Command Line (One-liners)

```bash
# Check if user exists
sudo mysql -u root -p -e "SELECT User, Host FROM mysql.user WHERE User = 'clothpos_user';"

# Check user privileges
sudo mysql -u root -p -e "SHOW GRANTS FOR 'clothpos_user'@'localhost';"

# Check if database exists
sudo mysql -u root -p -e "SHOW DATABASES LIKE 'ClothPosDB';"

# Test user connection (will prompt for password)
mysql -u clothpos_user -p -e "SELECT DATABASE(), USER();"
```

## Current Configuration (from appsettings.json)

Based on your current setup, the expected configuration is:

- **Username**: `clothpos_user`
- **Password**: `Cl0thP0s@2024#Secure!`
- **Database**: `ClothPosDB`
- **Host**: Your DigitalOcean server IP

## Verify Configuration Matches

1. **Check user exists:**
   ```sql
   SELECT User, Host FROM mysql.user WHERE User = 'clothpos_user';
   ```
   Should show:
   - `clothpos_user` @ `localhost`
   - `clothpos_user` @ `%` (if remote access is enabled)

2. **Test password:**
   ```bash
   mysql -u clothpos_user -p ClothPosDB
   # Enter: Cl0thP0s@2024#Secure!
   ```

3. **Check database access:**
   ```sql
   USE ClothPosDB;
   SHOW TABLES;
   ```

## Troubleshooting

### User doesn't exist:
```sql
CREATE USER 'clothpos_user'@'localhost' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';
CREATE USER 'clothpos_user'@'%' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';
GRANT ALL PRIVILEGES ON ClothPosDB.* TO 'clothpos_user'@'localhost';
GRANT ALL PRIVILEGES ON ClothPosDB.* TO 'clothpos_user'@'%';
FLUSH PRIVILEGES;
```

### Password doesn't work:
```sql
ALTER USER 'clothpos_user'@'localhost' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';
ALTER USER 'clothpos_user'@'%' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';
FLUSH PRIVILEGES;
```

### User exists but can't access database:
```sql
GRANT ALL PRIVILEGES ON ClothPosDB.* TO 'clothpos_user'@'localhost';
GRANT ALL PRIVILEGES ON ClothPosDB.* TO 'clothpos_user'@'%';
FLUSH PRIVILEGES;
```

