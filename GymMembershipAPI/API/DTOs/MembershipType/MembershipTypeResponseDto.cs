namespace GymMembershipAPI.API.DTOs.MembershipType;

public record MembershipTypeResponseDto(
    Guid PublicId, 
    string Name, 
    decimal Price, 
    int DurationMonths);