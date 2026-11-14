using KinderHub.Application.Common;
using KinderHub.Application.DTOs;

namespace KinderHub.Application.Interfaces;

public interface IAnnouncementService
{
    // groupIds = null → all announcements; otherwise general ones + those for these groups
    Task<IReadOnlyList<AnnouncementDto>> GetLatestAsync(IReadOnlyCollection<int>? groupIds = null, int take = 20, CancellationToken ct = default);
    Task<Result<AnnouncementDto>> CreateAsync(AnnouncementEditModel model, string authorName, CancellationToken ct = default);
    Task<Result> UpdateAsync(AnnouncementEditModel model, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
