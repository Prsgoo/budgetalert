# BudgetAlert

![CI](https://github.com/Prsgoo/BudgetAlert/actions/workflows/ci.yml/badge.svg)

Event-driven budget alerting REST API built with .NET 10, Clean Architecture, and CQRS.

Tracks budgets and fires alerts when spending thresholds are crossed. The messaging layer is provider-agnostic: RabbitMQ locally, Azure Service Bus in production - switchable via config with no code changes. Authentication is similarly toggled: off for local development, Entra ID JWT validation in production.

## Architecture

```
[HTTP Client] --> [ASP.NET Core API]
                        |
                        | CQRS via MediatR
                        v
                 [Application Layer]
                 handlers, validators,
                 pipeline behaviors
                        |
               [Domain Layer] <-- pure C#, no deps
                        |
              [Infrastructure Layer]
               EF Core     IEventBus (pluggable)
                                  |
                    RabbitMQ / Azure Service Bus
                                  |
                      [.NET Worker Service]
                       evaluates alert rules
                       writes Alert to DB
```

**Layer rules:** Domain has no outward dependencies. Application depends only on Domain. Infrastructure and API depend inward - never outward.

## Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 10, MediatR, FluentValidation |
| Domain | Pure C#, no NuGet deps |
| Persistence | EF Core 10, SQL Server / Azure SQL |
| Messaging | RabbitMQ (local) or Azure Service Bus (production) |
| Auth | Entra ID JWT via Microsoft.Identity.Web (toggle off for local dev) |
| Passwordless access | DefaultAzureCredential (Azure SQL + Service Bus - no secrets in config) |
| Background processing | .NET Worker Service |
| Testing | xUnit, Moq, FluentAssertions, Testcontainers, WebApplicationFactory |
| CI/CD | GitHub Actions - build + test on PR, container publish and deploy on merge |

## Local Setup

**Prerequisites:** Docker Desktop

```bash
git clone https://github.com/Prsgoo/BudgetAlert.git
cd BudgetAlert

# Start dependencies only
docker compose up -d sqlserver rabbitmq

# Run the API
dotnet run --project src/BudgetAlert.Api

# In another terminal, run the Worker
dotnet run --project workers/BudgetAlert.Worker
```

Or run the full stack including API and Worker in Docker:

```bash
docker compose up
```

| Service | URL |
|---|---|
| API | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| RabbitMQ management | http://localhost:15672 (guest/guest) |
| SQL Server | localhost:1433 (sa/DevPassword123!) |

## API Quick Reference

```bash
# Create a budget
curl -X POST http://localhost:8080/api/budgets \
  -H "Content-Type: application/json" \
  -d '{ "name": "Monthly Groceries", "limit": 500.00, "currency": "EUR" }'

# Add an alert rule (fire at 80% spend)
curl -X POST http://localhost:8080/api/budgets/{id}/rules \
  -H "Content-Type: application/json" \
  -d '{ "thresholdPercentage": 80 }'

# Register a transaction
curl -X POST http://localhost:8080/api/budgets/{id}/transactions \
  -H "Content-Type: application/json" \
  -d '{ "amount": 420.00, "description": "Supermarket", "occurredAt": "2026-09-27T10:00:00Z" }'

# Get budget with recent transactions
curl http://localhost:8080/api/budgets/{id}

# Get alerts (optionally filtered by budget)
curl http://localhost:8080/api/alerts?budgetId={id}
```

## Development

```bash
dotnet build BudgetAlert.slnx          # build all projects
dotnet test BudgetAlert.slnx           # full test suite (unit + integration)
dotnet test tests/BudgetAlert.Api.Tests # single project
dotnet format                           # format code
```

## Project Structure

```
src/
  BudgetAlert.Domain/               # Entities, domain events, interfaces - no dependencies
  BudgetAlert.Application/          # CQRS handlers, validators, pipeline behaviors
  BudgetAlert.Infrastructure/       # EF Core DbContext, pluggable messaging publishers
  BudgetAlert.Api/                  # Controllers, middleware, DI wiring
workers/
  BudgetAlert.Worker/               # Message consumer + alert rule evaluator
tests/
  BudgetAlert.Domain.Tests/
  BudgetAlert.Application.Tests/
  BudgetAlert.Infrastructure.Tests/
  BudgetAlert.Api.Tests/            # Controller tests with fake repositories
  BudgetAlert.Api.IntegrationTests/ # Integration tests with Testcontainers (real SQL Server)
  BudgetAlert.Worker.Tests/
infra/
  main.bicep                        # Azure SQL + Service Bus provisioning
  main.bicepparam                   # Deployment parameters
```

## Environment Configurations

The app has three independent config dimensions. Any combination is valid:

| Messaging | Database | Auth | Notes |
|-----------|----------|------|-------|
| RabbitMQ | Local SQL | Off | Default - nothing to change |
| ASB emulator | Local SQL | Off | `Messaging:Provider=AzureServiceBus`, set `ConnectionString` |
| Azure Service Bus | Azure SQL | Off | Passwordless via DefaultAzureCredential, no keys in config |
| Azure Service Bus | Azure SQL | On | Full production stack - Entra ID JWT + Managed Identity |

### RabbitMQ (default for local development)

```bash
docker compose --profile rabbitmq up -d sqlserver rabbitmq
dotnet run --project src/BudgetAlert.Api
dotnet run --project workers/BudgetAlert.Worker
```

### Azure Service Bus emulator (local, zero cloud cost)

```bash
docker compose --profile azureservicebus up -d sqlserver sqledge servicebus-emulator
```

Copy `appsettings.AzureServiceBus.example.json` values into `appsettings.json` for both Api and Worker, then run the projects normally. The emulator auto-creates the queue on first message.

### Azure (production)

See [infra/README.md](infra/README.md) for provisioning and configuration.

## Design Decisions

- **Provider-agnostic messaging** - `IEventBus` abstraction lets local development use RabbitMQ without a cloud account. The producer/consumer pattern is identical; switching providers is a config change. The same approach applies when targeting other brokers (Kafka, Azure Event Hubs).
- **Auth toggle** - `Auth:Enabled=false` runs a permissive no-op authorization policy so `[Authorize]` attributes are present in code but don't block local requests. This avoids maintaining two code paths while keeping the security surface clear in production.
- **DefaultAzureCredential for passwordless access** - no connection strings with passwords or SAS keys in production config. Locally, `az login` satisfies the credential chain; Managed Identity takes over when hosted in Azure.
- **MediatR for CQRS** - pipeline behaviors (logging, validation) run on every command and query without touching handler code.
- **Worker Service over Azure Functions** - same `BackgroundService` pattern, no cloud dependency for local development or CI.
- **EF Core private setters** - domain model enforces invariants; persistence is an infrastructure concern and should not dictate how entities are constructed.

## License

MIT - see [LICENSE](LICENSE)
