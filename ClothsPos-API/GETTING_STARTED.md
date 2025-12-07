# Getting Started with ClothPos .NET API

## Quick Start Guide

### 1. Prerequisites
- Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022, VS Code, or any code editor

### 2. Setup Steps

```bash
# Navigate to API directory
cd ClothsPos-API

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run the API
dotnet run
```

### 3. Access Points

- **API Base URL**: `http://localhost:5000/api`
- **Swagger UI**: `http://localhost:5000/swagger`
- **Health Check**: `http://localhost:5000/api/health` (if implemented)

### 4. Test Login

The admin user is automatically created in the database on first run. Default credentials:
- **Email/Username**: `admin@shop.com`
- **Password**: `Admin123!`

**Note**: You can change these in `appsettings.json` under `AdminUser` section.

### 5. Frontend Integration

1. Update your React app's `.env` file:
```
VITE_API_BASE_URL=http://localhost:5000/api
```

2. Or update `src/services/api.ts`:
```typescript
const API_BASE_URL = 'http://localhost:5000/api'
```

### 6. Database

The API uses SQLite by default. The database file `ClothPos.db` will be created automatically on first run.

To use SQL Server instead:
1. Update `appsettings.json` connection string
2. Change `UseSqlite` to `UseSqlServer` in `Program.cs`
3. Run migrations: `dotnet ef migrations add InitialCreate`
4. Update database: `dotnet ef database update`

### 7. Image Uploads

Images are stored in `wwwroot/uploads/items/{itemId}/`
Make sure the `wwwroot` folder exists and has write permissions.

### 8. Troubleshooting

**Port already in use?**
- Change the port in `Properties/launchSettings.json`
- Or use: `dotnet run --urls "http://localhost:5001"`

**CORS errors?**
- Update CORS origins in `Program.cs` to match your frontend URL

**Database errors?**
- Delete `ClothsPos.db` and restart the API to recreate it
- Check connection string in `appsettings.json`

### 9. API Testing

Use Swagger UI at `http://localhost:5000/swagger` to test all endpoints interactively.

### 10. Production Deployment

1. Update `appsettings.json` with production connection string
2. Set strong JWT key in environment variables
3. Configure HTTPS
4. Set up proper CORS for production domain
5. Enable authentication on all protected endpoints


