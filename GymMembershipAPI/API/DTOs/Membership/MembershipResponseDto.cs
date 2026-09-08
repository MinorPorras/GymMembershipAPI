namespace GymMembershipAPI.API.DTOs.Membership;

public record MembershipResponseDto(
    Guid PublicId,
    Guid MemberPublicId,
    Guid MembershipTypePublicId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);