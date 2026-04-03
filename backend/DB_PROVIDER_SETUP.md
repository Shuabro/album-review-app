# Database Provider Configuration

To switch between SQL Server and PostgreSQL, set the environment variable `DB_PROVIDER` to either `sqlserver` or `postgresql` (or `postgres`).

## Example (Windows PowerShell)

# For SQL Server (default)
$env:DB_PROVIDER = "sqlserver"

# For PostgreSQL
$env:DB_PROVIDER = "postgresql"

## Connection Strings
- SQL Server: Set `DefaultConnection` in `appsettings.json` or `appsettings.Development.json`.
- PostgreSQL: Set `PostgresConnection` in `appsettings.json` or `appsettings.Development.json`.

## Example Usage

```
# Set environment variable for PostgreSQL
$env:DB_PROVIDER = "postgresql"

# Run the backend
 dotnet run
```

The backend will automatically use the correct provider and connection string based on the environment variable.