# How to Fix CORS Issues

## ✅ CORS Configuration Updated

The CORS configuration has been improved to handle all scenarios properly.

## Changes Made

1. **CORS Middleware Order**: Moved `UseCors()` before `UseAuthentication()` and `UseAuthorization()` - this is critical!
2. **Preflight Caching**: Added 24-hour cache for preflight requests to improve performance
3. **Better Origin Handling**: Improved logic for adding multiple allowed origins

## Current Allowed Origins

- `http://localhost:3000` (Development)
- `http://localhost:5173` (Development)
- `https://clothpos-frontend.netlify.app` (Production)

## How to Apply the Fix

### Step 1: Update Code on Server

If you haven't already, update `Program.cs` on your server with the new CORS configuration.

### Step 2: Restart API Server

```bash
# SSH into your server
ssh root@139.59.2.164

# Restart the API service
sudo systemctl restart clothpos-api

# Check status
sudo systemctl status clothpos-api

# View logs if there are errors
sudo journalctl -u clothpos-api -n 50
```

### Step 3: Verify CORS Headers

Test from your browser console or using curl:

```bash
# Test preflight request (OPTIONS)
curl -X OPTIONS http://139.59.2.164/api/settings \
  -H "Origin: https://clothpos-frontend.netlify.app" \
  -H "Access-Control-Request-Method: GET" \
  -H "Access-Control-Request-Headers: authorization" \
  -v

# Should return:
# Access-Control-Allow-Origin: https://clothpos-frontend.netlify.app
# Access-Control-Allow-Methods: GET, POST, PUT, DELETE, etc.
# Access-Control-Allow-Headers: *
# Access-Control-Allow-Credentials: true
```

## Common CORS Issues and Solutions

### Issue 1: "No 'Access-Control-Allow-Origin' header"

**Solution**: Ensure CORS middleware is before authentication middleware in `Program.cs`

### Issue 2: Preflight (OPTIONS) requests failing

**Solution**: CORS middleware must be before authentication. The updated configuration handles this.

### Issue 3: Credentials not working

**Solution**: Ensure `AllowCredentials()` is set and origin is specific (not `AllowAnyOrigin()`)

### Issue 4: Mixed Content (HTTPS → HTTP)

**Problem**: Browsers block HTTP requests from HTTPS pages

**Solutions**:
1. **Set up HTTPS on backend** (Best solution)
   - Use Let's Encrypt with Certbot
   - Configure Nginx reverse proxy with SSL

2. **Use Netlify Proxy** (Temporary workaround)
   - Configure proxy in `netlify.toml` to forward `/api/*` requests

3. **Allow mixed content in browser** (Development only)
   - Chrome: `chrome://flags/#block-insecure-private-network-requests` → Disable
   - Not recommended for production

## Testing CORS

### From Browser Console

```javascript
// Test API call
fetch('http://139.59.2.164/api/settings', {
  method: 'GET',
  headers: {
    'Authorization': 'Bearer YOUR_TOKEN',
    'Content-Type': 'application/json'
  },
  credentials: 'include'
})
.then(response => response.json())
.then(data => console.log('Success:', data))
.catch(error => console.error('CORS Error:', error));
```

### Using curl

```bash
# Test actual request
curl -X GET http://139.59.2.164/api/settings \
  -H "Origin: https://clothpos-frontend.netlify.app" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -v
```

## Adding More Origins

To allow additional origins, update `appsettings.json`:

```json
{
  "AllowedOrigins": [
    "https://your-custom-domain.com",
    "https://another-domain.com"
  ],
  "FrontendUrl": "https://clothpos-frontend.netlify.app"
}
```

Or set environment variables:

```bash
export AllowedOrigins__0="https://your-custom-domain.com"
export AllowedOrigins__1="https://another-domain.com"
```

## Verification Checklist

- [ ] CORS middleware is before authentication middleware
- [ ] `AllowCredentials()` is enabled
- [ ] Origins are specific (not `AllowAnyOrigin()`)
- [ ] API server has been restarted
- [ ] Preflight requests return correct headers
- [ ] Actual requests include CORS headers in response

## Still Having Issues?

1. **Check browser console** for specific CORS error messages
2. **Check API logs**: `sudo journalctl -u clothpos-api -f`
3. **Test with curl** to see actual response headers
4. **Verify origin** matches exactly (no trailing slashes, correct protocol)

---

**After restarting the API server, CORS should work correctly!** 🎉

