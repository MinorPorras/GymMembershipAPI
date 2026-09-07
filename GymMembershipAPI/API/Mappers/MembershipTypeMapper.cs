using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.Domain.Entities;

namespace GymMembershipAPI.API.Mappers;

public class MembershipTypeMapper
{
    public static MembershipType ToEntity(CreateMembershipTypeDto dto)
    {
        return new MembershipType
        {
            Name = dto.Name,
            Price = dto.Price,
            DurationMonths = dto.DurationMonths
        };
    }

    public static MembershipType ToEntity(UpdateMembershipTypeDto dto)
    {
        return new MembershipType
        {
            Name = dto.Name,
            Price = dto.Price,
            DurationMonths = dto.DurationMonths
        };
    }

    public static MembershipTypeResponseDto ToResponseDto(MembershipType entity)
    {
        return new MembershipTypeResponseDto(
            entity.PublicId,
            entity.Name,
            entity.Price,
            entity.DurationMonths
        );
    }

    public static IEnumerable<MembershipTypeResponseDto> ToResponseDto(IEnumerable<MembershipType> entities)
    {
        return entities.Select(ToResponseDto);
    }
}