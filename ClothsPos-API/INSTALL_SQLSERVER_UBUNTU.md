# Installing SQL Server on Ubuntu

SQL Server on Linux requires specific installation steps. Here's how to install it.

## Prerequisites

- Ubuntu 20.04 or 22.04
- At least 2 GB RAM
- Root or sudo access

## Installation Steps

### Step 1: Import the public repository GPG keys

```bash
curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | sudo gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg
```

### Step 2: Register the Microsoft SQL Server Ubuntu repository

```bash
curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list | sudo tee /etc/apt/sources.list.d/mssql-server.list
```

(For Ubuntu 20.04, use `ubuntu/20.04` instead of `ubuntu/22.04`)

### Step 3: Update package list

```bash
sudo apt update
```

### Step 4: Install SQL Server

```bash
sudo apt install -y mssql-server
```

### Step 5: Configure SQL Server

```bash
sudo /opt/mssql/bin/mssql-conf setup
```

You'll be prompted to:
- Choose an edition (Developer Edition is free)
- Accept the license terms
- Set the SA (System Administrator) password

**Important:** Remember the SA password you set!

### Step 6: Verify SQL Server is running

```bash
sudo systemctl status mssql-server
```

### Step 7: Install SQL Server command-line tools (optional but recommended)

```bash
# Add repository
curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/prod.list | sudo tee /etc/apt/sources.list.d/msprod.list

# Update and install
sudo apt update
sudo apt install -y mssql-tools unixodbc-dev

# Add to PATH
echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc
source ~/.bashrc
```

### Step 8: Test connection

```bash
sqlcmd -S localhost -U sa -P 'YourPassword' -Q "SELECT @@VERSION"
```

## Configure Firewall (if needed)

```bash
sudo ufw allow 1433/tcp
```

## Create Database for ClothPos

```bash
sqlcmd -S localhost -U sa -P 'YourPassword' -Q "CREATE DATABASE ClothPosDB;"
```

## Update appsettings.json

```json
{
  "UseMySql": false,
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=ClothPosDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

## Common Issues

### Issue: "E: Unable to locate package mssql-server"

**Solution:** Make sure you're using Ubuntu 20.04 or 22.04. Check your version:
```bash
lsb_release -a
```

### Issue: "Failed to start mssql-server"

**Solution:** Check logs:
```bash
sudo journalctl -u mssql-server -n 50
```

### Issue: Can't connect to SQL Server

**Solution:** 
1. Check if service is running: `sudo systemctl status mssql-server`
2. Check if port 1433 is listening: `sudo netstat -tuln | grep 1433`
3. Verify firewall allows port 1433

## Alternative: Use MySQL (Easier)

If SQL Server installation is too complex, MySQL is easier to install:

```bash
sudo apt update
sudo apt install -y mysql-server
sudo mysql_secure_installation
```

Then use MySQL connection string in appsettings.json.

## Alternative: Use PostgreSQL (Also Easy)

```bash
sudo apt update
sudo apt install -y postgresql postgresql-contrib
sudo -u postgres createdb ClothPosDB
```

Note: The current code supports SQL Server and MySQL. PostgreSQL support would need to be added.

