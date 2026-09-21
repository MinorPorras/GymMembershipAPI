using System.ComponentModel.DataAnnotations;

namespace GymMembershipAPI.API.DTOs.Membership;

public record MembershipRequestDto(
    [Required(ErrorMessage = "El ID del miembro es obligatoria")]
    Guid MemberPublicId,
    [Required(ErrorMessage = "El ID del tipo de membresía es obligatoria")]
    Guid MembershipTypePublicId
);