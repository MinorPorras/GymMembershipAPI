using System.ComponentModel.DataAnnotations;

namespace GymMembershipAPI.API.DTOs.MembershipType;

public record MembershipTypeRequestDto(
    [Required(ErrorMessage = "El nombre del miembro es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe de tener entre 3 y 100 caracteres")]
    string Name,
    [Required(ErrorMessage = "El precio de la membresía es  obligatorio.")]
    [Range(0.01, 10000, ErrorMessage = "El precio debe ser mayor a 0")]
    decimal Price,
    [Required(ErrorMessage = "La duración en meses es obligatoria")]
    [Range(1, 120, ErrorMessage = "La  duración en meses debe ser mayor a 0")]
    int DurationMonths);