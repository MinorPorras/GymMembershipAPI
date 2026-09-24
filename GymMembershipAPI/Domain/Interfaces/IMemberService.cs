using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IMemberService : IServiceBase<Member>
{
    public Task<Result<Member>> CreateAsync(MemberCreateDto dto, CancellationToken ct);
    public Task<Result<Member>> UpdateAsync(Guid publicId, MemberUpdateDto dto, CancellationToken ct);
    public Task<Result> DeleteAsync(Guid publicId, CancellationToken ct);
    public Task<Result<List<Member>>> GetAllAsync(CancellationToken ct);

}