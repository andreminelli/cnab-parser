# CNAB Parser

This repository contains a .NET application designed to parse and process CNAB files as described in [this repo](https://github.com/ByCodersTec/desafio-ruby-on-rails).

It leverages a modular architecture, separating concerns into different layers for maintainability and scalability.

## Technologies and Tools Used

*   **Backend:** C#, .NET (targeting .NET 9.0), ASP.NET Core
*   **Orchestration:** .NET Aspire
*   **Database:** SQL Server (via Entity Framework Core)
*   **Object-Relational Mapper (ORM):** Entity Framework Core
*   **Containerization:** Docker, Docker Compose
*   **Testing Frameworks:** xUnit, NSubstitute
*   **Build/Run Scripts:** PowerShell (`.ps1`), Bash (`.sh`)

## Project Structure Overview

The solution is organized into several projects, each with a specific responsibility:

*   **`src/CnabParser.Api`**: An ASP.NET Core Web API project responsible for exposing endpoints to interact with the CNAB parsing functionality.
*   **`src/CnabParser.AppHost`**: The .NET Aspire AppHost project, used to define and orchestrate the various services (API, Web, database) within the application.
*   **`src/CnabParser.Application`**: The application layer, containing services (`CnabImporterService`) that implement the core business logic and coordinate operations between the domain and infrastructure layers.
*   **`src/CnabParser.Core`**: The core domain layer, defining the fundamental business entities (`Store`, `Transaction`), value objects, and interfaces for repositories (`IStoreRepository`, `ITransactionRepository`, `IUnitOfWork`) and services (`ICnabFileParser`).
*   **`src/CnabParser.Infrastructure`**: The infrastructure layer, providing concrete implementations for the interfaces defined in the `Core` layer. This includes data access (Entity Framework Core `CnabDbContext`, `StoreRepository`, `TransactionRepository`, `UnitOfWork`) and the CNAB file parsing logic (`CnabFileParser`).
*   **`src/CnabParser.ServiceDefaults`**: A shared project for common configurations, extensions, and utilities used across different Aspire services.
*   **`src/CnabParser.Web`**: An ASP.NET Core Web application, likely serving as a frontend or UI to interact with the API and display processed CNAB data.
*   **`tests/`**: Contains various test projects (e.g., `CnabParser.Api.IntegrationTests`, `CnabParser.Application.Tests`, `CnabParser.Core.Tests`, `CnabParser.Infrastructure.Tests`) to ensure the quality and correctness of the application.

## How to Run

You can run this application using either the provided helper scripts or Docker Compose.

### Using Helper Scripts (`run.ps1` or `run.sh`)

The project includes platform-specific scripts to simplify running the application, typically leveraging the .NET Aspire AppHost.

**On Windows (PowerShell):**

```bash
.\run.ps1
```

**On Linux/macOS (Bash):**

```bash
./run.sh
```

These scripts will usually build the projects and start the `CnabParser.AppHost` which in turn launches all configured services, including the API, Web frontend, and any dependent databases or services.

### Using Docker Compose

The application can also be run using Docker Compose, which will build and orchestrate the services as containers.

1.  **Build and run the containers:**
    ```bash
    docker-compose up --build
    ```
2.  **Access the application:**
    Once the containers are up and running, you can typically access the web application or API at the addresses specified in your `launchSettings.json` files or the Aspire dashboard. Common addresses are `http://localhost:5000` or `http://localhost:8080` for the web frontend and `http://localhost:5001` or `http://localhost:8081` for the API, but these may vary based on configuration.

3.  **Stop the containers:**
    ```bash
    docker-compose down
    ```
