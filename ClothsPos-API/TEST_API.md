# How to Test Your ClothPos API

## Quick Tests

### 1. Check Service Status

```bash
sudo systemctl status clothpos-api
```

Should show: `Active: active (running)`

### 2. Check if Port is Listening

```bash
sudo netstat -tuln | grep 5000
# or
sudo ss -tuln | grep 5000
```

Should show: `0.0.0.0:5000` or `127.0.0.1:5000`

### 3. Test API Endpoint (Basic)

```bash
curl http://localhost:5000/api/items
```

**Expected responses:**
- `200 OK` - API is working (may return empty array `[]` if no items)
- `401 Unauthorized` - API is working but requires authentication
- `Connection refused` - API is not running

### 4. Test with Authentication (Login)

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin-dev","password":"Cl0thP0s@2024#Secure!"}'
```

**Expected response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "...",
    "username": "admin-dev",
    "email": "admin@shop.com"
  }
}
```

### 5. Test Protected Endpoint (with token)

```bash
# First, get token
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin-dev","password":"Cl0thP0s@2024#Secure!"}' \
  | grep -o '"token":"[^"]*' | cut -d'"' -f4)

# Then use token
curl http://localhost:5000/api/items \
  -H "Authorization: Bearer $TOKEN"
```

## Common Endpoints to Test

### Public Endpoints (if any)
```bash
# Health check (if implemented)
curl http://localhost:5000/api/health

# Swagger UI (if enabled)
curl http://localhost:5000/swagger
```

### Protected Endpoints (require login)
```bash
# Get token first
TOKEN="your-token-here"

# Items
curl http://localhost:5000/api/items -H "Authorization: Bearer $TOKEN"

# Categories
curl http://localhost:5000/api/categories -H "Authorization: Bearer $TOKEN"

# Users
curl http://localhost:5000/api/users -H "Authorization: Bearer $TOKEN"

# Sales
curl http://localhost:5000/api/sales -H "Authorization: Bearer $TOKEN"
```

## View Logs

### Real-time logs
```bash
sudo journalctl -u clothpos-api -f
```

### Recent logs
```bash
sudo journalctl -u clothpos-api -n 50
```

### Logs with errors only
```bash
sudo journalctl -u clothpos-api -p err
```

## Test from Browser

If you have Nginx configured, you can access:
- `http://your-server-ip/api/items` (if public)
- `http://your-server-ip/swagger` (Swagger UI)

## Test from Another Machine

If firewall allows:

```bash
# From your local machine
curl http://YOUR_SERVER_IP:5000/api/items

# Or if Nginx is configured
curl http://YOUR_SERVER_IP/api/items
```

## Automated Test Script

I've created `test-api.sh` that tests everything automatically:

```bash
bash test-api.sh
```

## Expected Results

### ✅ API is Working If:
- Service status shows `active (running)`
- Port 5000 is listening
- `curl` returns HTTP 200, 401, or 404 (not connection refused)
- Login endpoint returns a token
- Logs show no errors

### ❌ API is Not Working If:
- Service status shows `failed` or `inactive`
- Port 5000 is not listening
- `curl` returns "Connection refused"
- Logs show errors

## Troubleshooting

### "Connection refused"
- Service is not running: `sudo systemctl start clothpos-api`
- Wrong port: Check `ASPNETCORE_URLS` in service file

### "401 Unauthorized"
- This is normal for protected endpoints
- You need to login first to get a token

### "500 Internal Server Error"
- Check logs: `sudo journalctl -u clothpos-api -n 50`
- Usually database connection issue

### "Timeout"
- Database might be slow to connect
- Check MySQL is running: `sudo systemctl status mysql`

## Quick Health Check

Run this one-liner:

```bash
echo "Service: $(systemctl is-active clothpos-api)" && \
echo "Port: $(netstat -tuln 2>/dev/null | grep :5000 || echo 'not listening')" && \
echo "API: $(curl -s -o /dev/null -w '%{http_code}' http://localhost:5000/api/items --max-time 2)"
```

Expected output:
```
Service: active
Port: tcp 0.0.0.0:5000 LISTEN
API: 200
```

