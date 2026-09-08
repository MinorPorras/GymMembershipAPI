using GymMembershipAPI.API.DTOs.GroupClass;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymMembershipAPI.API.Services;

public class GroupClassService : IGroupClassService
{
    private readonly ILogger<GroupClassService> _logger;
    private readonly GymDbContext _context;

    public GroupClassService(ILogger<GroupClassService> logger, GymDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Result<GroupClass>> GetByPublicIdAsync(Guid publicId)
    {
        var existingClass = await _context.GroupClasses.FirstOrDefaultAsync(c => c.PublicId == publicId);
        return existingClass == null 
            ? Result<GroupClass>.Failure(GroupClassErrors.NotFound) 
            : Result<GroupClass>.Success(existingClass);
    }

    public Task<Result<GroupClass>> GetByIdAsync(Guid publicId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<GroupClass>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<GroupClass>>> GetByDateAsync(DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<Result<GroupClass>> CreateAsync(GroupClassRequestDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Result<GroupClass>> DeleteAsync(Guid publicId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<GroupClass>> UpdateAsync(Guid publicId, GroupClassRequestDto dto)
    {
        throw new NotImplementedException();
    }
}