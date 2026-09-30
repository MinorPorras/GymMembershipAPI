using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using GymMembershipAPI.API.Mappers;
using Microsoft.Extensions.Caching.Memory;
using static System.String;

namespace GymMembershipAPI.API.Services;

public class MembershipTypeService : IMembershipTypeService
{
    // Context
    private readonly GymDbContext _context;
    private readonly ILogger<MembershipTypeService> _logger;
    private readonly IMemoryCache _cache;

    public MembershipTypeService(GymDbContext context, ILogger<MembershipTypeService> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    private async Task<bool> ExistsByNameAsync(string name, CancellationToken ct)
        => await _context.MembershipTypes.AnyAsync(x => x.Name == name, ct);

    // IMembershipType implementation
    public async Task<Result<MembershipType>> GetByPublicIdAsync(Guid publicId, CancellationToken ct)
    {
        var entity = await _context.MembershipTypes.FirstOrDefaultAsync(x => x.PublicId == publicId, ct);
        return entity == null
            ? Result<MembershipType>.Failure(MembershipTypeErrors.NotFound)
            : Result<MembershipType>.Success(entity);
    }

    public async Task<Result<List<MembershipType>>> GetAllAsync(CancellationToken ct)
    {
        const string cacheKey = "MembershipTypes_All";
        try
        {
            var cachedData = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                entry.Priority = CacheItemPriority.Normal;
                _logger.LogInformation(" CACHE MISS: Obtiendo tipos de membresía desde la base de datos.");
                var entities = await _context.MembershipTypes.OrderBy(m => m.Name).ToListAsync(ct);
                return entities;
            });
            return Result<List<MembershipType>>.Success(cachedData!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inesperado al obtener los tipos de membresía");
            return Result<List<MembershipType>>.Failure(Error.Unknown);
        }
    }

    public async Task<Result<MembershipType>> CreateAsync(MembershipTypeRequestDto requestDto, CancellationToken ct)
    {
        if (await ExistsByNameAsync(requestDto.Name, ct))
            return Result<MembershipType>.Failure(MembershipTypeErrors.AlreadyExists);

        var entity = MembershipTypeMapper.ToEntity(requestDto);
        try
        {
            _context.MembershipTypes.Add(entity);
            await _context.SaveChangesAsync(ct);
            _cache.Remove("MembershipTypes_All");
            return Result<MembershipType>.Success(entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error creating membership type with Name {MembershipTypeName}",
                requestDto.Name);
            return Result<MembershipType>.Failure(Error.Unknown);
        }
    }

    public async Task<Result<MembershipType>> UpdateAsync(Guid publicId, MembershipTypeRequestDto dto,
        CancellationToken ct)
    {
        var nameAlreadyExists =
            await _context.MembershipTypes.AnyAsync(x => x.Name == dto.Name && x.PublicId != publicId, ct);
        if (nameAlreadyExists) return Result<MembershipType>.Failure(MembershipTypeErrors.AlreadyExists);

        var existingType = await _context.MembershipTypes.FirstOrDefaultAsync(x => x.PublicId == publicId, ct);
        if (existingType == null) return Result<MembershipType>.Failure(MembershipTypeErrors.NotFound);

        try
        {
            existingType.Name = dto.Name;
            existingType.Price = dto.Price;
            existingType.DurationMonths = dto.DurationMonths;
            await _context.SaveChangesAsync(ct);
            _cache.Remove("MembershipTypes_All");
            return Result<MembershipType>
                .Success(existingType);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error updating membership type {MembershipTypePublicId}",
                publicId);
            return Result<MembershipType>.Failure(Error.Unknown);
        }
    }

    public async Task<Result> DeleteAsync(Guid publicId, CancellationToken ct)
    {
        var existingType = await _context.MembershipTypes.FirstOrDefaultAsync(x => x.PublicId == publicId, ct);
        if (existingType == null) return Result.Failure(MembershipTypeErrors.NotFound);
        try
        {
            _context.MembershipTypes.Remove(existingType);
            await _context.SaveChangesAsync(ct);
            _cache.Remove("MembershipTypes_All");
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error deleting membership type {MembershipTypePublicId}",
                publicId);
            return Result.Failure(Error.Unknown);
        }
    }
}