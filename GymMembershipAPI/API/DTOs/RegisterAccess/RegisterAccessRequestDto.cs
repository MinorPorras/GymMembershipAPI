namespace GymMembershipAPI.API.DTOs.RegisterAccess;

public record RegisterAccessRequestDto(
    Guid MemberPublciId,
    DateTime AccessDate,
    bool AllowAccess
);