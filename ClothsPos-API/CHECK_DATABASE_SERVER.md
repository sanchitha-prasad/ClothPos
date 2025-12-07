# How to Check Which Database Server is Installed

## Check SQL Server

```bash
# Check if SQL Server is installed
sqlcmd -S localhost -U sa -Q "SELECT @@VERSION" 2>/dev/null && echo "SQL Server is installed" || echo "SQL Server is NOT installed"

# Or check if SQL Server service is running
systemctl status mssql-server 2>/dev/null || echo "SQL Server service not found"

# Check if SQL Server package is installed
dpkg -l | grep mssql-server || echo "SQL Server package not found"
```

## Check MySQL

```bash
# Check if MySQL is installed
mysql --version 2>/dev/null && echo "MySQL is installed" || echo "MySQL is NOT installed"

# Check if MySQL service is running
systemctl status mysql 2>/dev/null || systemctl status mysqld 2>/dev/null || echo "MySQL service not found"

# Check if MySQL package is installed
dpkg -l | grep mysql-server || echo "MySQL package not found"
```

## Check PostgreSQL

```bash
# Check if PostgreSQL is installed
psql --version 2>/dev/null && echo "PostgreSQL is installed" || echo "PostgreSQL is NOT installed"

# Check if PostgreSQL service is running
systemctl status postgresql 2>/dev/null || echo "PostgreSQL service not found"
```

## Quick All-in-One Check Script

Run this to check all databases:

```bash
#!/bin/bash
echo "=== Checking Database Servers ==="
echo ""

echo "SQL Server:"
if command -v sqlcmd &> /dev/null; then
    echo "  ✓ sqlcmd found"
    systemctl status mssql-server --no-pager 2>/dev/null | head -3 || echo "  ✗ Service not running"
else
    echo "  ✗ SQL Server NOT installed"
fi
echo ""

echo "MySQL:"
if command -v mysql &> /dev/null; then
    echo "  ✓ mysql found"
    systemctl status mysql --no-pager 2>/dev/null | head -3 || echo "  ✗ Service not running"
else
    echo "  ✗ MySQL NOT installed"
fi
echo ""

echo "PostgreSQL:"
if command -v psql &> /dev/null; then
    echo "  ✓ psql found"
    systemctl status postgresql --no-pager 2>/dev/null | head -3 || echo "  ✗ Service not running"
else
    echo "  ✗ PostgreSQL NOT installed"
fi
```

Save as `check-databases.sh`, make executable (`chmod +x check-databases.sh`), and run it.

