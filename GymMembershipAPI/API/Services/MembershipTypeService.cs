using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using GymMembershipAPI.API.Mappers;
using static System.String;

namespace GymMembershipAPI.API.Services;

public class MembershipTypeService : IMembershipTypeService
{
    // Context
    private readonly GymDbContext _context;
    private readonly ILogger<MembershipTypeService> _logger;

    public MembershipTypeService(GymDbContext context, ILogger<MembershipTypeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private async Task<bool> ExistsAsync(Guid publicId)
        => await _context.MembershipTypes.AnyAsync(x => x.PublicId == publicId);

    private async Task<bool> ExistsByNameAsync(string name)
        => await _context.MembershipTypes.AnyAsync(x => x.Name == name);

    // IMembershipType implementation
    public async Task<Result<MembershipType>> GetByPublicIdAsync(Guid publicId)
    {
        var entity = await _context.MembershipTypes.FirstOrDefaultAsync(x => x.PublicId == publicId);
        return entity == null
            ? Result<MembershipType>.Failure(MembershipTypeErrors.NotFound)
            : Result<MembershipType>.Success(entity);
    }

    public async Task<Result<List<MembershipType>>> GetAllAsync()
    {
        var list = await _context.MembershipTypes.ToListAsync();
        return Result<List<MembershipType>>.Success(list);
    }

    public async Task<Result<MembershipType>> CreateAsync(MembershipTypeRequestDto requestDto)
    {
        if (await ExistsByNameAsync(requestDto.Name))
            return Result<MembershipType>.Failure(MembershipTypeErrors.AlreadyExists);

        var entity = MembershipTypeMapper.ToEntity(requestDto);
        try
        {
            _context.MembershipTypes.Add(entity);
            await _context.SaveChangesAsync();
            return Result<MembershipType>.Success(entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error creating membership type with Name {MembershipTypeName}",
                requestDto.Name);
            return Result<MembershipType>.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result<MembershipType>> UpdateAsync(Guid publicId, MembershipTypeRequestDto dto)
    {
        var nameAlreadyExists =
            await _context.MembershipTypes.AnyAsync(x => x.Name == dto.Name && x.PublicId != publicId);
        if (nameAlreadyExists) return Result<MembershipType>.Failure(MembershipTypeErrors.AlreadyExists);

        var existingType = await _context.MembershipTypes.FirstOrDefaultAsync(x => x.PublicId == publicId);
        if (existingType == null) return Result<MembershipType>.Failure(MembershipTypeErrors.NotFound);

        try
        {
            existingType.Name = dto.Name;
            existingType.Price = dto.Price;
            existingType.DurationMonths = dto.DurationMonths;
            await _context.SaveChangesAsync();
            return Result<MembershipType>
                .Success(existingType);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error updating membership type {MembershipTypePublicId}",
                publicId);
            return Result<MembershipType>.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result> DeleteAsync(Guid publicId)
    {
        var existingType = _context.MembershipTypes.FirstOrDefault(x => x.PublicId == publicId);
        if (existingType == null) return Result.Failure(MembershipTypeErrors.NotFound);
        try
        {
            _context.MembershipTypes.Remove(existingType);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error deleting membership type {MembershipTypePublicId}",
                publicId);
            return Result.Failure(Error.Unknown(e.Message));
        }
    }
}