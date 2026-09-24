using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IBookingService : IServiceBase<Booking>
{
    Task<Result<List<Booking>>> GetAllAsync(CancellationToken ct);
    Task<Result<List<Booking>>> GetByMemberPublicIdAsync(Guid memberPublicId, CancellationToken ct);
    Task<Result<List<Booking>>> GetByClassPublicIdAsync(Guid groupClassPublicId, CancellationToken ct);
    Task<Result<Booking>> CreateAsync(BookingRequestDto booking, CancellationToken ct);
    Task<Result> CancelAsync(Guid publicId, CancellationToken ct);
}