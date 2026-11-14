using KinderHub.Application.Common;
using KinderHub.Application.DTOs;

namespace KinderHub.Application.Interfaces;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceRowDto>> GetDailySheetAsync(int groupId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceDto>> GetChildHistoryAsync(int childId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<Result> SaveAsync(AttendanceEditModel model, CancellationToken ct = default);
}
