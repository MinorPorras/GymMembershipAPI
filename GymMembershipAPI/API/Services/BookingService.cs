using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.API.DTOs.Shared;
using GymMembershipAPI.API.Extensions;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Polly.Registry;

namespace GymMembershipAPI.API.Services;

public class BookingService : IBookingService
{
    private readonly ILogger<BookingService> _logger;
    private readonly GymDbContext _context;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;

    public BookingService(ILogger<BookingService> logger, GymDbContext context,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _logger = logger;
        _context = context;
        _pipelineProvider = pipelineProvider;
    }

    public async Task<Result<Booking>> GetByPublicIdAsync(Guid publicId, CancellationToken ct)
    {
        var entity = await _context.Bookings
            .Include(e => e.Member)
            .Include(e => e.GroupClass)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(e => e.PublicId == publicId, ct);
        return entity == null
            ? Result<Booking>.Failure(BookingErrors.NotFound)
            : Result<Booking>.Success(entity);
    }


    public async Task<Result<PaginatedResult<Booking>>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("db-pipeline");

        try
        {
            var state = (Page: page, PageSize: pageSize);

            return await pipeline.ExecuteAsync(Callback, state, ct);

            async ValueTask<Result<PaginatedResult<Booking>>> Callback((int Page, int PageSize) s,
                CancellationToken innerCt)
            {
                var result = await _context.Bookings
                    .Include(b => b.Member)
                    .Include(b => b.GroupClass)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToPaginatedResultAsync(page, pageSize, innerCt);
                return Result<PaginatedResult<Booking>>.Success(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar todos las reservas");
            return Result<PaginatedResult<Booking>>.Failure(Error.Unknown);
        }
    }

    public async Task<Result<PaginatedResult<Booking>>> GetByMemberPublicIdAsync(
        Guid memberPublicId, int page, int pageSize, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("db-pipeline");
        try
        {
            var state = (MemberId: memberPublicId, Page: page, PageSize: pageSize);

            return await pipeline.ExecuteAsync((Func<(Guid MemberId, int Page, int PageSize), CancellationToken,
                ValueTask<Result<PaginatedResult<Booking>>>>)Callback, state, ct);

            async ValueTask<Result<PaginatedResult<Booking>>> Callback((Guid MemberId, int Page, int PageSize) s,
                CancellationToken innerCt)
            {
                var result = await _context.Bookings.Where(b => b.Member.PublicId == s.MemberId)
                    .Include(b => b.Member)
                    .Include(b => b.GroupClass)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToPaginatedResultAsync(s.Page, s.PageSize, innerCt);

                return Result<PaginatedResult<Booking>>.Success(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar reservas por ID pública del miembro: {guid}",
                memberPublicId);
            return Result<PaginatedResult<Booking>>.Failure(Error.Unknown);
        }
    }

    public async Task<Result<PaginatedResult<Booking>>> GetByClassPublicIdAsync(Guid groupClassPublicId, int page,
        int pageSize, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("db-pipeline");
        try
        {
            var state = (GroupClassPublicId: groupClassPublicId, Page: page, PageSize: pageSize);

            return await pipeline.ExecuteAsync(Callback, state, ct);

            async ValueTask<Result<PaginatedResult<Booking>>> Callback(
                (Guid groupClassPublicId, int Page, int PageSize) s, CancellationToken innerCt)
            {
                var result = await _context.Bookings
                    .Include(b => b.Member)
                    .Include(b => b.GroupClass)
                    .Where(b => b.GroupClass.PublicId == groupClassPublicId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToPaginatedResultAsync(page, pageSize, innerCt);
                return Result<PaginatedResult<Booking>>.Success(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar todos las reservas con el ID publico {id}",
                groupClassPublicId);
            return Result<PaginatedResult<Booking>>.Failure(Error.Unknown);
        }
    }

    public async Task<Result<Booking>> CreateAsync(BookingRequestDto dto, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("db-pipeline");

        try
        {
            return await pipeline.ExecuteAsync(
                (Func<BookingRequestDto, CancellationToken, ValueTask<Result<Booking>>>)Callback, dto, ct);

            async ValueTask<Result<Booking>> Callback(BookingRequestDto state, CancellationToken innerCt)
            {
                // --- FASE DE LECTURA ---
                var member = await _context.Members.Include(e => e.Memberships)
                    .FirstOrDefaultAsync(e => e.PublicId == state.MemberPublicId, innerCt);

                if (member == null) return Result<Booking>.Failure(BookingErrors.MemberNotFound);

                var hasActiveMembership = member.Memberships.Any(m => m.IsActive && m.EndDate > DateTime.UtcNow);

                if (!hasActiveMembership) return Result<Booking>.Failure(BookingErrors.NoActiveMembership);

                var groupClass = await _context.GroupClasses.Include(e => e.Bookings)
                    .FirstOrDefaultAsync(e => e.PublicId == state.GroupClassPublicId, innerCt);

                if (groupClass == null) return Result<Booking>.Failure(BookingErrors.GroupClassNotFound);

                var activeBookingsCount =
                    await _context.Bookings.CountAsync(b => b.GroupClassId == groupClass.Id && b.State != "Cancelled",
                        innerCt);

                if (activeBookingsCount >= groupClass.MaxMembers)
                    return Result<Booking>.Failure(BookingErrors.ClassIsFull);

                var alreadyBooked = groupClass.Bookings.Any(b => b.MemberId == member.Id && b.State != "Cancelled");

                if (alreadyBooked) return Result<Booking>.Failure(BookingErrors.AlreadyBooked);

                // --- FASE DE ESCRITURA ---
                var entity = new Booking
                {
                    GroupClassId = groupClass.Id, MemberId = member.Id, CreatedAt = DateTime.UtcNow,
                    State = "Confirmada"
                };

                _context.Bookings.Add(entity);
                groupClass.LastBookingAt = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync(innerCt);
                    return Result<Booking>.Success(entity);
                }
                catch (DbUpdateConcurrencyException)
                {
                    _logger.LogWarning("Conflicto de concurrencia al crear reserva para la clase {classId}",
                        state.GroupClassPublicId);
                    return Result<Booking>.Failure(BookingErrors.ClassIsFull);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo crítico tras agotar reintentos en la creación de reserva");
            return Result<Booking>.Failure(Error.Unknown);
        }
    }

    public async Task<Result> CancelAsync(Guid publicId, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("db-pipeline");
        try
        {
            return await pipeline.ExecuteAsync(
                (Func<Guid, CancellationToken, ValueTask<Result>>)Callback,
                publicId,
                ct);

            async ValueTask<Result> Callback(Guid state, CancellationToken innerCt)
            {
                var entity = await _context.Bookings
                    .FirstOrDefaultAsync(e => e.PublicId == state, innerCt);
                if (entity == null)
                    return Result.Failure(BookingErrors.NotFound);
                if (entity.State == "Cancelada")
                    return Result.Failure(BookingErrors.AlreadyCancelled);

                entity.State = "Cancelada";
                await _context.SaveChangesAsync(innerCt);
                return Result.Success();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fallo crítico tras agotar reintentos al cancelar la reserva {PublicId}", publicId);
            return Result.Failure(Error.Unknown);
        }
    }
}