namespace GymMembershipAPI.API.DTOs.Members;

public record MemberResponseDto(
    Guid PublicId,
    string Name,
    string Email,
    string Telefono,
    bool IsActive
);