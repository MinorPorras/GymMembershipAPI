namespace GymMembershipAPI.API.DTOs.GroupClass;

public record GroupClassRequestDto(
    string Name,
    string Instructor,
    DateTime DateHour,
    int MaxMembers
    );