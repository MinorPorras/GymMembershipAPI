using GymMembershipAPI.API.DTOs.RegisterAccess;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IRegisterAccessService : IServiceBase<RegisterAccess>
{
    public Task<Result<RegisterAccess>> RegisterAsync(RegisterAccessRequestDto dto, CancellationToken ct);
    public Task<Result<List<RegisterAccess>>> GetAllAsync(CancellationToken ct);
    public Task<Result<List<RegisterAccess>>> GetByDateAsync(DateTime date, CancellationToken ct);
    public Task<Result<List<RegisterAccess>>> GetByMemberPublicIdAsync(Guid memberPublicId, CancellationToken ct);
}