namespace GymMembershipAPI.API.DTOs;

public record UpdateMembershipTypeDto(
    int id,
    string name,
    decimal price,
    int monthDurantion);