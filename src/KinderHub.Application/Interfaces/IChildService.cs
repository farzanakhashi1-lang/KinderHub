using KinderHub.Application.Common;
using KinderHub.Application.DTOs;

namespace KinderHub.Application.Interfaces;

public interface IChildService
{
    Task<PagedResult<ChildDto>> GetPagedAsync(ChildQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<ChildDto>> GetByGroupAsync(int groupId, CancellationToken ct = default);
    Task<IReadOnlyList<ChildDto>> GetByParentEmailAsync(string parentEmail, CancellationToken ct = default);
    Task<ChildDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(ChildEditModel model, CancellationToken ct = default);
    Task<Result> UpdateAsync(ChildEditModel model, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
