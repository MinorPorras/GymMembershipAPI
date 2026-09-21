using System.ComponentModel.DataAnnotations;

namespace GymMembershipAPI.API.DTOs.Auth;

public record LoginRequestDto(
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato correo electrónico no es válido")]
    string Email,
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    string Password
);