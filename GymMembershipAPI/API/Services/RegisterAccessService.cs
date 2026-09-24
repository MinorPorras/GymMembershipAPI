using GymMembershipAPI.API.DTOs.RegisterAccess;
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

    public async Task<Result<List<RegisterAccess>>> GetAllAsync(CancellationToken ct) =>
        Result<List<RegisterAccess>>.Success(
            await _context.RegisterAccesses.ToListAsync(ct)
        );

    public async Task<Result<List<RegisterAccess>>> GetByDateAsync(DateTime date, CancellationToken ct) =>
        Result<List<RegisterAccess>>.Success(
            await _context.RegisterAccesses.Where(r => r.AccessDate.Date == date.Date).ToListAsync(ct)
        );

    public async Task<Result<List<RegisterAccess>>> GetByMemberPublicIdAsync(Guid memberPublicId, CancellationToken ct)
    {
        var list = await _context.RegisterAccesses
            .Where(r => r.Member.PublicId == memberPublicId)
            .ToListAsync(ct);

        return Result<List<RegisterAccess>>.Success(list);
    }
}