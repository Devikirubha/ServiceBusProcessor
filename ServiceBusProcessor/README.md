# Service Bus Processor API

A **production-ready** .NET 8 Clean Architecture API that:
1. Listens to an **Azure Service Bus queue** via a background processor
2. Saves each message to **SQL Server** via EF Core
3. Exposes a **versioned, JWT-secured REST API** to query those messages

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (or Docker)
- Azure Service Bus namespace + queue (or use Azure Service Bus Emulator)

### Run locally

```bash
# 1. Clone and navigate
cd ServiceBusProcessor

# 2. Set user secrets (avoids committing secrets)
cd src/API
dotnet user-secrets set "JwtSettings:SecretKey" "your-local-secret-at-least-32-chars!"
dotnet user-secrets set "AzureServiceBus:ConnectionString" "Endpoint=sb://..."
dotnet user-secrets set "AzureServiceBus:QueueName" "messages-dev"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;"

# 3. Apply EF Core migrations
dotnet ef database update --project ../Infrastructure --startup-project .

# 4. Run
dotnet run
```

### Run with Docker Compose

```bash
# Copy and fill in your Service Bus connection string
cp .env.example .env

docker-compose up --build
```

### Run tests

```bash
dotnet test tests/Tests/Tests.csproj --verbosity normal
```

---

## API Endpoints

### v1

| Method | Route | Policy | Description |
|---|---|---|---|
| `GET` | `/api/v1/messages` | `messages:read` | Paged list of messages |
| `GET` | `/api/v1/messages/{id}` | `messages:read` | Single message by ID |

### v2 (enhanced envelope)

| Method | Route | Policy | Description |
|---|---|---|---|
| `GET` | `/api/v2/messages` | `messages:read` | Paged list wrapped in `Envelope<T>` |
| `GET` | `/api/v2/messages/{id}` | `messages:read` | Single message in `Envelope<T>` |
| `GET` | `/api/v2/messages/admin/summary` | `admin` | Admin summary (requires Admin role) |

### Health

| Route | Description |
|---|---|
| `GET /health` | SQL Server + Service Bus health |

---

## JWT Token Requirements

Tokens must include:
- `iss` — matches `JwtSettings:Issuer`
- `aud` — matches `JwtSettings:Audience`
- `scope` — `messages:read` and/or `messages:write`
- `role` — `Admin` (for admin endpoints)

---

## EF Core Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure \
  --startup-project src/API

# Apply to database
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/API
```

---

## Environment Configuration

| Environment | File | Secret handling |
|---|---|---|
| Development | `appsettings.Development.json` | `dotnet user-secrets` |
| TST | `appsettings.TST.json` | Token placeholders `#{...}#` replaced by CI/CD |
| UAT | `appsettings.UAT.json` | Token placeholders replaced by CI/CD |
| Production | `appsettings.Production.json` | Token placeholders replaced by CI/CD |

---

## Serilog Logs table

This project uses `Serilog.Sinks.MSSqlServer` to optionally write structured logs to a SQL Server table named `Logs`.

A SQL script is provided at `scripts/create_serilog_logs.sql` to create the table. Run it as a DBA or in your deployment pipeline before enabling the MSSqlServer sink in production (`autoCreateSqlTable: false` recommended).

Run the script using `sqlcmd` or SSMS:

```powershell
# LocalDB / SQL Server (example)
sqlcmd -S "(localdb)\MSSQLLocalDB" -d ServiceBusProcessorDev -i scripts/create_serilog_logs.sql

# Azure SQL
sqlcmd -S "tcp:<your-server>.database.windows.net,1433" -U "<user>@<your-server>" -P "<password>" -d "<database>" -i scripts/create_serilog_logs.sql
```

