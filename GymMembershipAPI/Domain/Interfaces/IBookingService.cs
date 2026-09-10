using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IBookingService : IServiceBase<Booking>
{
    Task<Result<List<Booking>>> GetAllBookings();
    Task<Result<List<Booking>>> GetBookingByMemberPublicId(Guid memberPublicId);
    Task<Result<List<Booking>>> GetByClassPublicIdAsync(Guid groupClassPublicId);
    Task<Result<Booking>> CreateAsync(BookingRequestDto booking);
    Task<Result> CancelAsync(Guid publicId);
}