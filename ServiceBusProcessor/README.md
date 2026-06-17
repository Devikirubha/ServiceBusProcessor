# Service Bus Processor API

 **.NET 8 Web API** built using **Clean Architecture**.
The system processes messages from Azure Service Bus and persists them into SQL Server, exposing secured REST APIs for querying data.

---

## Features

* .NET 8 Web API
* Clean Architecture 
* Azure Service Bus background processor
* Entity Framework Core (SQL Server)
* JWT Authentication & Role-based Authorization
* API Versioning (v1, v2)
* Global Exception Handling
* Health Checks
* Serilog Structured Logging
* Unit Testing
* Docker Support

---

## Architecture

```
API (Presentation)
   ↓
Application Layer
   ↓
Domain Layer
   ↓
Infrastructure Layer
   ↓
SQL Server
Azure Service Bus
```

---

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

Update configuration in `appsettings.json` or use User Secrets:

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

## API Endpoints

### Messages API

| Method | Endpoint                | Description        |
| ------ | ----------------------- | ------------------ |
| GET    | `/api/v1/messages`      | Get paged messages |
| GET    | `/api/v1/messages/{id}` | Get message by ID  |

### Enhanced API (v2)

| Method | Endpoint                | Description               |
| ------ | ----------------------- | ------------------------- |
| GET    | `/api/v2/messages`      | Paged response (Envelope) |
| GET    | `/api/v2/messages/{id}` | Single message (Envelope) |

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

### Clean Architecture

Used to enforce separation of concerns, improve testability, and support long-term maintainability.

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
