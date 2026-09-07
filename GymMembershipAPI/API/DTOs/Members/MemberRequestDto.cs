namespace GymMembershipAPI.API.DTOs.Members;

public record MemberRequestDto(
    string Name,
    string Email,
    string Phone
);