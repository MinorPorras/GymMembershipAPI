using GymMembershipAPI.API.DTOs.RegisterAccess;
using GymMembershipAPI.API.DTOs.Shared;
using GymMembershipAPI.API.Extensions;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymMembershipAPI.API.Services;

public class RegisterAccessService : IRegisterAccessService
{
    private readonly ILogger<RegisterAccessService> _logger;
    private readonly GymDbContext _context;

    public RegisterAccessService(ILogger<RegisterAccessService> logger, GymDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Result<RegisterAccess>> GetByPublicIdAsync(Guid publicId, CancellationToken ct)
    {
        var entity = await _context.RegisterAccesses.FirstOrDefaultAsync(r => r.PublicId == publicId, ct);
        return entity == null
            ? Result<RegisterAccess>.Failure(RegisterAccessErrors.NotFound)
            : Result<RegisterAccess>.Success(entity);
    }

    public async Task<Result<RegisterAccess>> RegisterAsync(RegisterAccessRequestDto dto, CancellationToken ct)
    {
        var member = await _context.Members.FirstOrDefaultAsync(r => r.PublicId == dto.MemberPublicId, ct);
        if (member == null) return Result<RegisterAccess>.Failure(RegisterAccessErrors.NotFound);

        var hasMembership = await _context.Memberships
            .AnyAsync(r => r.MemberId == member.Id && r.IsActive && r.EndDate > DateTime.UtcNow, ct);

        var entity = new RegisterAccess()
        {
            MemberId = member.Id,
            AccessDate = DateTime.UtcNow,
            AllowAccess = hasMembership
        };
        try
        {
            _context.RegisterAccesses.Add(entity);
            await _context.SaveChangesAsync(ct);
            return Result<RegisterAccess>.Success(entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error registering access for member {MemberId}", member.Id);
            return Result<RegisterAccess>.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result<PaginatedResult<RegisterAccess>>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        try
        {
            var pagedResult = await _context.RegisterAccesses
                .OrderBy(m => m.Id)
                .ToPaginatedResultAsync(page, pageSize, ct);
            return Result<PaginatedResult<RegisterAccess>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los registros de acceso");
            return Result<PaginatedResult<RegisterAccess>>.Failure(Error.Unknown(ex.Message));
        }
    }

    public async Task<Result<PaginatedResult<RegisterAccess>>> GetByDateAsync(DateTime date, int page, int pageSize,
        CancellationToken ct)
    {
        try
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var pagedResult = await _context.RegisterAccesses
                .Where(m => m.AccessDate >= startOfDay && m.AccessDate < endOfDay)
                .OrderByDescending(m => m.AccessDate)
                .ToPaginatedResultAsync(page, pageSize, ct);
            return Result<PaginatedResult<RegisterAccess>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los registros de acceso por fecha");
            return Result<PaginatedResult<RegisterAccess>>.Failure(Error.Unknown(ex.Message));
        }
    }

    public async Task<Result<PaginatedResult<RegisterAccess>>> GetByMemberPublicIdAsync(
        Guid memberPublicId,
        int page,
        int pageSize,
        CancellationToken ct
    )
    {
        try
        {
            var pagedResult = await _context.RegisterAccesses
                .Where(m => m.Member.PublicId == memberPublicId)
                .OrderBy(m => m.Id)
                .ToPaginatedResultAsync(page, pageSize, ct);
            return Result<PaginatedResult<RegisterAccess>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los registros por la ID publica del usuario");
            return Result<PaginatedResult<RegisterAccess>>.Failure(Error.Unknown(ex.Message));
        }
    }
}