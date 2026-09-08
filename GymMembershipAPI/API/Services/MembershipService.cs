using GymMembershipAPI.API.DTOs.Membership;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymMembershipAPI.API.Services;

public class MembershipService(GymDbContext context, ILogger<MembershipService> logger) : IMembershipService
{
    private readonly GymDbContext _context = context;
    private readonly ILogger<MembershipService> _logger = logger;

    public async Task<Result<Membership>> GetByPublicIdAsync(Guid publicId)
    {
        var entity = await _context.Memberships
            .Include(e => e.MembershipType)
            .Include(e => e.Member)
            .FirstOrDefaultAsync(e => e.PublicId == publicId);
        return entity == null
            ? Result<Membership>.Failure(MembershipErrors.NotFound)
            : Result<Membership>.Success(entity);
    }

    public async Task<Result<List<Membership>>> GetActiveByMemberAsync(Guid memberPublicId)
    {
        var existingMember =
            await _context.Members.FirstOrDefaultAsync(e => e.PublicId == memberPublicId);
        if (existingMember == null) return Result<List<Membership>>.Failure(MembershipErrors.MemberNotFound);

        var list = await _context.Memberships.Where(e => e.IsActive && e.MemberId == existingMember.Id)
            .ToListAsync();

        return Result<List<Membership>>.Success(list);
    }

    public async Task<Result<List<Membership>>> GetHistoryByMemberAsync(Guid memberPublicId)
    {
        var existingMember =
            await _context.Members.FirstOrDefaultAsync(e => e.PublicId == memberPublicId);
        if (existingMember == null) return Result<List<Membership>>.Failure(MembershipErrors.MemberNotFound);

        var list = await _context.Memberships.Where(m => m.MemberId == existingMember.Id)
            .OrderByDescending(m => m.StartDate).ToListAsync();

        return Result<List<Membership>>.Success(list);
    }

    public async Task<Result<Membership>> CreateAsync(MembershipRequestDto dto)
    {
        var existingMember =
            await _context.Members.FirstOrDefaultAsync(e => e.PublicId == dto.MemberPublicId);
        if (existingMember == null) return Result<Membership>.Failure(MembershipErrors.MemberNotFound);

        var existingMembershipType =
            await _context.MembershipTypes.FirstOrDefaultAsync(e => e.PublicId == dto.MembershipTypePublicId);
        if (existingMembershipType == null) return Result<Membership>.Failure(MembershipErrors.MembershipTypeNotFound);

        var entity = MembershipMapper.ToEntity(dto);
        entity.MemberId = existingMember.Id;
        entity.MembershipTypeId = existingMembershipType.Id;
        entity.StartDate = DateTime.UtcNow;
        entity.EndDate = entity.StartDate.AddMonths(existingMembershipType.DurationMonths);
        entity.IsActive = true;

        var activeMemberships = await _context.Memberships
            .Where(e => e.MemberId == existingMember.Id && e.IsActive)
            .ToListAsync();

        foreach (var prevMembership in activeMemberships)
        {
            prevMembership.IsActive = false;
            prevMembership.EndDate = DateTime.UtcNow;
        }

        try
        {
            _context.Memberships.Add(entity);
            await _context.SaveChangesAsync();
            return Result<Membership>.Success(entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create membership for member {MemberPublicId}", dto.MemberPublicId);
            return Result<Membership>.Failure(Error.Unknown(e.Message));
        }
    }
    
    public async Task<Result> CancelAsync(Guid membershipPublicId)
    {
        var membership = await _context.Memberships.FirstOrDefaultAsync(e => e.PublicId == membershipPublicId);
        if (membership == null) return Result.Failure(MembershipErrors.NotFound);

        if (!membership.IsActive)
            return Result.Failure(MembershipErrors.AlreadyCancelled);

        membership.IsActive = false;
        membership.EndDate = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to cancel membership {MembershipPublicId}", membershipPublicId);
            return Result.Failure(Error.Unknown(e.Message));
        }
    }
}