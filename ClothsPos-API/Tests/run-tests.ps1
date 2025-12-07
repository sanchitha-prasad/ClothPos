# PowerShell script to run API tests
# Usage: .\run-tests.ps1

Write-Host "`n╔════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   ClothPos API Test Runner            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Check if Node.js is installed
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
$nodeVersion = node --version 2>$null
if (-not $nodeVersion) {
    Write-Host "❌ Node.js is not installed!" -ForegroundColor Red
    Write-Host "   Please install Node.js from https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Node.js version: $nodeVersion" -ForegroundColor Green

# Check if axios is installed
if (-not (Test-Path "node_modules\axios")) {
    Write-Host "`nInstalling dependencies..." -ForegroundColor Yellow
    npm install axios
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to install dependencies" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Dependencies installed" -ForegroundColor Green
} else {
    Write-Host "✅ Dependencies already installed" -ForegroundColor Green
}

# Check if API is running
Write-Host "`nChecking if API is running..." -ForegroundColor Yellow
$apiRunning = $false

# Try HTTP first (port 5000)
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -TimeoutSec 2 -ErrorAction Stop
    $apiRunning = $true
    Write-Host "✅ API is running on http://localhost:5000" -ForegroundColor Green
} catch {
    # Try HTTPS (port 5001)
    try {
        $response = Invoke-WebRequest -Uri "https://localhost:5001/swagger/index.html" -SkipCertificateCheck -TimeoutSec 2 -ErrorAction Stop
        $apiRunning = $true
        Write-Host "✅ API is running on https://localhost:5001" -ForegroundColor Green
    } catch {
        Write-Host "❌ API is not running!" -ForegroundColor Red
        Write-Host "`nPlease start the API first:" -ForegroundColor Yellow
        Write-Host "   cd .." -ForegroundColor White
        Write-Host "   dotnet run" -ForegroundColor White
        Write-Host "`nOr run the API in a separate terminal window." -ForegroundColor Yellow
        Write-Host "`nPress any key to continue anyway (tests will fail if API is not running)..." -ForegroundColor Yellow
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }
}

# Run tests
Write-Host "`nRunning API tests...`n" -ForegroundColor Cyan
node ApiTestScript.js

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Some tests failed. Check the output above." -ForegroundColor Yellow
}

