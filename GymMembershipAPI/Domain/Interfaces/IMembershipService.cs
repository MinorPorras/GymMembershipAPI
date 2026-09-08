using GymMembershipAPI.API.DTOs.Membership;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IMembershipService : IServiceBase<Membership>
{
    Task<Result<Membership>> CreateAsync(MembershipRequestDto dto);
    Task<Result<List<Membership>>> GetActiveByMemberAsync(Guid memberPublicId);
    Task<Result<List<Membership>>> GetHistoryByMemberAsync(Guid memberPublicId);
    Task<Result> CancelAsync(Guid membershipPublicId);
}