namespace GymMembershipAPI.API.DTOs.Members;

public record MemberCreateDto(
    string Name,
    string Email,
    string Phone,
    Guid MembershipTypePublicId
);