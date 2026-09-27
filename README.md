# BudgetAlert

![CI](https://github.com/Prsgoo/BudgetAlert/actions/workflows/ci.yml/badge.svg)

Event-driven budget alerting REST API built with .NET 10, Clean Architecture, CQRS, and RabbitMQ.

A REST API that tracks budgets and fires alerts when spending thresholds are crossed. Built as a portfolio project to demonstrate Clean Architecture, CQRS, and event-driven patterns in .NET - the same patterns used in production Azure Service Bus deployments.

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
               EF Core     RabbitMQ publisher
                                  |
                           [RabbitMQ]
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
| Persistence | EF Core 10, SQL Server |
| Messaging | RabbitMQ (patterns identical to Azure Service Bus) |
| Background processing | .NET Worker Service |
| Testing | xUnit, Moq, FluentAssertions, Testcontainers, WebApplicationFactory |
| CI | GitHub Actions (build + test on PR, release build on merge to main) |

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
  BudgetAlert.Infrastructure/       # EF Core DbContext, RabbitMQ publisher
  BudgetAlert.Api/                  # Controllers, middleware, DI wiring
workers/
  BudgetAlert.Worker/               # RabbitMQ consumer + alert rule evaluator
tests/
  BudgetAlert.Domain.Tests/
  BudgetAlert.Application.Tests/
  BudgetAlert.Infrastructure.Tests/
  BudgetAlert.Api.Tests/            # Controller tests with fake repositories
  BudgetAlert.Api.IntegrationTests/ # Integration tests with Testcontainers (real SQL Server)
  BudgetAlert.Worker.Tests/
```

## Design Decisions

- **RabbitMQ over Azure Service Bus** - no cloud account needed locally; the producer/consumer pattern is identical and swapping is one NuGet package change.
- **MediatR for CQRS** - pipeline behaviors are the key value: logging and validation run on every command/query without touching handlers.
- **Worker Service over Azure Functions** - same `BackgroundService` pattern, no cloud dependency for local development or CI.
- **EF Core private setters** - domain model enforces invariants; persistence is an infrastructure concern and should not dictate how entities are constructed.

## License

MIT - see [LICENSE](LICENSE)
