namespace GymMembershipAPI.API.DTOs.Booking;

public record BookingResponseDto(
    Guid PublicId,
    Guid MemberPublicId,
    Guid GroupClassPublicId,
    DateTime BookingDate,
    string State
);