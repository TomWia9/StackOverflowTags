# Stack Overflow Tags Explorer

Full-stack app built with **.NET 10 Minimal API + Clean Architecture + CQRS/MediatR** and **Angular 19**.

## Quick Start

```bash
git clone https://github.com/TomWia9/stackoverflow-tags.git
cd stackoverflow-tags
docker compose up --build
```

| Service            | URL                                   |
| ------------------ | ------------------------------------- |
| Frontend (Angular) | http://localhost                      |
| API (Minimal API)  | http://localhost:5000                 |
| API Docs (Scalar)  | http://localhost:5000/scalar/v1       |
| OpenAPI JSON       | http://localhost:5000/openapi/v1.json |

> On first start the backend fetches 1000+ tags from the Stack Overflow API. Data persists in a Docker volume between restarts.

---

## Architecture

```
backend/src/
├── Domain/              # Entities, repository & client interfaces
├── Application/         # CQRS handlers, validators, MediatR pipeline behaviours
├── Infrastructure/      # EF Core (SQLite), Stack Overflow HTTP client implementation
└── Api/                 # Minimal API endpoints, DI composition root

frontend/src/app/
└── features/tags/
    ├── models/          # TypeScript interfaces
    ├── services/        # HTTP service (TagsApiService)
    ├── store/           # Signal-based state (TagsStore)
    └── components/      # TagsTableComponent
```

### Backend highlights

- **Clean Architecture** — Domain has zero external dependencies
- **CQRS via MediatR** — `GetTagsQuery` / `FetchTagsCommand` / `EnsureTagsLoadedCommand`
- **MediatR pipeline behaviours** — `LoggingBehaviour` + `ValidationBehaviour`
- **FluentValidation** — auto-registered, wired into the pipeline
- **Minimal API** with typed results (`Results<Ok<T>, BadRequest<string>>`)
- **Scalar UI** — modern OpenAPI explorer
- **SQLite** via EF Core

### Frontend highlights

- **Angular 19** — standalone components, `ChangeDetectionStrategy.OnPush`
- **Signal-based state** (`TagsStore`) using `signal()`, `computed()`, `effect()`
- **`@if` / `@for`** — new Angular control-flow syntax (no `*ngIf` / `*ngFor`)
- **`takeUntilDestroyed`** — RxJS + Angular 16+ cleanup
- **`provideHttpClient(withFetch())`** — Fetch API instead of XMLHttpRequest

---

## API Reference

### `GET /api/tags`

| Param       | Type   | Default | Values               |
| ----------- | ------ | ------- | -------------------- |
| `page`      | int    | 1       | ≥ 1                  |
| `pageSize`  | int    | 25      | 1–100                |
| `sortBy`    | string | `name`  | `name`, `percentage` |
| `sortOrder` | string | `asc`   | `asc`, `desc`        |

### `POST /api/tags/refresh`

Forces re-fetch from Stack Overflow API, recomputes percentages, replaces DB contents.

---

## Running Tests

```bash
dotnet test StackOverflowTags.sln
```

### Unit tests (`tests/UnitTests/`)

- `TagEntityTests` — domain entity invariants
- `GetTagsQueryValidatorTests` — FluentValidation rules
- `GetTagsQueryHandlerTests` — pagination & sorting logic (InMemory DB)
- `FetchTagsCommandHandlerTests` — percentage computation, replace semantics

### Integration tests (`tests/IntegrationTests/`)

- Full HTTP tests via `WebApplicationFactory<Program>`
- Mocked `IStackOverflowClient`, InMemory DB
- Tests: 200/400 responses, sort orders, pagination, percentage sum, refresh, OpenAPI endpoint
