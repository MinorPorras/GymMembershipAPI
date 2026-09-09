using GymMembershipAPI.API.DTOs.GroupClass;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IGroupClassService : IServiceBase<GroupClass>
{
    public Task<Result<List<GroupClass>>> GetAllAsync();
    public Task<Result<List<GroupClass>>> GetByDateAsync(DateTime date);
    public Task<Result<GroupClass>> CreateAsync(GroupClassRequestDto dto);
    public Task<Result> DeleteAsync(Guid publicId);
    public Task<Result<GroupClass>> UpdateAsync(Guid publicId, GroupClassRequestDto dto);
}