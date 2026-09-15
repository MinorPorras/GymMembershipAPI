using GymMembershipAPI.API.DTOs.GroupClass;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using static System.String;

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

    private async Task<Result> IsValidDto(GroupClassRequestDto dto, Guid? excludePubliId = null)
    {
        if (IsNullOrWhiteSpace(dto.Name)) return Result.Failure(GroupClassErrors.EmptyOrNullName);

        var nameAlredyExists = await _context.GroupClasses
            .AnyAsync(c => c.Name == dto.Name && c.PublicId != excludePubliId);
        if (nameAlredyExists)
            return Result.Failure(GroupClassErrors.NameAlreadyExists);

        if (IsNullOrWhiteSpace(dto.Instructor))
            return Result.Failure(GroupClassErrors.EmptyOrNullInstructor);

        if (dto.DateHour.Date < DateTime.UtcNow.Date)
            return Result.Failure(GroupClassErrors.InvalidDate);

        if (dto.MaxMembers <= 0) return Result.Failure(GroupClassErrors.InvalidMaxMembers);

        return Result.Success();
    }

    public async Task<Result<GroupClass>> GetByPublicIdAsync(Guid publicId)
    {
        var existingClass = await _context.GroupClasses.FirstOrDefaultAsync(c => c.PublicId == publicId);
        return existingClass == null
            ? Result<GroupClass>.Failure(GroupClassErrors.NotFound)
            : Result<GroupClass>.Success(existingClass);
    }

    public async Task<Result<List<GroupClass>>> GetAllAsync()
    {
        var list = await _context.GroupClasses.ToListAsync();
        return Result<List<GroupClass>>.Success(list);
    }

    public async Task<Result<List<GroupClass>>> GetByDateAsync(DateTime date)
    {
        var list = await _context.GroupClasses.Where(c => c.DateHour.Date == date.Date).ToListAsync();
        return Result<List<GroupClass>>.Success(list);
    }

    public async Task<Result<GroupClass>> CreateAsync(GroupClassRequestDto dto)
    {
        var isValid = await IsValidDto(dto);
        if (isValid.IsFailure) return Result<GroupClass>.Failure(isValid.Error);
        var entity = GroupClassMapper.ToEntity(dto);
        try
        {
            _context.GroupClasses.Add(entity);
            await _context.SaveChangesAsync();
            return Result<GroupClass>.Success(entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create GroupClass with name {Name}", entity.Name);
            return Result<GroupClass>.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result> DeleteAsync(Guid publicId)
    {
        var existingClass = await _context.GroupClasses.FirstOrDefaultAsync(c => c.PublicId == publicId);
        if (existingClass == null) return Result.Failure(GroupClassErrors.NotFound);
        try
        {
            _context.GroupClasses.Remove(existingClass);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete GroupClass {PublicId}", existingClass.PublicId);
            return Result.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result<GroupClass>> UpdateAsync(Guid publicId, GroupClassRequestDto dto)
    {
        var isValid = await IsValidDto(dto, publicId);
        if (isValid.IsFailure) return Result<GroupClass>.Failure(isValid.Error);

        var existingClass = await _context.GroupClasses.FirstOrDefaultAsync(c => c.PublicId == publicId);
        if (existingClass == null) return Result<GroupClass>.Failure(GroupClassErrors.NotFound);

        try
        {
            existingClass.Name = dto.Name;
            existingClass.Instructor = dto.Instructor;
            existingClass.DateHour = dto.DateHour;
            existingClass.MaxMembers = dto.MaxMembers;

            await _context.SaveChangesAsync();
            return Result<GroupClass>.Success(existingClass);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update GroupClass {PublicId}", existingClass.PublicId);
            return Result<GroupClass>.Failure(Error.Unknown(e.Message));
        }
    }
}