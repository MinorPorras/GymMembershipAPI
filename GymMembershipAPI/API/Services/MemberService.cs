using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using GymMembershipAPI.Infraestructure.Data;

namespace GymMembershipAPI.API.Services;

public class MemberService(GymDbContext context, IMembershipTypeService memberTypeService)
    : IMemberService
{
    private readonly GymDbContext _context = context;
    private readonly IMembershipTypeService _memberTypeService = memberTypeService;

    public Task<Result<Member>> GetByPublicIdAsync(Guid publicId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<Member>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteAsync(Guid publicId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Member>> CreateAsync(MemberRequestDto requestDto)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Member>> UpdateAsync(Guid publicId, MemberRequestDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Result> InvertState(Guid publicId)
    {
        throw new NotImplementedException();
    }
}