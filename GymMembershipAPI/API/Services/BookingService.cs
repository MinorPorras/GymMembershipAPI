using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using GymMembershipAPI.Infraestructure.Data;

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

    public Task<Result<Booking>> GetByPublicIdAsync(Guid publicId)
    {
        throw new NotImplementedException();
    }

    
    public Task<Result<List<Booking>>> GetAllBookings()
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<Booking>>> GetBookingByMemberPublicId(Guid memberPublicId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<Booking>>> GetByClassPublicIdAsync(Guid groupClassPublicId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Booking>> CreateAsync(BookingRequestDto booking)
    {
        throw new NotImplementedException();
    }

    public Task<Result> CancelAsync(Guid publicId)
    {
        throw new NotImplementedException();
    }
}