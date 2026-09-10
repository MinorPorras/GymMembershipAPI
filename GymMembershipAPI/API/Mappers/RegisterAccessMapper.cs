using GymMembershipAPI.API.DTOs.RegisterAccess;
using GymMembershipAPI.Domain.Entities;

namespace GymMembershipAPI.API.Mappers;

public class RegisterAccessMapper
{
    public static RegisterAccessResponseDto ToResponseDto(RegisterAccess entity) => new(
        PublicId: entity.PublicId,
        MemberPublciId: entity.PublicId,
        AccessDate: entity.AccessDate,
        AllowAccess: entity.AllowAccess 
    );
    
    public static IEnumerable<RegisterAccessResponseDto> ToResponseDtos(IEnumerable<RegisterAccess> entities) => entities.Select(ToResponseDto);
}