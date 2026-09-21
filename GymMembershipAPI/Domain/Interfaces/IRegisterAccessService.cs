using GymMembershipAPI.API.DTOs.RegisterAccess;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IRegisterAccessService : IServiceBase<RegisterAccess>
{
    public Task<Result<RegisterAccess>> RegisterAsync(RegisterAccessRequestDto dto);
    public Task<Result<List<RegisterAccess>>> GetAllAsync();
    public Task<Result<List<RegisterAccess>>> GetByDateAsync(DateTime date);
    public Task<Result<List<RegisterAccess>>> GetByMemberPublicIdAsync(Guid memberPublicId);
}