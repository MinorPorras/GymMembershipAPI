using System.ComponentModel.DataAnnotations;
using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.API.DTOs.Membership;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using static System.String;

namespace GymMembershipAPI.API.Services;

public class MemberService : IMemberService
{
    private readonly GymDbContext _context;
    private readonly IMembershipTypeService _memberTypeService;
    private readonly ILogger<MemberService> _logger;
    private readonly IMembershipService _membershipService;

    public MemberService(GymDbContext context, IMembershipTypeService memberTypeService, ILogger<MemberService> logger, IMembershipService membershipService)
    {
        _context = context;
        _memberTypeService = memberTypeService;
        _logger = logger;
        _membershipService = membershipService;
    }

    private Result IsValidDto(MemberCreateDto dto)
    {
        if (IsNullOrWhiteSpace(dto.Name)) return Result.Failure(MemberErrors.EmptyOrNullName);
        if (IsNullOrWhiteSpace(dto.Email)) return Result.Failure(MemberErrors.EmptyOrNullEmail);
        var exists = _context.Members.Any(x => x.Email == dto.Email);
        if (exists) return Result.Failure(MemberErrors.EmailAlreadyExists);
        var attribute = new EmailAddressAttribute();
        return attribute.IsValid(dto.Email)
            ? Result.Success()
            : Result.Failure(MemberErrors.InvalidEmailFormat);
    }

    private Result IsValidDto(MemberUpdateDto dto)
    {
        if (IsNullOrWhiteSpace(dto.Name)) return Result.Failure(MemberErrors.EmptyOrNullName);
        if (IsNullOrWhiteSpace(dto.Email)) return Result.Failure(MemberErrors.EmptyOrNullEmail);
        var exists = _context.Members.Any(x => x.Email == dto.Email);
        if (exists) return Result.Failure(MemberErrors.EmailAlreadyExists);
        var attribute = new EmailAddressAttribute();
        return attribute.IsValid(dto.Email)
            ? Result.Success()
            : Result.Failure(MemberErrors.InvalidEmailFormat);
    }

    public async Task<Result<Member>> GetByPublicIdAsync(Guid publicId)
    {
        var entity = await _context.Members.FirstOrDefaultAsync(x => x.PublicId == publicId);
        return entity == null
            ? Result<Member>.Failure(MemberErrors.NotFound)
            : Result<Member>.Success(entity);
    }

    public async Task<Result<List<Member>>> GetAllAsync()
    {
        var list = await _context.Members.ToListAsync();
        return Result<List<Member>>.Success(list);
    }

    public async Task<Result> DeleteAsync(Guid publicId)
    {
        var entity = await _context.Members.FirstOrDefaultAsync(x => x.PublicId == publicId);
        if (entity == null) return Result.Failure(MemberErrors.NotFound);
        try
        {
            _context.Members.Remove(entity);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar miembro {PublicId}", publicId);
            return Result.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result<Member>> CreateAsync(MemberCreateDto dto)
    {
        var isValid = IsValidDto(dto);
        if (isValid.IsFailure) return Result<Member>.Failure(isValid.Error);
        var membershipType = await _context.MembershipTypes
            .FirstOrDefaultAsync(t => t.PublicId == dto.MembershipTypeId);
        if (membershipType == null) 
            return Result<Member>.Failure(MembershipTypeErrors.NotFound);

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
            await _context.SaveChangesAsync();
            return Result<Member>.Success(entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating the Member with Name {Name} and Email {Email}", dto.Name, dto.Email);
            return Result<Member>.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result<Member>> UpdateAsync(Guid publicId, MemberUpdateDto dto)
    {
        var isValid = IsValidDto(dto);
        if (isValid.IsFailure) return Result<Member>.Failure(isValid.Error);
        var entity = await _context.Members.FirstOrDefaultAsync(x => x.PublicId == publicId);
        if (entity == null) return Result<Member>.Failure(MemberErrors.NotFound);
        try
        {
            entity.Name = dto.Name;
            entity.Email = dto.Email;
            entity.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return Result<Member>.Success(entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating the Member {PublicId}", publicId);
            return Result<Member>.Failure(Error.Unknown(e.Message));
        }
    }
}