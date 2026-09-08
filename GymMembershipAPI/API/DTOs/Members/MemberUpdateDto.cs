namespace GymMembershipAPI.API.DTOs.Members;

public record MemberUpdateDto(
    string Name,
    string Email,
    string Phone
);