using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.Domain.Entities;

namespace GymMembershipAPI.API.Mappers;

public class MemberMapper
{
    public static Member ToEntity(MemberCreateDto createDto)
    {
        return new Member
        {
            Name = createDto.Name,
            Email = createDto.Email,
            Phone = createDto.Phone,
        };
    }

    public static MemberResponseDto ToResponseDto(Member entity)
    {
        return new MemberResponseDto(
            PublicId: entity.PublicId,
            Name: entity.Name,
            Email: entity.Email,
            Phone: entity.Phone,
            IsActive: entity.IsActive
        );
    }

    public static IEnumerable<MemberResponseDto> ToResponseDto(IEnumerable<Member> entities)
    {
        return entities.Select(ToResponseDto);
    }
}