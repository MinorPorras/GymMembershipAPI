using GymMembershipAPI.API.DTOs.GroupClass;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IGroupClassService : IServiceBase<GroupClass>
{
    public Task<Result<List<GroupClass>>> GetAllAsync(CancellationToken ct);
    public Task<Result<List<GroupClass>>> GetByDateAsync(DateTime date, CancellationToken ct);
    public Task<Result<GroupClass>> CreateAsync(GroupClassRequestDto dto, CancellationToken ct);
    public Task<Result> DeleteAsync(Guid publicId, CancellationToken ct);
    public Task<Result<GroupClass>> UpdateAsync(Guid publicId, GroupClassRequestDto dto, CancellationToken ct);
}