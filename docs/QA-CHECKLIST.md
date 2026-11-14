# KinderHub QA Checklist & Bug Log

Manual testing done in addition to the 78 automated tests.
Mark each case ✅ (pass) or ❌ (fail) and write the date. Fill the browser/device table at the end.

**Tester:** Farzana Khashi · **Version:** final project · **Date:** 14.11.2026 · **Browser used:** Microsoft Edge (Windows)

## 1. Authentication & roles

| # | Test case | Steps | Expected | Result |
|---|---|---|---|---|
| A1 | Login works | Log in as admin, teacher, parent | Correct name + role in the top bar | ✅ |
| A2 | Wrong password | Log in with a wrong password | Error message, not logged in | ⬜ not tested yet |
| A3 | Role menu | Compare menus of the 3 roles | Admin: Groups + System status; Parent: My children only | ✅ |
| A4 | Protected page | As parent open `/children` | "Access denied" | ✅ |
| A5 | Protected export | Logged out, open `/export/attendance.csv` | Redirect to login | ⬜ not tested yet |
| A6 | Logout | Click Logout | Back to home, menu shows Login | ✅ |

## 2. Children & groups

| # | Test case | Steps | Expected | Result |
|---|---|---|---|---|
| C1 | Search | Type "ser" | Only the Serikov family | ✅ |
| C2 | Filter + paging | Filter by group, use page buttons | Correct rows, page counter | ⬜ not tested yet |
| C3 | Add child | Admin → Add child with valid data | Toast "Child added", row appears | ⬜ not tested yet |
| C4 | Validation | Empty name, email "abc", birth date this year | Red messages, not saved | ✅ |
| C5 | Full group | Group capacity 1, add 2 children | 2nd child refused: "is full" | ⬜ manual not done (✅ automated test) |
| C6 | Delete child | Delete → confirm dialog → Delete | Child and attendance removed | ⬜ not tested yet |
| G1 | Duplicate group | Create group "sunflowers" | "already exists" | ✅ |
| G2 | Delete non-empty group | Delete Sunflowers | Refused with clear message | ✅ confirm dialog shown (refusal ✅ automated test) |

## 3. Attendance, reports, announcements

| # | Test case | Steps | Expected | Result |
|---|---|---|---|---|
| T1 | Mark status | Click Present / Late / Absent / Sick | Buttons + summary chips update, toast on error only | ✅ |
| T2 | Check-out | Present → Check out | Time shown, "Picked up" | ✅ |
| T3 | Future date | Choose tomorrow | Warning, not allowed | ⬜ not tested yet |
| T4 | Mark all present | Click the button | All unmarked children present | ⬜ not tested yet |
| R1 | Write report | Write report for Aruzhan | Saved, button becomes Edit | ⬜ not tested yet |
| N1 | Group announcement | Publish "Only Sunflowers" | Parent of Sunflowers child sees it; others don't | ⬜ not tested yet |

## 4. Real-time (two windows: teacher + parent in private window)

| # | Test case | Expected | Result |
|---|---|---|---|
| L1 | Live badge | Green "● Live" in both windows | ✅ |
| L2 | Arrival | Parent gets "✅ … arrived at …" without refresh | ⬜ not tested yet |
| L3 | Pick-up | Parent gets "👋 … was picked up at …" | ⬜ not tested yet |
| L4 | Report | Parent gets "📝 New daily report …" and sees it | ⬜ not tested yet |
| L5 | Announcement | Pop-up + orange counter in the menu, cleared on the page | ⬜ not tested yet |
| L6 | Reconnect | Stop + start the server → badge Offline → Live again | ⬜ not tested yet |

## 5. UI, UX, accessibility, PWA

| # | Test case | Expected | Result |
|---|---|---|---|
| U1 | Dark mode | All pages readable; choice remembered after refresh | ✅ |
| U2 | Keyboard only | Tab through menu, buttons, forms; Esc closes dialogs; focus visible | ⬜ not tested yet |
| U3 | Skip link | Press Tab on a page → "Skip to main content" | ⬜ not tested yet |
| U4 | Empty / loading states | Lists show "Loading..." and friendly empty messages | ⬜ not tested yet |
| U5 | Chart | Hover/focus shows tooltip; "Table" shows the same data | ✅ table view |
| U6 | CSV | Export opens in Excel with correct letters | ⬜ not tested yet |
| U7 | Install (PWA) | Install icon in Chrome/Edge → app window | ⬜ not tested yet |
| U8 | Offline page | Installed app, server stopped, reload → "You are offline" page | ⬜ not tested yet |
| U9 | Health | `/health` → Healthy; System status page shows cards | ✅ |

## 6. Cross-browser & cross-device matrix

Use real devices or the browser's device mode (F12 → Ctrl+Shift+M).

| Browser / device | Screen | Login | Menu | Attendance | Dark mode | Live | Result |
|---|---|---|---|---|---|---|---|
| Chrome (Windows) | 1920×1080 | | | | | | |
| Edge (Windows) | 1920×1080 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Firefox (Windows) | 1920×1080 | | | | | | |
| Safari / Chrome (macOS) *if available* | 1440×900 | | | | | | |
| iPhone 12 (device mode) | 390×844 | | ☰ menu | | | | |
| Galaxy S20 (device mode) | 360×800 | | ☰ menu | | | | |
| iPad (device mode) | 820×1180 | | | | | | |

## 7. Bug log (found and fixed during development)

| # | Bug | Cause | Fix | Status |
|---|---|---|---|---|
| B1 | Build failed after adding EF Core packages | NuGet installed .NET 11 packages into a .NET 10 project | Pinned versions `10.0.*`, reinstalled `dotnet-ef` 10 | ✅ Fixed |
| B2 | `/children` returned "Not Found" | Old app instance still running after adding the page | Stop, rebuild, run; documented in README | ✅ Fixed |
| B3 | Parent could reach staff pages by URL | Pages had no role restriction yet | `[Authorize(Roles = ...)]` on every page + endpoint | ✅ Fixed |
| B4 | Chart build error RZ1023 | Razor treats `<text>` as a special tag inside code | Wrapped SVG `<text>` in `<g>` | ✅ Fixed |
| B5 | Risk (code review): duplicate attendance if two teachers save the same child at once | Race condition | Unique index + one retry as update | ✅ Fixed |
| B6 | Risk (code review): group child counts could be stale after caching was added | Groups are cached | Cache invalidated on child/group changes (covered by a test) | ✅ Fixed |
| B7 | Check-out button missing for a child | Child was already checked out (expected behavior) | No change; explained in User Guide | ✅ By design |
| B8 | `appsettings.json` broken after manual edit | Missing brackets | Restored valid JSON | ✅ Fixed |
| | | | | |
