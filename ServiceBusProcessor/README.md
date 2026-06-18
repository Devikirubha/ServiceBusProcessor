# Service Bus Processor API

A scalable message-processing service that demonstrates event-driven integration patterns, secure API design, and production-ready engineering practices using Azure Service Bus, SQL Server, and .NET 8.

---
## Technology Stack

- .NET 8
- ASP.NET Core Web API
- Azure Service Bus
- SQL Server
- Entity Framework Core
- Serilog
- JWT Authentication
- Docker
- Azure Key Vault

## Features

* Asynchronous message processing using Azure Service Bus
* Secure REST APIs with JWT authentication and authorization
* API versioning to support backward compatibility
* Structured logging with Serilog and Correlation IDs
* Centralized exception handling and consistent API responses
* SQL Server persistence using Entity Framework Core
* Health monitoring for infrastructure dependencies
* Unit and integration testing
* Docker-ready deployment

---

## Architecture

```
Azure Service Bus
        │
        ▼
Background Processor
        │
        ▼
Application Layer
        │
        ▼
SQL Server

        ▲
        │

Versioned REST APIs
(JWT Protected)
```
---
## Solution Structure

src/
├── API             # Controllers, middleware, configuration
├── Application     # Use cases, DTOs, interfaces
├── Domain          # Entities and business rules
├── Infrastructure  # EF Core, Service Bus, external services

tests/
├── UnitTests
├── IntegrationTests


## Getting Started

### Prerequisites

* .NET 8 SDK
* SQL Server (local or Docker)
* Azure Service Bus namespace + queue

---

### Run Locally

```bash
# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run application
dotnet run
```

---

## Configuration

The application supports configuration through appsettings files, environment variables, .NET User Secrets or Azure Key Vault.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "AzureServiceBus": {
    "ConnectionString": "",
    "QueueName": ""
  },
  "JwtSettings": {
    "Issuer": "",
    "Audience": "",
    "SecretKey": ""
  }
}
```

---
## Azure Key Vault (Optional)

Sensitive values such as database connection strings, Service Bus credentials, and JWT secrets are intentionally excluded from source control.

The application can load secrets from:

* Azure Key Vault
* Environment Variables
* .NET User Secrets (Development)
* CI/CD Pipeline Variables

### Example

```bash
az keyvault secret set \
  --vault-name MyVault \
  --name ConnectionStrings--DefaultConnection \
  --value "Server=...;Database=...;"
```

```bash
az keyvault secret set \
  --vault-name MyVault \
  --name AzureServiceBus--ConnectionString \
  --value "Endpoint=sb://..."
```

Configure the application by setting:

```bash
KEYVAULT_URI=https://MyVault.vault.azure.net/
```

When `KEYVAULT_URI` is configured, the application automatically loads secrets using `DefaultAzureCredential`.

## API Endpoints

### Messages API

| Method | Endpoint | Description |
|---------|---------|---------|
| GET | /api/v1/messages | Get paged messages |
| GET | /api/v1/messages/{id} | Get message by ID |

### Enhanced API (v2)

| Method | Endpoint | Description |
|---------|---------|---------|
| GET | /api/v2/messages | Get paged messages wrapped in Envelope<T> |
| GET | /api/v2/messages/{id} | Get message by ID wrapped in Envelope<T> |

---

## Authentication

JWT Bearer authentication is required.

### Required Claims

* `scope`: `messages:read`
* `role`: `Admin` (for admin endpoints)

---

## Health Check

| Endpoint  | Description                                  |
| --------- | -------------------------------------------- |
| `/health` | Checks SQL Server & Service Bus connectivity |

---

## Running Tests

```bash
dotnet test
```

---

## Design Decisions

### Separation of Concerns

The solution is organized into distinct layers to isolate business logic, infrastructure concerns, and API responsibilities, improving maintainability and testability.

### Service Bus Processing

A background hosted service listens to Azure Service Bus messages and persists them into SQL Server asynchronously.

### API Versioning

Implemented to ensure backward compatibility while allowing system evolution.

### Error Handling

Centralized global exception handling with consistent API responses.

### Logging

Structured logging implemented using Serilog for observability and troubleshooting.

---

## Production Considerations

* Use Azure Key Vault for secrets management
* Enable HTTPS and strict CORS policies
* Add rate limiting for API protection
* Use managed identity for Azure resources
* Apply database migrations via CI/CD pipeline
* Enable health checks in orchestration layer (Kubernetes / App Service)

---

## Future Enhancements

* OpenTelemetry tracing
* Distributed caching (Redis)
* CI/CD pipeline (GitHub Actions / Azure DevOps)
