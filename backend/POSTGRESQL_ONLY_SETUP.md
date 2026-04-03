# PostgreSQL-Only Backend Setup

This branch is configured to use PostgreSQL only.

## Configuration
- The backend uses the `PostgresConnection` string in `appsettings.json` or `appsettings.Development.json`.
- No SQL Server dependencies or configuration remain.

## Example Connection String
```
"ConnectionStrings": {
  "PostgresConnection": "Host=localhost;Database=AlbumReviewDb;Username=postgres;Password=yourpassword"
}
```

## Usage
1. Ensure PostgreSQL is running and accessible.
2. Update the connection string with your credentials.
3. Run the backend:
   ```
   dotnet run
   ```

## Data Access
- All data access is now handled via Entity Framework Core and `AppDbContext`.
- No direct SQL Server or raw ADO.NET code remains.

---

For any new data access, use EF Core patterns and avoid provider-specific SQL.
