# Accessing API from Outside the Server

## Why "localhost"?

When we test with `localhost:5000`, we're testing **from inside the server**. This is the default because:

1. **Security**: The API binds to `localhost` (127.0.0.1) by default - only accessible from the server itself
2. **Nginx Reverse Proxy**: Usually, you use Nginx to expose the API to the internet
3. **Testing**: `localhost` is for testing that the API works on the server

## Current Setup

Your API is running on:
- **Internal**: `http://localhost:5000` (only accessible from the server)
- **External**: Not directly accessible (needs Nginx or firewall rules)

## Option 1: Access via Nginx (Recommended)

If you have Nginx configured, access via:
- `http://YOUR_SERVER_IP/api/items`
- `http://your-domain.com/api/items`

Nginx forwards requests to `localhost:5000` internally.

## Option 2: Make API Accessible Directly

### Change API to Listen on All Interfaces

**Option A: Update Service File (Recommended)**

```bash
sudo nano /etc/systemd/system/clothpos-api.service
```

Change:
```ini
Environment=ASPNETCORE_URLS=http://localhost:5000
```

To:
```ini
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
```

Then:
```bash
sudo systemctl daemon-reload
sudo systemctl restart clothpos-api
```

**Option B: Update appsettings.json**

Add to `appsettings.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  }
}
```

### Open Firewall Port

```bash
# Allow port 5000
sudo ufw allow 5000/tcp

# Or allow from specific IP
sudo ufw allow from YOUR_IP to any port 5000
```

### Test from Outside

From your local machine:
```bash
curl http://YOUR_SERVER_IP:5000/api/items
```

## Option 3: Use Nginx Reverse Proxy (Best Practice)

This is the recommended way - keep API on localhost, use Nginx to expose it.

### Nginx Configuration

```bash
sudo nano /etc/nginx/sites-available/clothpos-api
```

```nginx
server {
    listen 80;
    server_name your-domain.com;  # or YOUR_SERVER_IP

    location /api {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable and restart:
```bash
sudo ln -s /etc/nginx/sites-available/clothpos-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

Then access via:
- `http://YOUR_SERVER_IP/api/items`
- `http://your-domain.com/api/items`

## Current vs External Access

### Testing on Server (localhost)
```bash
# From the server itself
curl http://localhost:5000/api/items
```

### Testing from Your Computer
```bash
# If API is on 0.0.0.0:5000 and firewall allows
curl http://YOUR_SERVER_IP:5000/api/items

# If using Nginx
curl http://YOUR_SERVER_IP/api/items
```

## Security Considerations

### Why localhost by default?
- ✅ **Security**: API not exposed to internet
- ✅ **Best Practice**: Use reverse proxy (Nginx)
- ✅ **Control**: You decide what's exposed

### If you expose directly:
- ⚠️ **Security Risk**: API directly accessible
- ⚠️ **No SSL**: Unless you configure it
- ⚠️ **Firewall**: Must manage firewall rules

## Quick Check: What's Your Setup?

```bash
# Check what the API is listening on
sudo netstat -tuln | grep 5000
```

- `127.0.0.1:5000` = Only localhost (current)
- `0.0.0.0:5000` = All interfaces (accessible from outside)

## Recommendation

**Keep API on localhost** and use **Nginx reverse proxy**:
1. ✅ More secure
2. ✅ Can add SSL easily
3. ✅ Better performance
4. ✅ Standard practice

## Test from Your Computer

After setting up Nginx or changing to 0.0.0.0:

```bash
# Replace YOUR_SERVER_IP with your actual server IP
curl http://YOUR_SERVER_IP/api/items

# Or if using direct port
curl http://YOUR_SERVER_IP:5000/api/items
```

