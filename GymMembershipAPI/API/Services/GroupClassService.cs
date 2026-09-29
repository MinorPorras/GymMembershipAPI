using GymMembershipAPI.API.DTOs.GroupClass;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using Polly.Registry;

namespace GymMembershipAPI.API.Services;

public class GroupClassService : IGroupClassService
{
    private readonly ILogger<GroupClassService> _logger;
    private readonly GymDbContext _context;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;

    public GroupClassService(ILogger<GroupClassService> logger, GymDbContext context,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _logger = logger;
        _context = context;
        _pipelineProvider = pipelineProvider;
    }

    private static Result IsValidDto(GroupClassRequestDto dto, Guid? excludePubliId = null) =>
        dto.DateHour.Date < DateTime.UtcNow.Date
            ? Result.Failure(GroupClassErrors.InvalidDate)
            : Result.Success();

    public async Task<Result<GroupClass>> GetByPublicIdAsync(Guid publicId, CancellationToken ct)
    {
        var existingClass = await _context.GroupClasses.FirstOrDefaultAsync(c => c.PublicId == publicId, ct);
        return existingClass == null
            ? Result<GroupClass>.Failure(GroupClassErrors.NotFound)
            : Result<GroupClass>.Success(existingClass);
    }

    public async Task<Result<List<GroupClass>>> GetAllAsync(CancellationToken ct)
    {
        var list = await _context.GroupClasses.ToListAsync(ct);
        return Result<List<GroupClass>>.Success(list);
    }

    public async Task<Result<List<GroupClass>>> GetByDateAsync(DateTime date, CancellationToken ct)
    {
        var list = await _context.GroupClasses.Where(c => c.DateHour.Date == date.Date).ToListAsync(ct);
        return Result<List<GroupClass>>.Success(list);
    }

    public async Task<Result<GroupClass>> CreateAsync(GroupClassRequestDto dto, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("db-pipeline");

        try
        {
            Func<GroupClassRequestDto, CancellationToken, ValueTask<Result<GroupClass>>> callback =
                async (state, innerCt) =>
                {
                    var isValid = IsValidDto(state);
                    if (isValid.IsFailure)
                        return Result<GroupClass>.Failure(isValid.Error);

                    var nameAlreadyExists = await _context.GroupClasses
                        .AnyAsync(c => c.Name == state.Name, innerCt);

                    if (nameAlreadyExists)
                        return Result<GroupClass>.Failure(GroupClassErrors.NameAlreadyExists);

                    var entity = GroupClassMapper.ToEntity(state);

                    try
                    {
                        _context.GroupClasses.Add(entity);
                        await _context.SaveChangesAsync(innerCt);
                        return Result<GroupClass>.Success(entity);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to create GroupClass with name {Name}", entity.Name);
                        return Result<GroupClass>.Failure(Error.Unknown);
                    }
                };
            return await pipeline.ExecuteAsync(callback, dto, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inesperado al crear la clase");
            return Result<GroupClass>.Failure(Error.Unknown);
        }
    }

    public async Task<Result> DeleteAsync(Guid publicId, CancellationToken ct)
    {
        var existingClass = await _context.GroupClasses.FirstOrDefaultAsync(c => c.PublicId == publicId, ct);
        if (existingClass == null) return Result.Failure(GroupClassErrors.NotFound);
        try
        {
            _context.GroupClasses.Remove(existingClass);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete GroupClass {PublicId}", existingClass.PublicId);
            return Result.Failure(Error.Unknown);
        }
    }

    public async Task<Result<GroupClass>> UpdateAsync(Guid publicId, GroupClassRequestDto dto, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("db-pipeline");

        try
        {
            var state = (PublicId: publicId, Dto: dto);

            Func<(Guid PublicId, GroupClassRequestDto Dto), CancellationToken, ValueTask<Result<GroupClass>>> callback =
                async (s, innerCt) =>
                {
                    var existingClass = await _context.GroupClasses
                        .FirstOrDefaultAsync(c => c.PublicId == s.PublicId, innerCt);

                    if (existingClass == null)
                        return Result<GroupClass>.Failure(GroupClassErrors.NotFound);

                    var isValid = IsValidDto(s.Dto);
                    if (isValid.IsFailure) return Result<GroupClass>.Failure(isValid.Error);

                    if (existingClass.Name != s.Dto.Name)
                    {
                        var nameAlreadyExists = await _context.GroupClasses
                            .AnyAsync(c => c.Name == s.Dto.Name, innerCt);

                        if (nameAlreadyExists)
                            return Result<GroupClass>.Failure(GroupClassErrors.NameAlreadyExists);
                    }

                    try
                    {
                        existingClass.Name = s.Dto.Name;
                        existingClass.Instructor = s.Dto.Instructor;
                        existingClass.DateHour = s.Dto.DateHour;
                        existingClass.MaxMembers = s.Dto.MaxMembers;

                        await _context.SaveChangesAsync(innerCt);
                        return Result<GroupClass>.Success(existingClass);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to update GroupClass {PublicId}", existingClass.PublicId);
                        return Result<GroupClass>.Failure(Error.Unknown);
                    }
                };

            return await pipeline.ExecuteAsync(callback, state, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inesperado al actualizar la clase");
            return Result<GroupClass>.Failure(Error.Unknown);
        }
    }
}