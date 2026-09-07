using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IMembershipTypeService: IServiceBase<MembershipType>
{
    public Task<Result<MembershipType>> CreateAsync(MembershipTypeRequestDto requestDto);
    public Task<Result<MembershipType>> UpdateAsync(Guid publicId, MembershipTypeRequestDto dto);
}