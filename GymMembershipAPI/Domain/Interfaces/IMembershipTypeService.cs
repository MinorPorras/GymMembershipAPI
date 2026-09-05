using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IMembershipTypeService
{
    public Task<Result<MembershipType>> CreateAsync(CreateMemberShipTypeDto dto);
    public Task<Result<MembershipType>> UpdateAsync(UpdateMembershipTypeDto dto);
    public Task<Result> DeleteAsync(Guid publicId);
    public Task<Result<MembershipType>> GetByIdAsync(Guid publicId);
    public Task<Result<List<MembershipType>>> GetAllAsync();
    public Task<bool> Exists(Guid publicId);
}