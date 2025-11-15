# API Modernization - Quick Reference

## What Changed?

### 1. Aspire Integration ?
- SQL Server connection now managed by Aspire AppHost
- Automatic service discovery for `cnabdb` database
- Falls back to local development server if needed

### 2. Documentation System ?
- Replaced Swagger with **Scalar** (modern OpenAPI UI)
- Using **Microsoft.AspNetCore.OpenApi** standard
- Access at: `https://localhost:7000/docs` (dev only)
- OpenAPI JSON: `https://localhost:7000/openapi/v1.json`

### 3. Endpoint Changes ?
- **Removed** `/api` prefix
- **Route now**: `POST /transactions/import` (was `POST /api/cnab/import`)
- **Single endpoint** for now (GET /stores/balance removed)

### 4. Files Changed ?
```
Modified:
- src/CnabParser.Api/Program.cs
- src/CnabParser.Api/CnabParser.Api.csproj
- src/CnabParser.Api/Endpoints/CnabEndpoints.cs (? TransactionEndpoints.cs)

Created:
- API_MODERNIZATION.md
```

## Running the Application

### With Aspire (Recommended)
```bash
dotnet run --project src/CnabParser.AppHost
```
- Starts SQL Server container automatically
- Creates `cnabdb` database
- Runs API with automatic connection string injection
- Access: `https://localhost:7000`

### Without Aspire (Local only)
```bash
dotnet run --project src/CnabParser.Api
```
- Uses fallback local development SQL Server
- Requires manual database setup

## API Endpoint

### POST /transactions/import

**Purpose:** Upload and import a CNAB file

**Request:**
```bash
curl -X POST "https://localhost:7000/transactions/import" \
  -F "file=@CNAB.txt" \
  --insecure
```

**Success Response (200):**
```json
{
  "message": "Transactions imported successfully"
}
```

**Error Responses:**
- `400` - File missing or wrong format
- `500` - Import processing error

## Documentation Access

### Scalar UI
```
https://localhost:7000/docs
```
- Modern, clean interface
- Try endpoint directly from browser
- View request/response examples
- Explore API interactively

### OpenAPI JSON
```
https://localhost:7000/openapi/v1.json
```
- Raw OpenAPI 3.0.1 specification
- Can be imported into other tools
- Valid against OpenAPI schema

## Key Points

| Item | Details |
|------|---------|
| **Main Endpoint** | `POST /transactions/import` |
| **Documentation** | `https://localhost:7000/docs` |
| **DB Connection** | Aspire-managed via AppHost |
| **Framework** | .NET 8.0 |
| **OpenAPI Version** | 3.0.1 |
| **UI Framework** | Scalar |

## Architecture

```
?? CnabParser.AppHost ??
?                      ?
? ?? SQL Server ????  ?
? ? - cnabdb       ?  ?
? ??????????????????  ?
?                      ?
? ?? CnabParser.Api ??
? ? Service Discovery??
? ? (auto connects)  ??
? ?????????????????????
????????????????????????
```

## Build Status

```
? BUILD SUCCESSFUL
? NO ERRORS
? NO WARNINGS
```

## Environment Variables

When running via Aspire:
- `DOTNET_ENVIRONMENT=Development` (auto-set)
- Connection string auto-injected
- No manual configuration needed

When running standalone:
- Fallback: `Server=localhost,11433;Database=CnabParserDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=true`
- Can be overridden via configuration

## Troubleshooting

### API won't start
```bash
# Ensure AppHost is running first
dotnet run --project src/CnabParser.AppHost
```

### Documentation page blank
```bash
# Check development environment
# Should be Development for /docs endpoint
```

### Import fails with connection error
```bash
# Verify SQL Server container running
# Check AppHost logs for database creation
```

## Next Development Steps

Future endpoints can easily be added:

```csharp
// Example: Add another endpoint
group.MapGet("/transactions/{id}", GetTransaction)
    .WithName("Get Transaction Details");
```

The route group prefix `/transactions` will automatically apply.

## Files Checklist

- [x] `Program.cs` - Updated with Aspire and Scalar
- [x] `TransactionEndpoints.cs` - New file with simplified endpoints
- [x] `CnabParser.Api.csproj` - Updated dependencies
- [x] `appsettings.json` - No changes needed
- [x] All other layers - Unchanged

## Performance Notes

- **Scalar UI**: ~300KB (includes dependencies)
- **Swagger UI**: Was ~1.5MB
- **Performance Impact**: Lighter than before

## Security Notes

- Documentation endpoints only in Development
- Remove manual OpenAPI route in production
- Use Azure API Management for production docs
- HTTPS enforced via `UseHttpsRedirection()`

---

**Last Updated**: Now
**Status**: ? Ready to Use
**Next Step**: Run `dotnet run --project src/CnabParser.AppHost`
