using System.ComponentModel.DataAnnotations;

namespace GymMembershipAPI.API.DTOs.RegisterAccess;

public record RegisterAccessRequestDto(
    [Required(ErrorMessage = "El Id del miembro es obligatorio")]
    Guid MemberPublicId
);