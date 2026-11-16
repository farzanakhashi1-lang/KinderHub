# KinderHub tests

| Folder | Type | What is tested |
|---|---|---|
| `Unit/` | Unit tests | Validation rules, Result/PagedResult, age calculation, CSV export + CSV-injection protection, toast service, display helpers |
| `Services/` | Integration tests (real SQLite in memory) | Child/Group/Attendance/Report/Announcement/Dashboard services: CRUD, business rules, caching, paging, live events |
| `Components/` | Component tests (bUnit) | Pager, ConfirmDialog/Modal, DataTable, AttendanceBadge, StatCard, Icon |
| `Integration/` | End-to-end HTTP tests (WebApplicationFactory) | `/health`, home page, CSV export security, PWA manifest |

Run all tests:

    dotnet test

Run with code coverage + HTML report:

    dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
    reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:Html

Then open `coverage-report/index.html`.
