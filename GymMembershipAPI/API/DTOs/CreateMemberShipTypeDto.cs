namespace GymMembershipAPI.API.DTOs;

public record CreateMemberShipTypeDto(
    string name, 
    decimal price, 
    int monthDurantion);