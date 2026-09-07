namespace GymMembershipAPI.API.DTOs.MembershipType;

public record UpdateMembershipTypeDto(
    string Name,
    decimal Price,
    int DurationMonths);