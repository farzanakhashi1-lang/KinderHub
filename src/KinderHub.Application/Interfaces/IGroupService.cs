using KinderHub.Application.Common;
using KinderHub.Application.DTOs;

namespace KinderHub.Application.Interfaces;

public interface IGroupService
{
    Task<IReadOnlyList<GroupDto>> GetAllAsync(CancellationToken ct = default);
    Task<GroupDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(GroupEditModel model, CancellationToken ct = default);
    Task<Result> UpdateAsync(GroupEditModel model, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
