using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IMembershipTypeService : IServiceBase<MembershipType>
{
    public Task<Result<MembershipType>> CreateAsync(MembershipTypeRequestDto requestDto, CancellationToken ct);
    public Task<Result<MembershipType>> UpdateAsync(Guid publicId, MembershipTypeRequestDto dto, CancellationToken ct);
    public Task<Result> DeleteAsync(Guid publicId, CancellationToken ct);
    public Task<Result<List<MembershipType>>> GetAllAsync(CancellationToken ct);
}