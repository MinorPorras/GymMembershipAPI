using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
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

    public async Task<Result<Booking>> GetByPublicIdAsync(Guid publicId)
    {
        var entity = await _context.Bookings
            .Include(e => e.Member)
            .Include(e => e.GroupClass)
            .FirstOrDefaultAsync(e => e.PublicId == publicId);
        return entity == null
            ? Result<Booking>.Failure(BookingErrors.NotFound)
            : Result<Booking>.Success(entity);
    }


    public async Task<Result<List<Booking>>> GetAllBookings()
    {
        var list = await _context.Bookings
            .Include(e => e.Member)
            .Include(e => e.GroupClass)
            .ToListAsync();
        return Result<List<Booking>>.Success(list);
    }

    public async Task<Result<List<Booking>>> GetBookingByMemberPublicId(Guid memberPublicId)
    {
        var list = await _context.Bookings
            .Include(e => e.Member)
            .Include(e => e.GroupClass)
            .Where(e => e.Member.PublicId == memberPublicId)
            .ToListAsync();
        return Result<List<Booking>>.Success(list);
    }

    public async Task<Result<List<Booking>>> GetByClassPublicIdAsync(Guid groupClassPublicId)
    {
        var list = await _context.Bookings
            .Include(e => e.Member)
            .Include(e => e.GroupClass)
            .Where(e => e.GroupClass.PublicId == groupClassPublicId)
            .ToListAsync();
        return Result<List<Booking>>.Success(list);
    }

    public async Task<Result<Booking>> CreateAsync(BookingRequestDto dto)
    {
        // Buscar miembro y sus membresías
        var member = await _context.Members.Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.PublicId == dto.MemberPublicId);
        if (member == null) return Result<Booking>.Failure(BookingErrors.MemberNotFound);

        var hasActiveMembership = member.Memberships.Any(m =>
            m.IsActive && m.EndDate > DateTime.UtcNow);
        if (!hasActiveMembership) return Result<Booking>.Failure(BookingErrors.NoActiveMembership);

        //Busca la clase y sus reservas actuales
        var groupClass = await _context.GroupClasses.Include(e => e.Bookings)
            .FirstOrDefaultAsync(e => e.PublicId == dto.GroupClassPublicId);
        if (groupClass == null) return Result<Booking>.Failure(BookingErrors.GroupClassNotFound);

        // Validaciones en memoria
        var activeBookings = groupClass.Bookings.Count(b => b.State != "Cancelada");
        if (activeBookings >= groupClass.MaxMembers)
            return Result<Booking>.Failure(BookingErrors.ClassIsFull);

        var alreadyBooked = groupClass.Bookings.Any(b =>
            b.MemberId == member.Id && b.State != "Cancelada");
        if (alreadyBooked)
            return Result<Booking>.Failure(BookingErrors.AlreadyBooked);

        var entity = BookingMapper.ToEntity(dto);
        entity.GroupClassId = groupClass.Id;
        entity.MemberId = member.Id;

        _context.Bookings.Add(entity);

        groupClass.LastBookingAt = DateTime.UtcNow;
        try
        {
            await _context.SaveChangesAsync();
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

    public async Task<Result> CancelAsync(Guid publicId)
    {
        var entity = await _context.Bookings.FirstOrDefaultAsync(e => e.PublicId == publicId);
        if (entity == null)
            return Result.Failure(BookingErrors.NotFound);
        if (entity.State == "Cancelled")
            return Result.Failure(BookingErrors.AlreadyCancelled);

        entity.State = "Cancelled";
        await _context.SaveChangesAsync();
        return Result.Success();
    }
}