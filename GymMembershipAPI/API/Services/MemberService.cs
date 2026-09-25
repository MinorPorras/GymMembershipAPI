using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.API.DTOs.Shared;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymMembershipAPI.API.Services;

public class MemberService : IMemberService
{
    private readonly GymDbContext _context;
    private readonly ILogger<MemberService> _logger;

    public MemberService(GymDbContext context, ILogger<MemberService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Member>> GetByPublicIdAsync(Guid publicId, CancellationToken ct)
    {
        var entity = await _context.Members.FirstOrDefaultAsync(x => x.PublicId == publicId, ct);
        return entity == null
            ? Result<Member>.Failure(MemberErrors.NotFound)
            : Result<Member>.Success(entity);
    }

    public async Task<Result<PaginatedResult<Member>>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var totalRecord = await _context.Members.CountAsync(ct);

        var skipAmount = (page - 1) * pageSize;

        var data = await _context.Members
            .OrderBy(m => m.Id)
            .Skip(skipAmount)
            .Take(pageSize)
            .ToListAsync(ct);

        return Result<PaginatedResult<Member>>.Success(
            new PaginatedResult<Member>(data, page, pageSize, totalRecord)
        );
    }

    public async Task<Result> DeleteAsync(Guid publicId, CancellationToken ct)
    {
        var entity = await _context.Members.FirstOrDefaultAsync(x => x.PublicId == publicId, ct);
        if (entity == null) return Result.Failure(MemberErrors.NotFound);
        try
        {
            _context.Members.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar miembro {PublicId}", publicId);
            return Result.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result<Member>> CreateAsync(MemberCreateDto dto, CancellationToken ct)
    {
        var membershipType = await _context.MembershipTypes
            .FirstOrDefaultAsync(t => t.PublicId == dto.MembershipTypePublicId, ct);

        if (membershipType == null)
            return Result<Member>.Failure(MembershipTypeErrors.NotFound);

        // Validar email duplicado
        var emailExists = await _context.Members
            .AnyAsync(x => x.Email == dto.Email, ct);

        if (emailExists)
            return Result<Member>.Failure(MemberErrors.EmailAlreadyExists);

        var entity = MemberMapper.ToEntity(dto);

        var initialMembership = new Membership
        {
            MembershipTypeId = membershipType.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(membershipType.DurationMonths),
            IsActive = true
        };

        entity.Memberships.Add(initialMembership);
        try
        {
            _context.Members.Add(entity);
            await _context.SaveChangesAsync(ct);
            return Result<Member>.Success(entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating the Member with Name {Name} and Email {Email}", dto.Name, dto.Email);
            return Result<Member>.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result<Member>> UpdateAsync(Guid publicId, MemberUpdateDto dto, CancellationToken ct)
    {
        var emailExists = await _context.Members
            .AnyAsync(x => x.Email == dto.Email && x.PublicId != publicId, ct);
        if (emailExists) return Result<Member>.Failure(MemberErrors.EmailAlreadyExists);

        var entity = await _context.Members.FirstOrDefaultAsync(x => x.PublicId == publicId, ct);
        if (entity == null) return Result<Member>.Failure(MemberErrors.NotFound);

        try
        {
            entity.Name = dto.Name;
            entity.Email = dto.Email;
            entity.Phone = dto.Phone ?? "";

            await _context.SaveChangesAsync(ct);
            return Result<Member>.Success(entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating the Member {PublicId}", publicId);
            return Result<Member>.Failure(Error.Unknown(e.Message));
        }
    }
}