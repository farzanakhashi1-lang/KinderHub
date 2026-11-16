using KinderHub.Application.Common;
using KinderHub.Application.DTOs;

namespace KinderHub.Application.Interfaces;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceRowDto>> GetDailySheetAsync(int groupId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceDto>> GetChildHistoryAsync(int childId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<Result> SaveAsync(AttendanceEditModel model, CancellationToken ct = default);

    // All records, newest first, loaded in small pieces (for virtual scrolling and CSV export)
    Task<PagedResult<AttendanceDto>> GetHistoryAsync(int? groupId, int skip, int take, CancellationToken ct = default);
}
