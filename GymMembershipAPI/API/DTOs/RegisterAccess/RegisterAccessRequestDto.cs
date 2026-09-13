namespace GymMembershipAPI.API.DTOs.RegisterAccess;

public record RegisterAccessRequestDto(
    Guid MemberPublicId,
    DateTime AccessDate
);