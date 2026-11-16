# KinderHub API Documentation

KinderHub is mainly a Blazor app, but it exposes a few HTTP endpoints and a SignalR hub.
The machine-readable **OpenAPI** document is available at **`/openapi/v1.json`** while the app is running.

## HTTP endpoints

### `GET /health`
Health check for monitoring tools. **No login required.**

Response `200 OK` (`application/json`):
```json
{
  "status": "Healthy",
  "durationMs": 12.3,
  "checks": [
    { "name": "database", "status": "Healthy", "description": "Database is reachable.", "data": { "children": 12 } }
  ]
}
```
Returns `503 Service Unavailable` with `"status": "Unhealthy"` if the database cannot be reached.

### `GET /export/attendance.csv?groupId={id}`
Downloads attendance records as CSV (UTF-8 with BOM, opens in Excel).

| | |
|---|---|
| Auth | Logged in as **Admin** or **Teacher** (cookie). Otherwise `302` redirect to `/Account/Login`. |
| Query | `groupId` (optional, int) — only this group. |
| Response | `200 OK`, `text/csv`, file `attendance-YYYY-MM-DD.csv` |
| Columns | `Date, Child, Status, Check-in, Check-out, Picked up by, Note` |
| Security | Cells starting with `= + - @` are prefixed with `'` (CSV-injection protection). Max 10 000 rows. |

### `GET /openapi/v1.json`
OpenAPI 3 description of the endpoints above.

### Account endpoints (ASP.NET Core Identity)
`/Account/Login`, `/Account/Register`, `/Account/Logout` (POST, antiforgery token), `/Account/Manage/*` — provided by the Identity template.

## SignalR hub

**URL:** `/hubs/notifications` (WebSockets, automatic reconnect)

| Message (server → client) | Payload | When |
|---|---|---|
| `AnnouncementPublished` | `{ id, title, groupId, isImportant, isNew }` | A new announcement is published |

Example .NET client:
```csharp
var hub = new HubConnectionBuilder()
    .WithUrl("https://your-server/hubs/notifications")
    .WithAutomaticReconnect()
    .Build();

hub.On<AnnouncementEvent>("AnnouncementPublished", e => Console.WriteLine($"New: {e.Title}"));
await hub.StartAsync();
```

## Internal service API (Application layer)

| Interface | Main methods |
|---|---|
| `IChildService` | `GetPagedAsync(ChildQuery)`, `GetByGroupAsync`, `GetByParentEmailAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` |
| `IGroupService` | `GetAllAsync` (cached), `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` |
| `IAttendanceService` | `GetDailySheetAsync(groupId, date)`, `GetChildHistoryAsync`, `GetHistoryAsync(groupId, skip, take)`, `SaveAsync` (create or update) |
| `IDailyReportService` | `GetForChildAsync`, `GetForGroupAndDateAsync`, `SaveAsync`, `DeleteAsync` |
| `IAnnouncementService` | `GetLatestAsync(groupIds?)`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` |
| `IDashboardService` | `GetStatsAsync()` |
| `IRealtimeNotifier` | `PublishAsync(...)` + events `AttendanceChanged`, `ReportSaved`, `AnnouncementChanged` |
| `ICurrentUserService` | `GetCurrentUserAsync()` → email, display name, roles |

All write methods return `Result` / `Result<T>`: `Succeeded` + a user-friendly `Error` message.
