#!/bin/bash
# Build and Publish ClothPos API
# Run on your server: sudo bash build-and-publish.sh

set -e

echo "=========================================="
echo "Building and Publishing ClothPos API"
echo "=========================================="
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "Please run as root or with sudo: sudo bash build-and-publish.sh"
    exit 1
fi

# Step 1: Find the actual directory structure
echo "Step 1: Finding API directory..."
if [ -d "/var/www/clothpos-api/ClothPos/ClothsPos-API" ]; then
    API_DIR="/var/www/clothpos-api/ClothPos/ClothsPos-API"
    echo "✓ Found: $API_DIR"
elif [ -d "/var/www/clothpos-api/ClothsPos-API" ]; then
    API_DIR="/var/www/clothpos-api/ClothsPos-API"
    echo "✓ Found: $API_DIR"
else
    echo "✗ ERROR: Cannot find ClothsPos-API directory"
    echo "Current structure:"
    ls -la /var/www/clothpos-api/ 2>/dev/null || echo "Directory /var/www/clothpos-api/ does not exist"
    exit 1
fi

# Step 2: Navigate to API directory
echo ""
echo "Step 2: Navigating to API directory..."
cd "$API_DIR"
echo "Current directory: $(pwd)"

# Step 3: Check if .csproj exists
if [ ! -f "ClothsPos-API.csproj" ]; then
    echo "✗ ERROR: ClothsPos-API.csproj not found in $API_DIR"
    echo "Files in directory:"
    ls -la
    exit 1
fi
echo "✓ Found ClothsPos-API.csproj"

# Step 4: Check .NET installation
echo ""
echo "Step 3: Checking .NET installation..."
if ! command -v dotnet &> /dev/null; then
    echo "✗ ERROR: .NET is not installed"
    echo "Install with: sudo apt install -y dotnet-sdk-8.0"
    exit 1
fi
echo "✓ .NET version: $(dotnet --version)"

# Step 5: Clean previous build
echo ""
echo "Step 4: Cleaning previous build..."
dotnet clean > /dev/null 2>&1 || true
echo "✓ Cleaned"

# Step 6: Restore packages
echo ""
echo "Step 5: Restoring NuGet packages..."
dotnet restore
echo "✓ Packages restored"

# Step 7: Build and publish
echo ""
echo "Step 6: Building and publishing..."
dotnet publish -c Release -o ./publish
echo "✓ Published to ./publish"

# Step 8: Verify publish directory
PUBLISH_DIR="$API_DIR/publish"
if [ ! -d "$PUBLISH_DIR" ]; then
    echo "✗ ERROR: Publish directory was not created"
    exit 1
fi

if [ ! -f "$PUBLISH_DIR/ClothsPos-API.dll" ]; then
    echo "✗ ERROR: ClothsPos-API.dll not found in publish directory"
    exit 1
fi
echo "✓ Publish directory verified: $PUBLISH_DIR"

# Step 9: Copy configuration files
echo ""
echo "Step 7: Copying configuration files..."
if [ -f "appsettings.json" ]; then
    cp appsettings.json "$PUBLISH_DIR/"
    echo "✓ Copied appsettings.json"
else
    echo "⚠ appsettings.json not found in source"
fi

if [ -f "appsettings.Production.json" ]; then
    # Validate JSON before copying
    if python3 -m json.tool appsettings.Production.json > /dev/null 2>&1; then
        cp appsettings.Production.json "$PUBLISH_DIR/"
        echo "✓ Copied appsettings.Production.json (valid JSON)"
    else
        echo "⚠ appsettings.Production.json has invalid JSON, creating clean version..."
        cat > "$PUBLISH_DIR/appsettings.Production.json" << 'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Key": "",
    "Issuer": "ClothPosAPI",
    "Audience": "ClothPosClient",
    "ExpirationMinutes": 60
  }
}
EOF
        echo "✓ Created clean appsettings.Production.json"
    fi
fi

if [ -f "appsettings.MySql.json" ]; then
    cp appsettings.MySql.json "$PUBLISH_DIR/" 2>/dev/null || true
    echo "✓ Copied appsettings.MySql.json"
fi

# Step 10: Fix permissions
echo ""
echo "Step 8: Fixing permissions..."
chown -R www-data:www-data "$PUBLISH_DIR"
chmod -R 755 "$PUBLISH_DIR"
echo "✓ Permissions fixed"

# Step 11: Update service file
echo ""
echo "Step 9: Updating service file..."
SERVICE_FILE="/etc/systemd/system/clothpos-api.service"
if [ -f "$SERVICE_FILE" ]; then
    # Backup service file
    cp "$SERVICE_FILE" "$SERVICE_FILE.backup.$(date +%Y%m%d_%H%M%S)"
    
    # Update paths
    sed -i "s|WorkingDirectory=.*|WorkingDirectory=$PUBLISH_DIR|" "$SERVICE_FILE"
    sed -i "s|ExecStart=.*dotnet.*|ExecStart=/usr/bin/dotnet $PUBLISH_DIR/ClothsPos-API.dll|" "$SERVICE_FILE"
    
    echo "✓ Service file updated"
    echo "  WorkingDirectory: $PUBLISH_DIR"
    echo "  ExecStart: /usr/bin/dotnet $PUBLISH_DIR/ClothsPos-API.dll"
else
    echo "⚠ Service file not found, creating new one..."
    cat > "$SERVICE_FILE" << EOF
[Unit]
Description=ClothPos API Service
After=network.target mysql.service

[Service]
Type=notify
User=www-data
Group=www-data
WorkingDirectory=$PUBLISH_DIR
ExecStart=/usr/bin/dotnet $PUBLISH_DIR/ClothsPos-API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=clothpos-api
StandardOutput=journal
StandardError=journal

# Environment
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

# Environment Variables (uncomment and set your values)
# Environment=ConnectionStrings__DefaultConnection=Server=localhost;Database=ClothPosDB;User=clothpos_user;Password=YOUR_PASSWORD;Port=3306;SslMode=None;
# Environment=Jwt__Key=YOUR_SECRET_JWT_KEY_32_CHARACTERS_MINIMUM
# Environment=Jwt__Issuer=ClothPosAPI
# Environment=Jwt__Audience=ClothPosClient

[Install]
WantedBy=multi-user.target
EOF
    echo "✓ Service file created"
fi

# Step 12: Reload systemd
echo ""
echo "Step 10: Reloading systemd..."
systemctl daemon-reload
echo "✓ Systemd reloaded"

# Step 11: Test manual run (optional, quick test)
echo ""
echo "Step 11: Testing manual run (5 seconds)..."
cd "$PUBLISH_DIR"
timeout 5 dotnet ClothsPos-API.dll 2>&1 || true
cd "$API_DIR"
echo "✓ Manual test completed"
echo ""

# Step 12: Start service
echo ""
echo "Step 12: Starting service..."
systemctl start clothpos-api.service
sleep 3

echo ""
echo "=========================================="
echo "Build and Publish Complete!"
echo "=========================================="
echo ""
echo "Service status:"
systemctl status clothpos-api.service --no-pager -l | head -20

echo ""
echo "Publish directory: $PUBLISH_DIR"
echo ""
echo "Next steps:"
echo "1. Check logs: sudo journalctl -u clothpos-api -f"
echo "2. Test API: curl http://localhost:5000/api/items"
echo "3. If errors, check: sudo journalctl -xeu clothpos-api.service -n 50"


