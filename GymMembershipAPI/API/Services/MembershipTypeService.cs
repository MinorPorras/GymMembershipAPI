using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using GymMembershipAPI.API.Mappers;
using static System.String;

namespace GymMembershipAPI.API.Services;

public class MembershipTypeService(GymDbContext context) : IMembershipTypeService
{
    // Context
    private readonly GymDbContext _context = context;

    // Validation
    private static Result ValidateDto(MembershipTypeRequestDto requestDto)
    {
        if (IsNullOrWhiteSpace(requestDto.Name))
            return Result.Failure(MembershipTypeErrors.InvalidName);

        if (requestDto.Price <= 0)
            return Result.Failure(MembershipTypeErrors.InvalidPrice);

        if (requestDto.DurationMonths <= 0)
            return Result.Failure(MembershipTypeErrors.InvalidMonthDuration);

        return Result.Success();
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
        var hasValidData = ValidateDto(requestDto);
        if (hasValidData.IsFailure)
            return Result<MembershipType>.Failure(hasValidData.Error);
        
        if (await ExistsByNameAsync(requestDto.Name)) 
            return Result<MembershipType>.Failure(MembershipTypeErrors.AlreadyExists);

        var entity = MembershipTypeMapper.ToEntity(requestDto);
        try
        {
            await _context.MembershipTypes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return Result<MembershipType>.Success(entity);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result<MembershipType>.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result<MembershipType>> UpdateAsync(Guid publicId, MembershipTypeRequestDto dto)
    {
        var hasValidData = ValidateDto(dto);
        if (hasValidData.IsFailure)
            return Result<MembershipType>.Failure(hasValidData.Error);

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
            Console.WriteLine(e);
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
            Console.WriteLine(e);
            return Result.Failure(Error.Unknown(e.Message));
        }
    }
}