# Quick Start Guide

## Prerequisites

- .NET 8.0 SDK or later
- SQL Server LocalDB (included with Visual Studio) or SQL Server instance
- Git

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/andreminelli/cnab-parser.git
cd cnab-parser
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

## Running the Application

### Local Development

```bash
cd src/CnabParser.Api
dotnet run
```

The API will start at:
- **HTTPS:** `https://localhost:7000`
- **HTTP:** `http://localhost:5000`

### Access Swagger UI

Navigate to `https://localhost:7000/swagger` in your browser to explore the API interactively.

## Database Setup

The database is automatically created on first run using `EnsureCreated()`.

### Connection String

By default, the app uses LocalDB:
```
Server=(localdb)\mssqllocaldb;Database=CnabParserDb;Trusted_Connection=true;
```

To use a different server, edit `src/CnabParser.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=CnabParserDb;Integrated Security=true;"
  }
}
```

## API Usage Examples

### 1. Import CNAB File

```bash
curl -X POST "https://localhost:7000/api/cnab/import" \
  -F "file=@CNAB.txt" \
  --insecure
```

**Response (Success):**
```json
{
  "message": "File imported successfully"
}
```

### 2. Get Stores and Balances

```bash
curl -X GET "https://localhost:7000/api/cnab/stores/balance" \
  --insecure
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "BAR DO JOÃO",
    "owner": "JOÃO MACEDO",
    "balance": 1500.50,
    "transactions": [
      {
        "id": 1,
        "date": "2019-03-01T00:00:00",
        "type": "Debit",
        "amount": 250.00,
        "cpf": "09082191673",
        "card": "515657****3153",
        "time": "214530"
      }
    ]
  }
]
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity detailed

# Run specific test project
dotnet test tests/CnabParser.Application.Tests/CnabParser.Application.Tests.csproj
```

## Project Structure Overview

```
cnab-parser/
??? src/
?   ??? CnabParser.Api/              # REST API
?   ??? CnabParser.Core/             # Domain layer
?   ??? CnabParser.Application/      # Application layer
?   ??? CnabParser.Infrastructure/   # Infrastructure layer
?   ??? CnabParser.AppHost/          # App host (orchestration)
?   ??? CnabParser.ServiceDefaults/  # Service defaults
??? tests/
?   ??? CnabParser.Application.Tests/
?   ??? CnabParser.Core.Tests/
??? README.md                        # Main documentation
??? IMPLEMENTATION_GUIDE.md          # Technical details
??? QUICK_START.md                   # This file
??? CNAB.txt                         # Sample CNAB file
```

## Troubleshooting

### Issue: Database Connection Error

**Error:** `A network-related or instance-specific error occurred`

**Solution:** 
- Ensure SQL Server LocalDB is running: `sqllocaldb start mssqllocaldb`
- Update connection string in `appsettings.json`
- Verify SQL Server instance name

### Issue: Port Already in Use

**Error:** `System.IO.IOException: Failed to bind to address`

**Solution:**
- Change port in `launchSettings.json`
- Or kill the process using the port:
  ```bash
  netstat -ano | findstr :7000
  taskkill /PID <PID> /F
  ```

### Issue: File Format Error

**Error:** `Only .txt files are supported`

**Solution:**
- Ensure the CNAB file has `.txt` extension
- Verify file is valid CNAB format (fixed-width, 81+ chars per line)

## Common Tasks

### Create a New CNAB File for Testing

Create a text file with the following format:

```
3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       
5201903010000013200556418150633123****7687145607MARIA JOSEFINALOJA DO Ó - MATRIZ
```

**Position Reference:**
- Char 1: Type
- Chars 2-9: Date (YYYYMMDD)
- Chars 10-19: Amount
- Chars 20-30: CPF
- Chars 31-42: Card
- Chars 43-48: Time
- Chars 49-62: Store Owner
- Chars 63-81: Store Name

### Query Stores via SQL

```sql
SELECT * FROM Stores;
SELECT * FROM Transactions WHERE StoreId = 1;
```

### Clear Database

```bash
# Using .NET CLI
dotnet ef database drop --project src/CnabParser.Infrastructure --startup-project src/CnabParser.Api
```

## Development Tips

### Debug Mode

Press `F5` in Visual Studio or run:
```bash
dotnet run --configuration Debug
```

### Code Navigation

- **Entities:** `src/CnabParser.Core/Entities/`
- **Use Cases:** `src/CnabParser.Application/UseCases/`
- **Repositories:** `src/CnabParser.Infrastructure/Repositories/`
- **Parser:** `src/CnabParser.Infrastructure/Services/CnabParserService.cs`
- **API Endpoints:** `src/CnabParser.Api/Endpoints/CnabEndpoints.cs`

### Hot Reload

Watch for file changes and recompile:
```bash
dotnet watch run
```

## Environment Variables

### Development
- **ASPNETCORE_ENVIRONMENT:** `Development`
- **ASPNETCORE_URLS:** `https://localhost:7000;http://localhost:5000`

### Production
- **ASPNETCORE_ENVIRONMENT:** `Production`
- **ConnectionStrings:DefaultConnection:** Production DB connection string

## Performance Tuning

### Large File Import

For CNAB files > 100MB:
1. Increase DbContext batch size:
   ```csharp
   options.UseSqlServer(connectionString, 
       o => o.MaxBatchSize(1000));
   ```

2. Disable change tracking for imports:
   ```csharp
   _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
   ```

### Query Optimization

Add indexes for frequently queried fields:
```sql
CREATE INDEX IX_TransactionDate ON Transactions(Date);
CREATE INDEX IX_TransactionType ON Transactions(Type);
```

## Support

For issues or questions:
1. Check README.md and IMPLEMENTATION_GUIDE.md
2. Review test files for usage examples
3. Check sample CNAB.txt for format reference

## Next Steps

1. ? Run the application
2. ? Import the sample CNAB.txt file
3. ? Query the results via `/api/cnab/stores/balance`
4. ? Explore the code structure
5. ? Run the tests
6. ? Customize for your needs
