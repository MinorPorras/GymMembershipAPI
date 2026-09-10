namespace GymMembershipAPI.API.DTOs.Booking;

public record BookingRequestDto(
    Guid MemberPublicId,
    Guid GroupClassPublicId,
    DateTime BookingDate
);