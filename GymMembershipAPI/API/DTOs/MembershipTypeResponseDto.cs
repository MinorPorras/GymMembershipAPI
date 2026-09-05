namespace GymMembershipAPI.API.DTOs;

public record MembershipTypeResponseDto(
    int id, 
    string name, 
    decimal price, 
    int monthDurantion);