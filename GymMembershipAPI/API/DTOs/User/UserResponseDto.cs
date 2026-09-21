namespace GymMembershipAPI.API.DTOs.User;

public record UserResponseDto(
    Guid PublicId,
    string Email,
    string Role, 
    Guid? MemberPublicId
    );