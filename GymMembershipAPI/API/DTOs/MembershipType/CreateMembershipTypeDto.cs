namespace GymMembershipAPI.API.DTOs.MembershipType;

public record CreateMembershipTypeDto(
    string Name, 
    decimal Price, 
    int DurationMonths);