using GymMembershipAPI.API.DTOs.RegisterAccess;
using GymMembershipAPI.API.DTOs.Shared;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IRegisterAccessService : IServiceBase<RegisterAccess>
{
    public Task<Result<RegisterAccess>> RegisterAsync(RegisterAccessRequestDto dto, CancellationToken ct);
    public Task<Result<PaginatedResult<RegisterAccess>>> GetAllAsync(int page, int pageSize, CancellationToken ct);

    public Task<Result<PaginatedResult<RegisterAccess>>> GetByDateAsync(DateTime date, int page, int pageSize,
        CancellationToken ct);

    public Task<Result<PaginatedResult<RegisterAccess>>> GetByMemberPublicIdAsync(Guid memberPublicId, int page,
        int pageSize, CancellationToken ct);
}