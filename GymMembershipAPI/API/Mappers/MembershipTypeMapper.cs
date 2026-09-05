using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.Domain.Entities;

namespace GymMembershipAPI.API.Mappers;

public class MembershipTypeMapper
{
    public static MembershipType ToEntity(CreateMemberShipTypeDto dto)
    {
        return new MembershipType
        {
            Name = dto.name,
            Price = dto.price,
            MonthDurantion = dto.monthDurantion
        };
    }

    public static MembershipTypeResponseDto ToResponseDto(MembershipType entity)
    {
        return new MembershipTypeResponseDto(
            entity.Id,
            entity.Name,
            entity.Price,
            entity.MonthDurantion
        );
    }

    public static IEnumerable<MembershipTypeResponseDto> ToResponseDto(IEnumerable<MembershipType> entities)
    {
        return entities.Select(ToResponseDto);
    }
}