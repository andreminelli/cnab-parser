# CNAB Parser API

A clean architecture REST API for importing and analyzing Brazilian CNAB financial transaction files.

## Architecture

The solution follows **Clean Architecture** and **Domain-Driven Design** principles with the following layers:

### Layers

1. **CnabParser.Core** - Domain layer
   - Domain entities (`Store`, `Transaction`)
   - Repository interfaces
   - Service interfaces
   - Business logic and value objects

2. **CnabParser.Application** - Application layer
   - Use cases (`ImportCnabFileUseCase`, `GetStoresBalanceUseCase`)
   - DTOs (Data Transfer Objects)
   - Application logic and orchestration

3. **CnabParser.Infrastructure** - Infrastructure layer
   - Entity Framework Core data context
   - Repository implementations
   - CNAB parser service implementation
   - Database persistence

4. **CnabParser.Api** - Presentation layer
   - REST endpoints
   - Dependency injection configuration
   - HTTP request handling
   - File upload handling

## Features

? **CNAB File Import** - Upload and parse CNAB transaction files  
? **Store Management** - Automatic store creation and tracking  
? **Transaction Persistence** - Store all transactions in SQL Server  
? **Balance Calculation** - Calculate store balances with transaction history  
? **REST API** - Clean endpoints for all operations  

## Database Schema

### Stores Table
```sql
- Id (PK)
- Name
- Owner
```

### Transactions Table
```sql
- Id (PK)
- StoreId (FK)
- Date
- Type (Enum: Debit, Boleto, Financiamento, etc.)
- Amount (Decimal)
- Cpf
- Card
- Time
- StoreName
- StoreOwner
```

## CNAB File Format

The CNAB file uses a fixed-width format with the following specification:

| Campo | Descrição | Início | Fim | Tamanho | Natureza |
|-------|-----------|--------|-----|---------|----------|
| Tipo | Tipo da transação | 1 | 1 | 1 | - |
| Data | Data da ocorrência | 2 | 9 | 8 | YYYYMMDD |
| Valor | Valor da movimentação | 10 | 19 | 10 | Divide por 100 para normalizar |
| CPF | CPF do beneficiário | 20 | 30 | 11 | - |
| Cartão | Cartão utilizado | 31 | 42 | 12 | - |
| Hora | Hora da ocorrência (UTC-3) | 43 | 48 | 6 | HHMMSS |
| Dono da loja | Nome do representante | 49 | 62 | 14 | - |
| Nome loja | Nome da loja | 63 | 81 | 19 | - |

### Transaction Types

| Tipo | Descrição | Natureza | Sinal |
|------|-----------|----------|-------|
| 1 | Débito | Entrada | + |
| 2 | Boleto | Saída | - |
| 3 | Financiamento | Saída | - |
| 4 | Crédito | Entrada | + |
| 5 | Recebimento Empréstimo | Entrada | + |
| 6 | Vendas | Entrada | + |
| 7 | Recebimento TED | Entrada | + |
| 8 | Recebimento DOC | Entrada | + |
| 9 | Aluguel | Saída | - |

## API Endpoints

### 1. Import CNAB File

**Endpoint:** `POST /api/cnab/import`

**Description:** Upload and import a CNAB file

**Request:**
```
Content-Type: multipart/form-data

file: <binary CNAB.txt>
```

**Response (Success - 200):**
```json
{
  "message": "File imported successfully"
}
```

**Response (Error - 400):**
```json
"File is required"
```

**Example cURL:**
```bash
curl -X POST "https://localhost:7000/api/cnab/import" \
  -F "file=@CNAB.txt"
```

### 2. Get Stores with Balance and Transactions

**Endpoint:** `GET /api/cnab/stores/balance`

**Description:** Retrieve all stores with their transaction history and calculated balance

**Response (200):**
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
        "cpf": "12345678901",
        "card": "123456****7890",
        "time": "121000"
      }
    ]
  },
  {
    "id": 2,
    "name": "LOJA DO Ó - MATRIZ",
    "owner": "MARIA JOSEFINA",
    "balance": 2800.75,
    "transactions": [
      {
        "id": 2,
        "date": "2019-03-01T00:00:00",
        "type": "Boleto",
        "amount": 500.00,
        "cpf": "98765432101",
        "card": "654321****1234",
        "time": "121500"
      }
    ]
  }
]
```

## Running the Application

### Prerequisites
- .NET 8.0 SDK
- SQL Server LocalDB (or configure your own connection string)

### Build
```bash
dotnet build
```

### Run
```bash
cd src/CnabParser.Api
dotnet run
```

The API will start at `https://localhost:7000`

### Access Swagger UI
Navigate to `https://localhost:7000/swagger`

## Configuration

Edit `appsettings.json` to change the database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=CnabParserDb;Trusted_Connection=true;"
  }
}
```

## Database Migration

The database is automatically created on application startup using `EnsureCreated()`. 

For production use, consider using Entity Framework Core migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Usage Example

```bash
# 1. Import CNAB file
curl -X POST "https://localhost:7000/api/cnab/import" \
  -F "file=@CNAB.txt"

# 2. Get all stores with balances
curl -X GET "https://localhost:7000/api/cnab/stores/balance"
```

## Error Handling

The API returns appropriate HTTP status codes:

- `200 OK` - Successful operation
- `400 Bad Request` - Invalid request (missing file, wrong format)
- `500 Internal Server Error` - Server error during processing

## Testing

Run unit tests:

```bash
dotnet test
```

## Dependencies

- **Entity Framework Core 8.0** - ORM
- **Microsoft.EntityFrameworkCore.SqlServer** - SQL Server provider
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation

## Implementation Details

### Parser Logic

The CNAB parser uses fixed-width field parsing:
1. Reads file line by line
2. Extracts fields based on exact position and length specifications
3. Converts date from YYYYMMDD format
4. Normalizes amounts by dividing by 100
5. Maps transaction type codes to enum values
6. Returns list of Transaction objects with embedded store information

### Balance Calculation

Store balance is calculated by summing all transaction signed amounts:
- **Entrada (Inbound)** transactions: Amount added to balance
- **Saída (Outbound)** transactions: Amount subtracted from balance

### Store Grouping

Stores are uniquely identified by the combination of:
- Store Name
- Store Owner

This ensures duplicate stores are not created during import.

## SOLID Principles Applied

? **Single Responsibility** - Each class has one reason to change  
? **Open/Closed** - Open for extension via interfaces  
? **Liskov Substitution** - Repository implementations follow contracts  
? **Interface Segregation** - Focused, single-purpose interfaces  
? **Dependency Inversion** - Depends on abstractions, not concretions  

## Clean Architecture Benefits

- **Testable** - Business logic independent of frameworks
- **Maintainable** - Clear separation of concerns
- **Scalable** - Easy to add new features
- **Flexible** - Can swap implementations without affecting other layers
