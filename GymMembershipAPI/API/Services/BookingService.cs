using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.API.DTOs.Shared;
using GymMembershipAPI.API.Extensions;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymMembershipAPI.API.Services;

public class BookingService : IBookingService
{
    private readonly ILogger<BookingService> _logger;
    private readonly GymDbContext _context;

    public BookingService(ILogger<BookingService> logger, GymDbContext context)
    {
        _logger = logger;
        _context = context;
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
        try
        {
            var result = await _context.Bookings
                .Include(b => b.Member)
                .Include(b => b.GroupClass)
                .OrderByDescending(b => b.CreatedAt)
                .ToPaginatedResultAsync(page, pageSize, ct);
            return Result<PaginatedResult<Booking>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar todos las reservas");
            return Result<PaginatedResult<Booking>>.Failure(
                Error.Unknown("Error inesperado al buscar todos las reservas"));
        }
    }

    public async Task<Result<PaginatedResult<Booking>>> GetByMemberPublicIdAsync(Guid memberPublicId, int page,
        int pageSize, CancellationToken ct)
    {
        try
        {
            var result = await _context.Bookings
                .Where(b => b.Member.PublicId == memberPublicId)
                .Include(b => b.Member)
                .Include(b => b.GroupClass)
                .OrderByDescending(b => b.CreatedAt)
                .ToPaginatedResultAsync(page, pageSize, ct);
            return Result<PaginatedResult<Booking>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar todos las reservas por ID publica del miembro: {guid}",
                memberPublicId);
            return Result<PaginatedResult<Booking>>.Failure(
                Error.Unknown("Error inesperado al buscar todos las reservas"));
        }
    }

    public async Task<Result<PaginatedResult<Booking>>> GetByClassPublicIdAsync(Guid groupClassPublicId, int page,
        int pageSize, CancellationToken ct)
    {
        try
        {
            var result = await _context.Bookings
                .Include(b => b.Member)
                .Include(b => b.GroupClass)
                .Where(b => b.GroupClass.PublicId == groupClassPublicId)
                .OrderByDescending(b => b.CreatedAt)
                .ToPaginatedResultAsync(page, pageSize, ct);
            return Result<PaginatedResult<Booking>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar todos las reservas con el ID publico {id}",
                groupClassPublicId);
            return Result<PaginatedResult<Booking>>.Failure(
                Error.Unknown("Error inesperado al buscar todos las reservas con el ID publico {id}"));
        }
    }

    public async Task<Result<Booking>> CreateAsync(BookingRequestDto dto, CancellationToken ct)
    {
        // Buscar miembro y sus membresías
        var member = await _context.Members.Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.PublicId == dto.MemberPublicId, ct);
        if (member == null) return Result<Booking>.Failure(BookingErrors.MemberNotFound);

        var hasActiveMembership = member.Memberships.Any(m =>
            m.IsActive && m.EndDate > DateTime.UtcNow);
        if (!hasActiveMembership) return Result<Booking>.Failure(BookingErrors.NoActiveMembership);

        //Busca la clase y sus reservas actuales
        var groupClass = await _context.GroupClasses.Include(e => e.Bookings)
            .FirstOrDefaultAsync(e => e.PublicId == dto.GroupClassPublicId, ct);
        if (groupClass == null) return Result<Booking>.Failure(BookingErrors.GroupClassNotFound);

        // Validaciones en memoria
        var activeBookingsCount = await _context.Bookings.CountAsync(b =>
            b.GroupClassId == groupClass.Id && b.State != "Cancelled", ct);
        if (activeBookingsCount >= groupClass.MaxMembers)
            return Result<Booking>.Failure(BookingErrors.ClassIsFull);

        var alreadyBooked = groupClass.Bookings.Any(b =>
            b.MemberId == member.Id && b.State != "Cancelada");
        if (alreadyBooked)
            return Result<Booking>.Failure(BookingErrors.AlreadyBooked);

        var entity = new Booking()
        {
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow,
            State = "Confirmada"
        };
        entity.GroupClassId = groupClass.Id;
        entity.MemberId = member.Id;

        _context.Bookings.Add(entity);

        groupClass.LastBookingAt = DateTime.UtcNow;
        try
        {
            await _context.SaveChangesAsync(ct);
            return Result<Booking>.Success(entity);
        }
        catch (DbUpdateConcurrencyException e)
        {
            _logger.LogWarning(e, "Concurrency conflict: The class {classId} has been modified by other user",
                dto.GroupClassPublicId);
            return Result<Booking>.Failure(BookingErrors.ClassIsFull);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured during booking creation");
            return Result<Booking>.Failure(Error.Unknown(e.Message));
        }
    }

    public async Task<Result> CancelAsync(Guid publicId, CancellationToken ct)
    {
        var entity = await _context.Bookings.FirstOrDefaultAsync(e => e.PublicId == publicId, ct);
        if (entity == null)
            return Result.Failure(BookingErrors.NotFound);
        if (entity.State == "Cancelada")
            return Result.Failure(BookingErrors.AlreadyCancelled);

        entity.State = "Cancelada";
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}