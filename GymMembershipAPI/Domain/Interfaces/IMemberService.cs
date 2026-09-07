using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IMemberService : IServiceBase<Member>
{
    public Task<Result<Member>> CreateAsync(MemberRequestDto requestDto);
    public Task<Result<Member>> UpdateAsync(Guid publicId, MemberRequestDto dto);
}