# KinderHub Developer Notes

## 1. Solution layout

| Project | Responsibility | Depends on |
|---|---|---|
| `KinderHub.Domain` | Entities (`Child`, `Group`, `AttendanceRecord`, `DailyReport`, `Announcement`), enums | — |
| `KinderHub.Application` | Service interfaces, DTOs + edit models (DataAnnotations, `IValidatableObject`), `Result`, `PagedResult`, `ModelValidator`, `Roles` | Domain |
| `KinderHub.Infrastructure` | `KinderHubDbContext`, Fluent API configurations, seeding, service implementations, caching, retry strategy | Application |
| `KinderHub.Web` | Blazor components/pages, layout, forms, `AppState`, `ToastService`, SignalR hub + notifier, health checks, minimal API endpoints, Identity | Application, Infrastructure |
| `KinderHub.Tests` | xUnit, bUnit, WebApplicationFactory tests | all |

## 2. Key design decisions

### Clean Architecture + SOLID
* **S**ingle responsibility: one service per area (`ChildService`, `AttendanceService` …); forms, tables and dialogs are separate components.
* **O**pen/closed: new features are new services/components; e.g. caching was added to `GroupService` without changing its callers.
* **L**iskov / **I**nterface segregation: small interfaces (`IChildService`, `IRealtimeNotifier`, `ICurrentUserService`).
* **D**ependency inversion: pages depend on interfaces from Application; implementations are registered in `DependencyInjection.cs` / `WebServices.cs`.

### Data access
* **`IDbContextFactory`** — Blazor Server circuits live long; each operation creates a short-lived `DbContext` (thread-safe, no stale tracking).
* **Projection with `Select`** + **`AsNoTracking`** — only needed columns are loaded, read-only.
* **Fluent API** — lengths, required fields, unique indexes (one attendance record / report per child per day, unique group names), delete behaviors (Restrict / Cascade / SetNull).
* **Two databases** — `app.db` (Identity, migrations from the template) and `kinderhub.db` (domain, `EnsureCreated`). Parents are linked to children by **email**.
* **Retry strategy** (`SqliteRetryStrategy`) for SQLITE_BUSY/LOCKED and a **one-time retry** on unique-constraint races (two teachers saving the same child at once).

### Validation & errors
1. DataAnnotations + `IValidatableObject` on edit models → shown in forms (`DataAnnotationsValidator`).
2. **Same rules again on the server** (`ModelValidator.Validate`) in every Create/Update.
3. Business rules in services (capacity, deletable group, unique names).
4. Services return **`Result` / `Result<T>`** instead of throwing; pages show the message in a toast.
5. `ErrorBoundary` in `MainLayout` isolates page crashes; it recovers on navigation.
6. Logging with `ILogger<T>` (structured, with ids).

### State management
* **`AppState`** (scoped = per browser tab/circuit): dark mode (persisted with `ProtectedLocalStorage`), unread announcements counter. Components subscribe to `OnChange` and unsubscribe in `Dispose`.
* **`CascadingAuthenticationState`** provides the user to every component; `ICurrentUserService` wraps it.
* **`ToastService`**: app-wide notifications.

### Real-time
* `IRealtimeNotifier` (singleton) — services **publish** `AttendanceChangedEvent`, `ReportSavedEvent`, `AnnouncementEvent` after a successful save.
* Pages (`Home`, `Attendance`, `MyChildren`, `Announcements`) **subscribe**, filter (a parent only reacts to own children) and re-render via `InvokeAsync(StateHasChanged)`. Blazor Server pushes the new UI over its SignalR circuit.
* `NotificationHub` (`/hubs/notifications`) broadcasts new announcements to hub clients; `LiveUpdates.razor` is a .NET SignalR client with automatic reconnect and the Live/Offline badge.

### Performance
`Virtualize` (history), server-side paging, `IMemoryCache` for groups (invalidated on change), debounced search, throttled dashboard reloads, `@key` in lists, `MapStaticAssets` (compression + fingerprinting).

### Reusable components (`Components/Shared`)
`Modal`, `ConfirmDialog`, `DataTable<TItem>` (generic, templated), `Pager` (`@bind-Page`), `SearchBox` (`@bind-Value`, debounce), `FormField<TValue>`, `StatCard`, `Badge`, `AttendanceBadge`, `EmptyState`, `LoadingSpinner`, `Icon`, `BarChart` (SVG), `ToastContainer`, `ThemeToggle`.

## 3. How to add a new feature (example: "Medical notes")

1. **Domain** — add the entity in `Entities/`.
2. **Infrastructure** — add a `DbSet` + configuration in `EntityConfigurations.cs`.
3. **Application** — add DTO + edit model (with validation) and `IMedicalNoteService`.
4. **Infrastructure** — implement the service (use `ModelValidator`, return `Result`), register it in `DependencyInjection.cs`.
5. **Web** — create a page in `Components/Pages` with `[Authorize(Roles = ...)]`, reuse `DataTable`, `Modal`, `FormField`; add a `NavLink` in `NavMenu.razor`.
6. **Tests** — add service tests using `TestDatabase` and component tests with bUnit.

> Because `kinderhub.db` uses `EnsureCreated`, delete the file after changing the model (demo data is re-seeded). For production, switch to EF Core migrations.

## 4. Conventions
* Async everywhere, `CancellationToken` on service methods.
* Pages: `PageHeader` → toolbar → content; loading / empty / error states always handled.
* CSS: one design system in `wwwroot/app.css` with CSS variables (light + dark).
* Accessibility: `aria-label` on icon buttons, `role="dialog"`/`aria-modal` on modals, keyboard focus, skip link, color never the only signal.

## 5. Known limitations & future work
* SQLite is ideal for a demo / small kindergarten; for many users switch to SQL Server or PostgreSQL (change `UseSqlite` in `DependencyInjection.cs`).
* The hub broadcasts only announcement titles and allows anonymous connections (no sensitive data). With more clients, add `[Authorize]` and SignalR groups per kindergarten group.
* With several servers, use a SignalR backplane (Redis / Azure SignalR) and a distributed cache.
* Email confirmation uses the template's no-op sender; connect a real email service for production.
* Possible next features: photos in daily reports, parent–teacher chat, multi-language UI (English / Russian / Kazakh).
