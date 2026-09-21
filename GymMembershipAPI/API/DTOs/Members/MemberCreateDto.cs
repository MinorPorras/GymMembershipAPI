using System.ComponentModel.DataAnnotations;

namespace GymMembershipAPI.API.DTOs.Members;

public record MemberCreateDto(
    [Required(ErrorMessage = "El nombre del miembro es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe de tener entre 3 y 100 caracteres")]
    string Name,
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    string Email,
    [StringLength(20, ErrorMessage = "El número de teléfono no puede exceder los 20 caracteres")]
    string? Phone,
    [Required(ErrorMessage = "El tipo de membresía es obligatorio")]
    Guid MembershipTypePublicId
);