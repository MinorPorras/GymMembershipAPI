using GymMembershipAPI.API.DTOs.Membership;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IMembershipService : IServiceBase<Membership>
{
    Task<Result<Membership>> CreateAsync(MembershipRequestDto dto, CancellationToken ct);
    Task<Result<List<Membership>>> GetActiveByMemberPublicId(Guid memberPublicId, CancellationToken ct);
    Task<Result<List<Membership>>> GetHistoryByMemberPublicIdAsync(Guid memberPublicId, CancellationToken ct);
    Task<Result> CancelAsync(Guid membershipPublicId, CancellationToken ct);
}