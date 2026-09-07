using System.ComponentModel.DataAnnotations;
using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using static System.String;

namespace GymMembershipAPI.API.Services;

public class MemberService(GymDbContext context, IMembershipTypeService memberTypeService)
    : IMemberService
{
    private readonly GymDbContext _context = context;
    private readonly IMembershipTypeService _memberTypeService = memberTypeService;

    private static Result IsValid(MemberRequestDto dto)
    {
        if (IsNullOrWhiteSpace(dto.Name)) return Result.Failure(MemberErrors.EmptyOrNullName);
        if (IsNullOrWhiteSpace(dto.Email)) return Result.Failure(MemberErrors.EmptyOrNullEmail);
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
            // TODO: Reemplazar con _logger.LogError(e, "Error al eliminar miembro {PublicId}", publicId);
            return Result.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result<Member>> CreateAsync(MemberRequestDto dto)
    {
        var isValid = IsValid(dto);
        if (isValid.IsFailure) return Result<Member>.Failure(isValid.Error);

        var entity = new Member
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone ?? string.Empty,
        };
        try
        {
            _context.Members.Add(entity);
            await _context.SaveChangesAsync();
            return Result<Member>.Success(entity);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result<Member>.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result<Member>> UpdateAsync(Guid publicId, MemberRequestDto dto)
    {
        var isValid = IsValid(dto);
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
            return Result<Member>.Failure(Error.Unknown(e.Message));
        }
    }
}