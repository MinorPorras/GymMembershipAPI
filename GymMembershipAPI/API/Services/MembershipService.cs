using GymMembershipAPI.API.DTOs.Membership;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymMembershipAPI.API.Services;

public class MembershipService : IMembershipService
{
    private readonly GymDbContext _context;
    private readonly ILogger<MembershipService> _logger;

    public MembershipService(ILogger<MembershipService> logger, GymDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Result<Membership>> GetByPublicIdAsync(Guid publicId, CancellationToken ct)
    {
        var entity = await _context.Memberships
            .Include(e => e.MembershipType)
            .Include(e => e.Member)
            .FirstOrDefaultAsync(e => e.PublicId == publicId, ct);
        return entity == null
            ? Result<Membership>.Failure(MembershipErrors.NotFound)
            : Result<Membership>.Success(entity);
    }

    public async Task<Result<List<Membership>>> GetActiveByMemberPublicId(Guid memberPublicId, CancellationToken ct)
    {
        var list = await _context.Memberships
            .Where(e => e.Member.PublicId == memberPublicId && e.IsActive)
            .ToListAsync(ct);

        return Result<List<Membership>>.Success(list);
    }

    public async Task<Result<List<Membership>>> GetHistoryByMemberPublicIdAsync(Guid memberPublicId, CancellationToken ct)
    {
        var list = await _context.Memberships
            .Where(m => m.Member.PublicId == memberPublicId)
            .OrderByDescending(m => m.StartDate)
            .ToListAsync(ct);

        return Result<List<Membership>>.Success(list);
    }

    public async Task<Result<Membership>> CreateAsync(MembershipRequestDto dto, CancellationToken ct)
    {
        var existingMember = await _context.Members
            .FirstOrDefaultAsync(e => e.PublicId == dto.MemberPublicId, ct);

        if (existingMember == null)
            return Result<Membership>.Failure(MembershipErrors.MemberNotFound);

        var existingMembershipType = await _context.MembershipTypes
            .FirstOrDefaultAsync(e => e.PublicId == dto.MembershipTypePublicId, ct);

        if (existingMembershipType == null)
            return Result<Membership>.Failure(MembershipErrors.MembershipTypeNotFound);

        var activeMemberships = await _context.Memberships
            .Where(e => e.MemberId == existingMember.Id && e.IsActive)
            .ToListAsync(ct);

        foreach (var prevMembership in activeMemberships)
        {
            prevMembership.IsActive = false;
            prevMembership.EndDate = DateTime.UtcNow;
        }

        var entity = new Membership
        {
            MemberId = existingMember.Id,
            MembershipTypeId = existingMembershipType.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(existingMembershipType.DurationMonths),
            IsActive = true
        };

        try
        {
            _context.Memberships.Add(entity);
            await _context.SaveChangesAsync(ct);
            return Result<Membership>.Success(entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create membership for member {MemberPublicId}", dto.MemberPublicId);
            return Result<Membership>.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result> CancelAsync(Guid membershipPublicId, CancellationToken ct)
    {
        var membership = await _context.Memberships.FirstOrDefaultAsync(e => e.PublicId == membershipPublicId, ct);
        if (membership == null) return Result.Failure(MembershipErrors.NotFound);

        if (!membership.IsActive)
            return Result.Failure(MembershipErrors.AlreadyCancelled);

        membership.IsActive = false;
        membership.EndDate = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to cancel membership {MembershipPublicId}", membershipPublicId);
            return Result.Failure(Error.Unknown(e.Message));
        }
    }
}