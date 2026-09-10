namespace GymMembershipAPI.API.DTOs.RegisterAccess;

public record RegisterAccessResponseDto(
    Guid PublicId,
    Guid MemberPublciId,
    DateTime AccessDate,
    bool AllowAccess
);