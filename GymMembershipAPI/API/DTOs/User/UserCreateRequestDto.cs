using System.ComponentModel.DataAnnotations;
using GymMembershipAPI.Domain.Enums;

namespace GymMembershipAPI.API.DTOs.User;

public record UserCreateRequestDto(
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    string Email,
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe de tener entre 6 y 100 caracteres")]
    string Password,
    [Required(ErrorMessage = "El rol es obligatorio")]
    string Role,
    Guid? MemberPublicId
);