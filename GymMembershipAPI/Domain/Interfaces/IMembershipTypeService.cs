using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IMembershipTypeService: IServiceBase<MembershipType>
{
    public Task<Result<MembershipType>> CreateAsync(CreateMembershipTypeDto dto);
    public Task<Result<MembershipType>> UpdateAsync(Guid publicId, UpdateMembershipTypeDto dto);
    public Task<Result> DeleteAsync(Guid publicId);
}