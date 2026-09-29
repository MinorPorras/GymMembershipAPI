using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.API.DTOs.Shared;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using Polly.Registry;

namespace GymMembershipAPI.API.Services;

public class MemberService : IMemberService
{
    private readonly GymDbContext _context;
    private readonly ILogger<MemberService> _logger;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;

    public MemberService(GymDbContext context, ILogger<MemberService> logger,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _context = context;
        _logger = logger;
        _pipelineProvider = pipelineProvider;
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
            return Result.Failure(Error.Unknown);
        }
    }

    public async Task<Result<Member>> CreateAsync(MemberCreateDto dto, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("db-pipeline");

        try
        {
            return await pipeline.ExecuteAsync(
                (Func<MemberCreateDto, CancellationToken, ValueTask<Result<Member>>>)Callback, dto, ct);

            async ValueTask<Result<Member>> Callback(MemberCreateDto state, CancellationToken innerCt)
            {
                var membershipType = await _context.MembershipTypes
                    .FirstOrDefaultAsync(t => t.PublicId == state.MembershipTypePublicId, innerCt);

                if (membershipType == null)
                    return Result<Member>.Failure(MembershipTypeErrors.NotFound);

                // Validar email duplicado
                var emailExists = await _context.Members
                    .AnyAsync(x => x.Email == state.Email, innerCt);

                if (emailExists)
                    return Result<Member>.Failure(MemberErrors.EmailAlreadyExists);

                var entity = MemberMapper.ToEntity(state);

                var initialMembership = new Membership
                {
                    MembershipTypeId = membershipType.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(membershipType.DurationMonths),
                    IsActive = true
                };

                entity.Memberships.Add(initialMembership);

                _context.Members.Add(entity);
                await _context.SaveChangesAsync(innerCt);
                return Result<Member>.Success(entity);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating the Member with Name {Name} and Email {Email}", dto.Name, dto.Email);
            return Result<Member>.Failure(Error.Unknown);
        }
    }

    public async Task<Result<Member>> UpdateAsync(Guid publicId, MemberUpdateDto dto, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("db-pipeline");
        try
        {
            var state = (Id: publicId, Data: dto);
            return await pipeline.ExecuteAsync(
                (Func<(Guid, MemberUpdateDto dto), CancellationToken, ValueTask<Result<Member>>>)Callback, state, ct);

            async ValueTask<Result<Member>> Callback((Guid Id, MemberUpdateDto Data) s, CancellationToken innerCt)
            {
                var emailExists = await _context.Members
                    .AnyAsync(x => x.Email == s.Data.Email && x.PublicId != s.Id, innerCt);
                if (emailExists) return Result<Member>.Failure(MemberErrors.EmailAlreadyExists);

                var entity = await _context.Members.FirstOrDefaultAsync(x => x.PublicId == s.Id, innerCt);
                if (entity == null) return Result<Member>.Failure(MemberErrors.NotFound);

                entity.Name = dto.Name;
                entity.Email = dto.Email;
                entity.Phone = dto.Phone ?? "";

                await _context.SaveChangesAsync(innerCt);
                return Result<Member>.Success(entity);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating the Member {PublicId}", publicId);
            return Result<Member>.Failure(Error.Unknown);
        }
    }
}