using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.Domain.Entities;

namespace GymMembershipAPI.API.Mappers;

public class MembershipTypeMapper
{
    public static MembershipType ToEntity(MembershipTypeRequestDto requestDto)
    {
        return new MembershipType
        {
            Name = requestDto.Name,
            Price = requestDto.Price,
            DurationMonths = requestDto.DurationMonths
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