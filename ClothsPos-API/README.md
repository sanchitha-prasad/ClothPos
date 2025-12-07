# ClothPos .NET API

This is the backend API for the ClothPos Web Portal application.

## Features

- **Authentication**: JWT-based authentication
- **Items Management**: CRUD operations for products/items
- **Categories Management**: CRUD operations for product categories
- **User Management**: CRUD operations for shop staff
- **Sales Management**: Track sales and generate reports
- **Payment Management**: Track pending payments and dues
- **Settings Management**: Shop settings including currency configuration

## Tech Stack

- .NET 8.0
- Entity Framework Core
- SQLite (can be changed to SQL Server)
- JWT Authentication
- Swagger/OpenAPI

## Project Name

This project is part of **ClothPos** - a modern POS system for clothing shops.

## Setup Instructions

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Installation

1. Navigate to the API directory:
```bash
cd ClothsPos-API
```

2. Restore packages:
```bash
dotnet restore
```

3. Update database connection string in `appsettings.json` if needed (default uses SQLite)

4. Run the application:
```bash
dotnet run
```

5. The API will be available at:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`
   - Swagger UI: `http://localhost:5000/swagger`

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login and get JWT token

### Items
- `GET /api/items` - Get all items (with search, category filter)
- `GET /api/items/{id}` - Get item by ID
- `POST /api/items` - Create new item
- `PUT /api/items/{id}` - Update item
- `DELETE /api/items/{id}` - Delete item
- `GET /api/items/low-stock` - Get low stock items
- `POST /api/items/images` - Upload item images

### Categories
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

### Users
- `GET /api/users` - Get all users (requires authentication)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `POST /api/users/{id}/password` - Change password

### Sales
- `GET /api/sales` - Get all sales (with date filters)
- `GET /api/sales/{id}` - Get sale by ID
- `POST /api/sales` - Create sale
- `GET /api/sales/reports` - Get sales reports

### Payments
- `GET /api/payments` - Get all payments
- `GET /api/payments/pending` - Get pending payments
- `GET /api/payments/overdue` - Get overdue payments
- `POST /api/payments/{id}/paid` - Mark payment as paid

### Settings
- `GET /api/settings` - Get shop settings
- `PUT /api/settings` - Update shop settings

## Database

The application uses SQLite by default. To change to SQL Server:

1. Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClothsPosDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

2. Update `Program.cs`:
```csharp
options.UseSqlServer(connectionString);
```

## Default Admin User

The admin user is automatically created in the database on first run. Default credentials:

- **Email/Username**: `admin@shop.com`
- **Password**: `Admin123!`

**To change default credentials**, update `appsettings.json`:
```json
{
  "AdminUser": {
    "Email": "your-admin@email.com",
    "Password": "YourSecurePassword123!",
    "Name": "Your Admin Name"
  }
}
```

Or set environment variables:
- `ADMIN_EMAIL`
- `ADMIN_PASSWORD`
- `ADMIN_NAME`

## CORS Configuration

The API is configured to allow requests from:
- `http://localhost:3000`
- `http://localhost:5173`

Update CORS settings in `Program.cs` if needed.

## Environment Variables

Set these environment variables or update `appsettings.json`:

- `Jwt:Key` - JWT signing key (minimum 32 characters)
- `Jwt:Issuer` - JWT issuer
- `Jwt:Audience` - JWT audience
- `ConnectionStrings:DefaultConnection` - Database connection string

## Frontend Integration

Update your React app's `.env` file:
```
VITE_API_BASE_URL=http://localhost:5000/api
```

Or update `src/services/api.ts`:
```typescript
const API_BASE_URL = 'http://localhost:5000/api'
```


