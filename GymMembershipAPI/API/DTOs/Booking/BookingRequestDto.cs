using System.ComponentModel.DataAnnotations;

namespace GymMembershipAPI.API.DTOs.Booking;

public record BookingRequestDto(
    [Required(ErrorMessage = "El ID del miembro es obligatorio")]
    Guid MemberPublicId,
    [Required(ErrorMessage = "El ID de la clase grupal es obligatorio")]
    Guid GroupClassPublicId
);