using KinderHub.Application.Common;
using KinderHub.Application.DTOs;

namespace KinderHub.Application.Interfaces;

public interface IDailyReportService
{
    Task<IReadOnlyList<DailyReportDto>> GetForChildAsync(int childId, int take = 10, CancellationToken ct = default);
    Task<IReadOnlyList<DailyReportDto>> GetForGroupAndDateAsync(int groupId, DateOnly date, CancellationToken ct = default);
    Task<Result> SaveAsync(DailyReportEditModel model, string authorName, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
