using System.ComponentModel.DataAnnotations;

namespace GymMembershipAPI.API.DTOs.GroupClass;

public record GroupClassRequestDto(
    [Required(ErrorMessage = "El nombre es  obligatorio")]
    [StringLength(150, MinimumLength = 3,
        ErrorMessage = "El nombre de la clase debe de tener entre 1 y 150 caracteres")]
    string Name,
    [Required(ErrorMessage = "El nombre del instructor es  obligatorio")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "El nombre del instructor debe de tener entre 1 y 100 caracteres")]
    string Instructor,
    [Required(ErrorMessage = "La fecha de la clase es obligatoria")]
    DateTime DateHour,
    [Required(ErrorMessage = "La cantidad máxima de miembros es obligatoria")]
    [Range(1, 1000, ErrorMessage = "Los espacios de la clase deben de estar entre 1 y 1000")]
    int MaxMembers
);