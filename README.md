# ShopFlow — Event-Driven E-Commerce Microservices

ShopFlow is a production-grade microservices platform built with ASP.NET Core 10, demonstrating event-driven architecture, a choreography-based Saga pattern, and the distributed-systems design patterns that show up in real production .NET systems: transactional outbox, circuit breakers, database-per-service, idempotent consumers, and an API gateway fronting real-time notifications over SignalR. It's built as a portfolio project — every pattern here is one you'd actually reach for on a production order-fulfillment system, not a toy example.

## Architecture

```
                              ┌───────────────┐
                              │    Client     │
                              └───────┬───────┘
                                      │ HTTP
                                      ▼
                       ┌──────────────────────────┐
                       │   API Gateway (YARP)      │
                       │   http://localhost:5000   │
                       └──────────────┬─────────────┘
                                      │ routes by path
           ┌─────────────┬───────────┼───────────┬─────────────┐
           ▼             ▼           ▼           ▼             │
  ┌────────────────┐┌────────────────┐┌────────────────┐┌────────────────┐
  │  OrderService  ││InventoryService││ PaymentService ││NotificationSvc │
  │  :5001         ││  :5002         ││  :5003         ││  :5004         │
  └───────┬────────┘└───────┬────────┘└───────┬────────┘└───────┬────────┘
          │                 │                 │                 │
          ▼                 ▼                 ▼                 ▼
  ┌────────────────┐┌────────────────┐┌────────────────┐┌────────────────┐
  │ SQL Server      ││ SQL Server     ││ SQL Server     ││ SQL Server     │
  │ ShopFlowOrderDb ││ InventoryDb    ││ PaymentDb      ││ NotificationDb │
  └────────────────┘└────────────────┘└────────────────┘└────────────────┘
          │                 │                 │                 │
          └─────────────────┴────────┬────────┴─────────────────┘
                                      ▼
                          ┌────────────────────────┐
                          │   RabbitMQ (MassTransit) │
                          │   :5672 / mgmt :15672    │
                          └────────────────────────┘
                                      ▲
                                      │ SignalR push
                                      ▼
                          ┌────────────────────────┐
                          │  Browser client         │
                          │  (subscribed via Hub)   │
                          └────────────────────────┘
```

Each service owns its own SQL Server database — no shared schema, no cross-service joins. All inter-service communication happens asynchronously through RabbitMQ via MassTransit; the only synchronous HTTP calls in the system are the client's requests through the Gateway. NotificationService additionally exposes a SignalR hub (`/hubs/notifications`) so browser clients can watch an order's Saga progress in real time.

## Saga Flow

ShopFlow implements the order-fulfillment Saga via **choreography** — there is no central orchestrator. Each service reacts to the events it cares about and publishes the next event in the chain.

### Happy path

```
POST /api/orders
  → OrderService saves order → publishes OrderPlaced via Outbox
  → InventoryService checks stock → reserves → publishes InventoryReserved
  → PaymentService processes payment → publishes PaymentProcessed
  → OrderService status: Confirmed
  → NotificationService pushes real-time update via SignalR
```

### Compensation path (payment failure)

```
PaymentFailed published
  → InventoryService releases stock reservation
  → OrderService status: Cancelled
  → NotificationService pushes failure notification
```

## Key Patterns

| Pattern | Implementation | Where |
|---|---|---|
| Choreography-based Saga | Each service publishes/consumes domain events via MassTransit; no orchestrator | `src/Services/*/Infrastructure/Consumers/` |
| Outbox Pattern | Order + event saved in the same DB transaction; a background processor publishes to RabbitMQ afterward | `OrderService.Infrastructure/BackgroundServices/OutboxProcessor.cs` |
| Database-per-service | Each of the 4 services has its own SQL Server database — `ShopFlowOrderDb`, `ShopFlowInventoryDb`, `ShopFlowPaymentDb`, `ShopFlowNotificationDb` | `docker-compose.yml` |
| Circuit Breaker | Polly circuit breaker wraps the payment gateway call, opening after 3 consecutive failures for 30s | `PaymentService.Infrastructure/Resilience/ResilientPaymentGateway.cs` |
| API Gateway | YARP reverse proxy routes `/api/orders`, `/api/products`, `/api/payments`, `/api/notifications`, `/hubs/*` to the right service | `src/Gateway/ShopFlow.ApiGateway/` |
| Real-time Notifications | SignalR hub broadcasts Saga status updates per order group | `NotificationService.Infrastructure/Hubs/NotificationHub.cs` |
| Clean Architecture | Every service is split into Domain / Application / Infrastructure / API layers | `src/Services/*/` |
| Idempotent Consumers | Handlers check for an existing record before processing, so a redelivered message is a no-op | `PaymentEventHandler.cs`, `InventoryEventHandler.cs` |

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| ASP.NET Core | 10 (net10.0) | Web API host for all 4 services + Gateway |
| Entity Framework Core | 10.0.5 | ORM / migrations per service |
| MassTransit | 8.3.6 | Message bus abstraction over RabbitMQ |
| RabbitMQ | 3-management | Message broker for async events |
| YARP | 2.3.0 | Reverse-proxy API Gateway |
| Polly | 8.6.6 | Circuit breaker around the payment gateway |
| SignalR | (ASP.NET Core 10 built-in) | Real-time order status push to browser clients |
| Docker Compose | — | Single-command local orchestration of all 6 containers |
| FluentValidation | 11.3.1 | Request validation (PaymentService) |
| xUnit | 2.9.3 | Unit test framework |
| Moq | 4.20.72 | Mocking for unit tests |
| SQL Server | 2022 | Database engine, one instance per service |

## Getting Started

**Prerequisites:** Docker Desktop only — no .NET SDK required to run the stack.

```bash
git clone https://github.com/srirakshathirumali/ShopFlow.git
cd ShopFlow
cp .env.example .env
docker-compose up
```

> First run takes 2–3 minutes: Docker needs to pull the SQL Server and RabbitMQ images and each service needs to apply its EF Core migrations before it reports healthy.

## Service URLs

| Service | URL | Notes |
|---|---|---|
| API Gateway | http://localhost:5000 | Single entry point — routes to all 4 services |
| OrderService (Scalar) | http://localhost:5001/scalar/v1 | Interactive API docs |
| InventoryService (Scalar) | http://localhost:5002/scalar/v1 | Interactive API docs |
| PaymentService (Scalar) | http://localhost:5003/scalar/v1 | Interactive API docs |
| NotificationService (Scalar) | http://localhost:5004/scalar/v1 | Interactive API docs |
| RabbitMQ Management | http://localhost:15672 | Login: `guest` / `guest` |
| SignalR Test Page | http://localhost:5004/test.html | Watch Saga events for an order in real time |
| Gateway Health | http://localhost:5000/health | Aggregated health check JSON |

## API Reference

### Place an order

```
POST http://localhost:5000/api/orders
Content-Type: application/json

{
  "customerId": "11111111-1111-1111-1111-111111111111",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "productName": "Laptop",
      "quantity": 1,
      "unitPrice": 999.99
    },
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
      "productName": "Mouse",
      "quantity": 2,
      "unitPrice": 29.99
    }
  ]
}
```

`customerId` isn't validated against a customer record — any GUID works. The IDs above are the seed products InventoryService creates on first startup:

| Product | ID | Stock | Price |
|---|---|---|---|
| Laptop | `3fa85f64-5717-4562-b3fc-2c963f66afa7` | 100 | $999.99 |
| Mouse | `3fa85f64-5717-4562-b3fc-2c963f66afa8` | 200 | $29.99 |
| Keyboard | `3fa85f64-5717-4562-b3fc-2c963f66afa9` | 150 | $79.99 |

### Watch the Saga play out in real time

1. Open http://localhost:5004/test.html
2. Place an order (above) — copy the `id` from the response
3. Paste it into the test page and click **Watch Order**
4. Place another order using the same product IDs
5. Watch each Saga event (`OrderPlaced` → `InventoryReserved` → `PaymentProcessed`/`PaymentFailed`) appear on the page as it happens

## Solution Structure

```
ShopFlow/
├── src/
│   ├── Services/         ← 4 microservices, each its own Clean Architecture (Domain/Application/Infrastructure/API)
│   ├── Gateway/           ← YARP reverse proxy fronting all 4 services
│   └── Shared/             ← ShopFlow.Contracts — shared event contracts every service references
├── tests/                  ← 36 unit tests across 4 test projects
└── docker-compose.yml       ← one command runs everything
```

## Running Tests

There's no `.sln` at the solution root, so run each test project directly:

```bash
dotnet test tests/ShopFlow.OrderService.Tests
dotnet test tests/ShopFlow.InventoryService.Tests
dotnet test tests/ShopFlow.PaymentService.Tests
dotnet test tests/ShopFlow.NotificationService.Tests
```

| Project | Tests |
|---|---|
| ShopFlow.OrderService.Tests | 16 |
| ShopFlow.InventoryService.Tests | 10 |
| ShopFlow.PaymentService.Tests | 5 |
| ShopFlow.NotificationService.Tests | 5 |
| **Total** | **36 tests — 36 passed — 0 failed** |

## Design Decisions

**1. Choreography over Orchestration**
Why: service autonomy, no single point of failure, independent deployment. Each service only needs to know the events it produces and consumes — not the shape of the whole Saga.

**2. Outbox Pattern in OrderService only**
Why: OrderService initiates the Saga — a lost `OrderPlaced` has no recovery path, since nothing else knows the order exists. Downstream services rely on MassTransit's built-in retry, which is sufficient for events that already have an upstream event to fall back on.

**3. Circuit Breaker registered as a Singleton**
Why: circuit-breaker state has to persist across requests to actually count failures. A scoped registration would reset the breaker on every request and it would never open. `ResilientPaymentGateway` is registered as a singleton and calls `IServiceProvider.CreateScope()` internally to resolve its scoped inner `PaymentGateway` per call — avoiding the captive-dependency problem while keeping the breaker state process-wide.

**4. MassTransit 8, not 9**
Why: MassTransit v9 moved to a commercial license; v8.3.6 is the last fully open-source major version.

**5. Explicit queue names in every `ReceiveEndpoint`**
Why: prevents naming collisions when multiple services have consumer classes with identical names — `OrderPlacedConsumer` exists in both InventoryService and NotificationService. Explicit endpoint names (`shopflow-inventory-order-placed`, `shopflow-notification-order-placed`, etc.) keep each service's queue distinct regardless of the consumer class name.

## Known Trade-offs & Scope Limits

**1. No Authentication**
Deliberately omitted — the focus of this project is distributed systems patterns, not auth. In production: JWT validation at the YARP Gateway level, with the Gateway forwarding a verified identity downstream. Auth patterns are already demonstrated in [SecureVaultAPI](https://github.com/srirakshathirumali/SecretVaultAPI) (companion project).

**2. Outbox Pattern in OrderService Only**
Downstream services rely on MassTransit retry rather than an outbox of their own. Extending the outbox to every service is the natural next iteration — the trade-off made here is simplicity vs. strict at-least-once delivery guarantees everywhere.

**3. Stock Reservation Race Condition**
Two concurrent orders can theoretically oversell the same product — there's no optimistic concurrency check on the `Product` entity today. Production fix: a `RowVersion` concurrency token with EF Core optimistic concurrency and a retry on `DbUpdateConcurrencyException`. Omitted here to keep the focus on the Saga and messaging patterns rather than concurrency control.

**4. Migration Retry Loop Duplicated**
Each service's `Program.cs` has its own copy of the "retry migration until the DB is ready" loop. Next refactor: a generic extension method on `IHost`. Deferred so each service's startup sequence stays explicit and readable on its own, without hiding behind a shared abstraction.

**5. `GlobalExceptionMiddleware` Copy-Pasted**
Each service maps its own domain exceptions to HTTP status codes. Next refactor: a shared base class with a `virtual MapException` method per service. Deferred to keep the exception-mapping pattern visible and easy to read per service rather than buried in a shared library.

**6. Circuit Breaker on the Payment Gateway Only**
The payment gateway is the only genuinely flaky external dependency in this system — it simulates real-world failures. The database and SignalR hub are within the system boundary and aren't wrapped in the same resilience policy.

**7. No Distributed Tracing**
Next iteration: OpenTelemetry with correlation IDs propagated through MassTransit message headers, so a single order's journey across all 4 services can be traced end-to-end. Dynatrace is used for distributed tracing in production at Axos Bank.

**8. No Integration Tests**
36 unit tests cover the business logic in each service with mocked dependencies. Next addition: Testcontainers-based integration tests exercising real EF Core migrations, MassTransit wiring, and the full Saga flow against real SQL Server and RabbitMQ instances.


## Author

**Sriraksha Thirumali** — .NET Tech Lead, 14+ years experience
LinkedIn: https://linkedin.com/in/sriraksha-thirumali
Location: San Diego, CA
