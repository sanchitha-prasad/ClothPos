# Troubleshooting Guide

## Common Errors and Solutions

### 1. Build Errors

**Error: `UseSqlite` not found**
- **Solution**: Added `Microsoft.EntityFrameworkCore.Sqlite` package
- Run: `dotnet restore`

**Error: File locked during build**
- **Solution**: Stop the running API process first
- Close any IDE or terminal running the API
- Then rebuild: `dotnet build`

### 2. Runtime Errors

**Error: Database not found**
- **Solution**: The database is created automatically on first run
- Delete `ClothPos.db` if corrupted and restart

**Error: Port already in use**
- **Solution**: Change port in `Properties/launchSettings.json`
- Or use: `dotnet run --urls "http://localhost:5001"`

**Error: CORS errors in browser**
- **Solution**: Update CORS origins in `Program.cs` to match your frontend URL
- Default allows: `http://localhost:3000` and `http://localhost:5173`

### 3. Authentication Errors

**Error: Login fails**
- **Solution**: Use default admin credentials:
  - Username: `admin-dev`
  - Password: `12345`
- Check JWT configuration in `appsettings.json`

**Error: Token validation fails**
- **Solution**: Ensure JWT key is at least 32 characters
- Check `Jwt:Key` in `appsettings.json`

### 4. Image Upload Errors

**Error: Images not uploading**
- **Solution**: Ensure `wwwroot` folder exists
- Check write permissions on `wwwroot/uploads/` directory
- Verify file size limits (default is 30MB)

**Error: Image URLs not accessible**
- **Solution**: Ensure `app.UseStaticFiles()` is in `Program.cs`
- Check that images are saved to `wwwroot/uploads/items/{itemId}/`

### 5. Database Errors

**Error: Migration errors**
- **Solution**: Delete `ClothsPos.db` and restart
- Database is auto-created with `EnsureCreated()`

**Error: Seed data not loading**
- **Solution**: Check `ApplicationDbContext.cs` seed method
- Ensure database is recreated if needed

### 6. Service Registration Errors

**Error: Service not registered**
- **Solution**: Check `Program.cs` for service registration
- All services should be registered in `builder.Services.AddScoped<>()`

### 7. DTO Validation Errors

**Error: Model validation fails**
- **Solution**: Check DTO validation attributes
- Ensure required fields are provided
- Check error messages in response

## Quick Fixes

1. **Clean and rebuild**:
```bash
dotnet clean
dotnet restore
dotnet build
```

2. **Reset database**:
```bash
# Delete the database file
rm ClothsPos.db
# Restart API
dotnet run
```

3. **Check logs**:
- Check console output for detailed error messages
- Enable detailed logging in `appsettings.Development.json`

4. **Verify dependencies**:
```bash
dotnet restore
dotnet list package
```

## Still Having Issues?

1. Check the console output for specific error messages
2. Verify all NuGet packages are installed: `dotnet restore`
3. Ensure .NET 8.0 SDK is installed: `dotnet --version`
4. Check file permissions on `wwwroot` folder
5. Verify CORS settings match your frontend URL


