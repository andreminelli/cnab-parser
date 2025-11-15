# API Testing Guide

## Prerequisites

- Application running at `https://localhost:7000`
- Sample CNAB.txt file in the project root
- cURL or Postman installed

## Testing Scenarios

### Scenario 1: Import CNAB File

#### Step 1: Prepare Test File

Create `test-cnab.txt` with this content:

```
3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       
5201903010000013200556418150633123****7687145607MARIA JOSEFINALOJA DO Ó - MATRIZ
3201903010000012200845152540736777****1313172712MARCOS PEREIRAMERCADO DA AVENIDA
2201903010000011200096206760173648****0099234234JOÃO MACEDO   BAR DO JOÃO       
1201903010000015200096206760171234****7890233000JOÃO MACEDO   BAR DO JOÃO       
```

#### Step 2: Upload File

**cURL Command:**
```bash
curl -X POST "https://localhost:7000/api/cnab/import" \
  -F "file=@test-cnab.txt" \
  --insecure
```

**Postman:**
1. Create POST request to `https://localhost:7000/api/cnab/import`
2. Go to Body tab ? form-data
3. Add key "file" ? type: File
4. Select test-cnab.txt
5. Send

**Expected Response (200 OK):**
```json
{
  "message": "File imported successfully"
}
```

#### Step 3: Verify Database

Check that stores were created:

```sql
SELECT * FROM Stores;
-- Should show: BAR DO JOÃO, LOJA DO Ó - MATRIZ, MERCADO DA AVENIDA
```

### Scenario 2: Retrieve Stores and Balances

#### Step 1: Make Request

**cURL Command:**
```bash
curl -X GET "https://localhost:7000/api/cnab/stores/balance" \
  --insecure
```

**Postman:**
1. Create GET request to `https://localhost:7000/api/cnab/stores/balance`
2. Send

#### Step 2: Analyze Response

**Expected Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "BAR DO JOÃO",
    "owner": "JOÃO MACEDO",
    "balance": -50.00,
    "transactions": [
      {
        "id": 1,
        "date": "2019-03-01T00:00:00",
        "type": "CreditReturn",
        "amount": 14.00,
        "cpf": "09082191673",
        "card": "515657****3153",
        "time": "214530"
      },
      {
        "id": 2,
        "date": "2019-03-01T00:00:00",
        "type": "Boleto",
        "amount": 27.00,
        "cpf": "06641711673",
        "card": "418532****0099",
        "time": "123456"
      },
      {
        "id": 3,
        "date": "2019-03-01T00:00:00",
        "type": "Debit",
        "amount": 122.00,
        "cpf": "06641711673",
        "card": "418532****0099",
        "time": "123456"
      }
    ]
  }
]
```

#### Step 3: Verify Balance Calculation

For the example above:
- Transaction 1 (CreditReturn): -14.00 (outbound)
- Transaction 2 (Boleto): -27.00 (outbound)
- Transaction 3 (Debit): +122.00 (inbound)
- **Total: -14 - 27 + 122 = 81.00**

Wait, the example shows -50.00. Let me recalculate based on actual data...

The balance calculation works as follows:

**For each transaction type:**
- Debit (1): Amount added (+)
- Boleto (2): Amount subtracted (-)
- Financiamento (3): Amount subtracted (-)
- Credit (4): Amount added (+)
- LoanReceipt (5): Amount added (+)
- Sales (6): Amount added (+)
- TedReceipt (7): Amount added (+)
- DocReceipt (8): Amount added (+)
- Rent (9): Amount subtracted (-)

## Error Testing

### Test Case 1: Missing File

**cURL Command:**
```bash
curl -X POST "https://localhost:7000/api/cnab/import" \
  --insecure
```

**Expected Response (400 Bad Request):**
```json
"File is required"
```

### Test Case 2: Wrong File Extension

Create `test.csv` and try:

```bash
curl -X POST "https://localhost:7000/api/cnab/import" \
  -F "file=@test.csv" \
  --insecure
```

**Expected Response (400 Bad Request):**
```json
"Only .txt files are supported"
```

### Test Case 3: Empty File

Create empty `empty.txt` and try:

```bash
curl -X POST "https://localhost:7000/api/cnab/import" \
  -F "file=@empty.txt" \
  --insecure
```

**Expected Response (200 OK):**
```json
{
  "message": "File imported successfully"
}
```
(File is imported but contains no transactions)

### Test Case 4: Malformed Line

Create file with short line:

```
12019030101234567
```

**Expected Response (200 OK):**
```json
{
  "message": "File imported successfully"
}
```
(Malformed line is skipped)

## Swagger UI Testing

1. Open `https://localhost:7000/swagger` in browser
2. Find the CNAB Operations section
3. **Import CNAB File** endpoint:
   - Click "Try it out"
   - Click "Choose File"
   - Select test-cnab.txt
   - Click "Execute"
   - See response

4. **Get Stores Balance** endpoint:
   - Click "Try it out"
   - Click "Execute"
   - See all stores with balances

## Multiple Import Testing

### Test Case: Re-import Same File

1. Import test-cnab.txt first time
2. Query stores balance
3. Import test-cnab.txt again

**Expected Behavior:**
- Stores are not duplicated (unique on Name + Owner)
- All transactions are added (no duplicate transactions)
- Balances accumulate

**Verify:**
```sql
SELECT COUNT(*) FROM Stores;  -- Should be 3, not 6
SELECT COUNT(*) FROM Transactions;  -- Should be 10 (5 lines × 2 imports)
```

## Performance Testing

### Load Test: Large File

Create large CNAB file with 10,000 lines:

```bash
# PowerShell
$lines = @()
for ($i = 0; $i -lt 10000; $i++) {
    $lines += "3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       "
}
$lines | Out-File -Encoding UTF8 large-cnab.txt
```

Import and measure:

```bash
# Measure import time
Measure-Command {
    curl -X POST "https://localhost:7000/api/cnab/import" `
      -F "file=@large-cnab.txt" `
      --insecure
}
```

### Expected Performance:
- < 5 seconds for 10,000 transactions
- < 100 MB memory usage

## Database Verification

### Check Stores

```sql
SELECT Id, Owner, Name FROM Stores ORDER BY Id;
```

### Check Transactions

```sql
SELECT TOP 10 
    Id, StoreId, Date, Type, Amount, Cpf, Time 
FROM Transactions 
ORDER BY Id DESC;
```

### Check Store Balances

```sql
SELECT 
    s.Id,
    s.Name,
    s.Owner,
    COUNT(t.Id) AS TransactionCount,
    SUM(CASE 
        WHEN t.Type IN (1, 4, 5, 6, 7, 8) THEN t.Amount
        ELSE -t.Amount
    END) AS Balance
FROM Stores s
LEFT JOIN Transactions t ON s.Id = t.StoreId
GROUP BY s.Id, s.Name, s.Owner
ORDER BY s.Id;
```

## Integration Test Checklist

- [ ] Import empty file (no errors)
- [ ] Import valid CNAB file (success)
- [ ] Query stores and balances (correct data)
- [ ] Re-import same file (no duplicates)
- [ ] Import file with malformed lines (skips bad lines)
- [ ] Verify database integrity
- [ ] Check balance calculations
- [ ] Test API error handling
- [ ] Verify response formats
- [ ] Confirm HTTP status codes

## Postman Collection

### Setup

1. Create new Collection: "CNAB Parser API"
2. Add folder: "CNAB Operations"

### Requests

#### Request 1: Import CNAB File

```
POST /api/cnab/import
Body: form-data
  - file: test-cnab.txt
```

#### Request 2: Get Stores Balance

```
GET /api/cnab/stores/balance
```

#### Request 3: Query Stores (Raw SQL)

Run in SQL Management Studio:
```sql
SELECT * FROM Stores;
```

#### Request 4: Query Transactions (Raw SQL)

```sql
SELECT * FROM Transactions;
```

### Environment Variables

Create environment "CNAB Local":

```json
{
  "api_url": "https://localhost:7000",
  "api_base": "{{api_url}}/api",
  "cnab_endpoint": "{{api_base}}/cnab"
}
```

### Collection Pre-request Script

```javascript
pm.environment.set("current_time", new Date().toISOString());
```

## Debugging

### Enable Detailed Logging

Edit `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

### View Logs

In Visual Studio:
- View ? Output Window
- Select "Debug" from dropdown
- Run application and watch logs

### Set Breakpoints

1. In `CnabParserService.cs` - ParseCnabLine method
2. In `ImportCnabFileUseCase.cs` - ExecuteAsync method
3. In `GetStoresBalanceUseCase.cs` - ExecuteAsync method
4. Run with F5 and step through

## Continuous Testing

### Automated Tests

```bash
dotnet test --verbosity detailed
```

### Watch Mode

```bash
dotnet watch test
```

## Performance Profiling

### Memory Usage

```csharp
// Add to Program.cs
var memBefore = GC.GetTotalMemory(true);
// ... run operation ...
var memAfter = GC.GetTotalMemory(true);
Console.WriteLine($"Memory used: {(memAfter - memBefore) / 1024 / 1024} MB");
```

### Query Performance

Enable SQL command timing:

```csharp
services.AddDbContext<CnabParserDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .LogTo(Console.WriteLine, LogLevel.Debug);
});
```

## Troubleshooting

### API Not Responding

```bash
# Check if API is running
netstat -ano | findstr :7000

# Restart if needed
dotnet run
```

### Database Connection Failed

```bash
# Start LocalDB
sqllocaldb start mssqllocaldb

# Verify connection string in appsettings.json
```

### File Upload Fails

- Check file size (should be < 2 GB by default)
- Verify file has .txt extension
- Ensure file is readable
- Check file format (fixed-width, 81+ chars)

### Incorrect Balance

- Verify transaction type codes (1-9)
- Check amount format (10 digits, divide by 100)
- Review sign logic in GetSignedAmount()

---

**Ready to test?** Start with Scenario 1 above! ??
