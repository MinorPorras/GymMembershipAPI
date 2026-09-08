using GymMembershipAPI.API.DTOs.Membership;
using GymMembershipAPI.Domain.Entities;

namespace GymMembershipAPI.API.Mappers;

public class MembershipMapper
{
    public static Membership ToEntity(MembershipRequestDto dto)
    {
        return new Membership()
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow,
            IsActive = true
        };
    }

    public static MembershipResponseDto ToResponseDto(Membership entity)
    {
        return new MembershipResponseDto(
            entity.PublicId,
            entity.Member.PublicId,  
            entity.MembershipType.PublicId,
            entity.StartDate,
            entity.EndDate,
            entity.IsActive
        );
    }

    public static IEnumerable<MembershipResponseDto> ToResponseDtos(IEnumerable<Membership> entities)
    {
        return entities.Select(ToResponseDto).ToList();
    }
}