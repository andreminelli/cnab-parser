# CNAB Parser Implementation Guide

## Overview

This document provides a comprehensive overview of the CNAB Parser API implementation, following Clean Architecture and Domain-Driven Design principles.

## Project Structure

```
src/
??? CnabParser.Core/                 # Domain Layer
?   ??? Entities/
?   ?   ??? Store.cs                # Store aggregate root
?   ?   ??? Transaction.cs          # Transaction entity
?   ??? Repositories/
?   ?   ??? IStoreRepository.cs
?   ?   ??? ITransactionRepository.cs
?   ??? Services/
?       ??? ICnabParserService.cs    # Parser contract
??? CnabParser.Application/          # Application Layer
?   ??? Dtos/
?   ?   ??? StoreBalanceDto.cs
?   ?   ??? TransactionDto.cs
?   ??? UseCases/
?       ??? ImportCnabFileUseCase.cs
?       ??? GetStoresBalanceUseCase.cs
??? CnabParser.Infrastructure/       # Infrastructure Layer
?   ??? Data/
?   ?   ??? CnabParserDbContext.cs
?   ??? Repositories/
?   ?   ??? StoreRepository.cs
?   ?   ??? TransactionRepository.cs
?   ??? Services/
?       ??? CnabParserService.cs     # Parser implementation
??? CnabParser.Api/                  # Presentation Layer
    ??? Program.cs                   # DI configuration
    ??? Endpoints/
    ?   ??? CnabEndpoints.cs         # REST endpoints
    ??? appsettings.json
```

## Domain Model

### Store Entity
```csharp
public class Store
{
    public int Id { get; set; }
    public string Owner { get; set; }           // Store owner name
    public string Name { get; set; }            // Store name
    public ICollection<Transaction> Transactions { get; set; }
    
    public decimal GetBalance()                 // Calculates total balance
}
```

**Uniqueness Constraint:** Stores are uniquely identified by the combination of `Name` and `Owner`.

### Transaction Entity
```csharp
public class Transaction
{
    public int Id { get; set; }
    
    // Store Information (denormalized)
    public string StoreOwner { get; set; }
    public string StoreName { get; set; }
    
    // Transaction Details
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Cpf { get; set; }
    public string Card { get; set; }
    public string Time { get; set; }
    
    public decimal GetSignedAmount()            // Applies sign based on type
}
```

### TransactionType Enum
```csharp
public enum TransactionType
{
    Debit = 1,           // Débito (Entrada) +
    Boleto = 2,          // Boleto (Saída) -
    Financiamento = 3,   // Financiamento (Saída) -
    Credit = 4,          // Crédito (Entrada) +
    LoanReceipt = 5,     // Recebimento Empréstimo (Entrada) +
    Sales = 6,           // Vendas (Entrada) +
    TedReceipt = 7,      // Recebimento TED (Entrada) +
    DocReceipt = 8,      // Recebimento DOC (Entrada) +
    Rent = 9             // Aluguel (Saída) -
}
```

## CNAB Parser Implementation

### File Format Parsing

The parser processes fixed-width records with the following field mapping:

```
Position (1-indexed)    Field                   Length    Type
1                       Type                    1         char
2-9                     Date                    8         YYYYMMDD
10-19                   Amount                  10        Numeric (÷100)
20-30                   CPF                     11        String
31-42                   Card                    12        String
43-48                   Time                    6         HHMMSS
49-62                   Store Owner             14        String
63-81                   Store Name              19        String
```

### Parsing Algorithm

1. **Read file line by line** - Stream-based processing for memory efficiency
2. **Extract fields** - Use substring with exact positions from CNAB spec
3. **Validate line length** - Ensure minimum 81 characters
4. **Parse values**:
   - Date: Convert from YYYYMMDD to DateTime
   - Amount: Parse as decimal and divide by 100
   - Type: Map character to TransactionType enum
5. **Create Transaction** - Return enriched transaction object
6. **Error handling** - Skip malformed lines and log errors

### Key Features

- **Async processing** - Uses async/await for non-blocking I/O
- **Error resilience** - Malformed lines are skipped, processing continues
- **Memory efficient** - Processes file sequentially without loading entirely into memory
- **Type safe** - Strong enum typing for transaction types

## Application Layer

### ImportCnabFileUseCase

**Responsibilities:**
1. Call parser service to extract transactions
2. Group transactions by Store Name + Owner
3. Find or create stores in repository
4. Persist transactions

**Flow:**
```
Stream ? ParseCnabFileAsync ? Transactions
         ?
      GroupBy(StoreName, StoreOwner)
         ?
      For each group:
         ? GetByNameAndOwnerAsync
         ? Create new Store if not found
         ? AddRangeAsync(Transactions)
```

### GetStoresBalanceUseCase

**Responsibilities:**
1. Retrieve all stores with transactions
2. Map to DTO with balance calculation
3. Include transaction details

**Flow:**
```
GetAllAsync(Stores with Transactions)
         ?
      For each Store:
         ? Calculate balance using GetSignedAmount()
         ? Map Transactions to TransactionDto
         ? Create StoreBalanceDto
         ?
      Return collection of StoreBalanceDto
```

## Repository Pattern

### IStoreRepository

```csharp
public interface IStoreRepository
{
    Task<IEnumerable<Store>> GetAllAsync();
    Task<Store?> GetByIdAsync(int id);
    Task<Store?> GetByNameAndOwnerAsync(string name, string owner);
    Task AddAsync(Store store);
    Task UpdateAsync(Store store);
}
```

### ITransactionRepository

```csharp
public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
}
```

## Database Schema

### Stores Table

```sql
CREATE TABLE Stores (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Owner NVARCHAR(255) NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    UNIQUE(Owner, Name)
);
```

### Transactions Table

```sql
CREATE TABLE Transactions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StoreId INT NOT NULL,
    Date DATETIME NOT NULL,
    Type INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    StoreOwner NVARCHAR(255) NOT NULL,
    StoreName NVARCHAR(255) NOT NULL,
    Cpf NVARCHAR(20) NOT NULL,
    Card NVARCHAR(20) NOT NULL,
    Time NVARCHAR(10) NOT NULL,
    FOREIGN KEY(StoreId) REFERENCES Stores(Id) ON DELETE CASCADE,
    INDEX IX_Store(StoreName, StoreOwner)
);
```

## REST API Endpoints

### POST /api/cnab/import

**Purpose:** Import a CNAB file

**Request:**
- Content-Type: `multipart/form-data`
- Body: `file` (binary CNAB.txt)

**Responses:**
- `200 OK` - File imported successfully
- `400 Bad Request` - Invalid request (missing file or wrong format)
- `500 Internal Server Error` - Processing error

**Example:**
```bash
curl -X POST "https://localhost:7000/api/cnab/import" \
  -F "file=@CNAB.txt"
```

### GET /api/cnab/stores/balance

**Purpose:** Get all stores with balances and transaction history

**Responses:**
- `200 OK` - Returns array of StoreBalanceDto

**Response Body:**
```json
[
  {
    "id": 1,
    "name": "Store Name",
    "owner": "Owner Name",
    "balance": 1500.50,
    "transactions": [
      {
        "id": 1,
        "date": "2019-03-01T00:00:00",
        "type": "Debit",
        "amount": 250.00,
        "cpf": "12345678901",
        "card": "123456****7890",
        "time": "120000"
      }
    ]
  }
]
```

## Dependency Injection

### Configuration (Program.cs)

```csharp
// Database
builder.Services.AddDbContext<CnabParserDbContext>(options =>
    options.UseSqlServer(connectionString));

// Use Cases
builder.Services.AddScoped<IImportCnabFileUseCase, ImportCnabFileUseCase>();
builder.Services.AddScoped<IGetStoresBalanceUseCase, GetStoresBalanceUseCase>();

// Services
builder.Services.AddScoped<ICnabParserService, CnabParserService>();

// Repositories
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
```

## Design Patterns Used

### 1. Repository Pattern
- Abstracts data access logic
- Allows easy testing and swapping implementations
- Single source of truth for data queries

### 2. Use Case Pattern
- Encapsulates business logic
- One use case per business operation
- Easy to test in isolation

### 3. Dependency Injection
- Loose coupling between layers
- Easy to mock for testing
- Configuration centralized in Program.cs

### 4. Value Objects
- TransactionType enum represents valid states
- Strong typing prevents invalid values

### 5. Aggregate Pattern
- Store is the aggregate root
- Transactions belong to a Store
- Consistency maintained through the Store aggregate

## Validation & Error Handling

### Parser Validation
- Minimum line length check (81 characters)
- Safe substring extraction with bounds checking
- Exception handling logs errors without throwing

### API Validation
- File presence check
- File extension validation (.txt only)
- Exception handling returns appropriate HTTP status codes

### Data Validation
- Entity Framework constraints (unique index on Store)
- Database constraints ensure data integrity

## Testing Strategy

### Unit Tests (ImportCnabFileUseCaseTests.cs)

```csharp
[Fact]
public async Task ShouldImportCnabFileSuccessfully()
{
    // Uses mock implementations of repositories
    // Verifies use case orchestration without database
    // Tests happy path: parse ? group ? store ? persist
}
```

### Test Doubles
- MockCnabParser: Simulates parser behavior
- MockStoreRepository: In-memory store collection
- MockTransactionRepository: In-memory transaction collection

## SOLID Principles Implementation

### Single Responsibility Principle
- Each class has one reason to change
- CnabParserService: Parsing logic only
- ImportCnabFileUseCase: Import orchestration only

### Open/Closed Principle
- Open for extension: Add new use cases
- Closed for modification: Interfaces define contracts
- Can add new transaction types without changing existing code

### Liskov Substitution Principle
- Repository implementations are interchangeable
- Can swap SqlServer repository with in-memory implementation

### Interface Segregation Principle
- Focused interfaces: IStoreRepository, ITransactionRepository
- Clients depend only on needed methods

### Dependency Inversion Principle
- Depend on abstractions (interfaces)
- Not on concrete implementations
- DI container manages instance creation

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CnabParserDb;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Custom Connection String

Edit the connection string for different databases:

```json
"DefaultConnection": "Server=MY_SERVER;Database=CnabParserDb;User Id=sa;Password=MyPassword;"
```

## Database Initialization

The application automatically creates the database on startup:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CnabParserDbContext>();
    dbContext.Database.EnsureCreated();
}
```

For production migration scripts, use Entity Framework Core migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Performance Considerations

1. **Async Processing** - Non-blocking I/O operations
2. **Streaming** - Line-by-line parsing reduces memory footprint
3. **Batch Operations** - AddRangeAsync for bulk inserts
4. **Indexing** - Database indexes on StoreName and StoreOwner
5. **Eager Loading** - Include(Transactions) loads relationships

## Future Enhancements

1. **Pagination** - Implement pagination for GetStoresBalance endpoint
2. **Filtering** - Filter transactions by date range, type, etc.
3. **Caching** - Cache store balance calculations
4. **Audit Trail** - Track import history and changes
5. **Batch Processing** - Handle large files with background jobs
6. **Validation Framework** - Fluent Validation for DTO validation
7. **Logging** - Serilog for structured logging
8. **API Versioning** - Support multiple API versions
