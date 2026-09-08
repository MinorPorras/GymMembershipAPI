namespace GymMembershipAPI.API.DTOs.Membership;

public record MembershipRequestDto(
    Guid MemberPublicId,
    Guid MembershipTypePublicId
);