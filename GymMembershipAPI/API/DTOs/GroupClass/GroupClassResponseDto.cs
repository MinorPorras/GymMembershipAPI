namespace GymMembershipAPI.API.DTOs.GroupClass;

public record GroupClassResponseDto(
    Guid PublicId,
    string Name,
    string Instructor,
    DateTime DateHour,
    int MaxMembers
    );