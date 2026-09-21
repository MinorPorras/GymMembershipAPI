namespace GymMembershipAPI.API.DTOs.Auth;

public record LoginResponseDto(
    string Token,
    DateTime ExpiresIn,
    string Role,
    Guid? MemberPublicId
);