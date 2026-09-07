using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.Domain.Entities;

namespace GymMembershipAPI.API.Mappers;

public class MemberMapper
{
    public static Member ToEntity(MemberRequestDto requestDto)
    {
        return new Member
        {
            Name = requestDto.Name,
            Email = requestDto.Email,
            Phone = requestDto.Phone,
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