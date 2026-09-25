using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.API.DTOs.Shared;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IBookingService : IServiceBase<Booking>
{
    Task<Result<PaginatedResult<Booking>>> GetAllAsync(int page, int pageSize, CancellationToken ct);

    Task<Result<PaginatedResult<Booking>>> GetByMemberPublicIdAsync(Guid memberPublicId, int page, int pageSize,
        CancellationToken ct);

    Task<Result<PaginatedResult<Booking>>> GetByClassPublicIdAsync(Guid groupClassPublicId, int page, int pageSize,
        CancellationToken ct);

    Task<Result<Booking>> CreateAsync(BookingRequestDto booking, CancellationToken ct);
    Task<Result> CancelAsync(Guid publicId, CancellationToken ct);
}