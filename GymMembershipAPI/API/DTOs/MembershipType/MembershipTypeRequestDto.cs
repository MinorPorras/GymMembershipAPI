namespace GymMembershipAPI.API.DTOs.MembershipType;

public record MembershipTypeRequestDto(
    string Name, 
    decimal Price, 
    int DurationMonths);