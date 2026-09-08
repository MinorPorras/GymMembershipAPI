using GymMembershipAPI.API.DTOs.GroupClass;
using GymMembershipAPI.Domain.Entities;

namespace GymMembershipAPI.API.Mappers;

public class GroupClassMapper
{
    public static GroupClass ToEntity(GroupClassRequestDto dto)
    {
        return new GroupClass()
        {
            Name = dto.Name,
            Instructor = dto.Instructor,
            DateHour = dto.DateHour,
            MaxMembers = dto.MaxMembers
        };
    }

    public static GroupClassResponseDto ToDto(GroupClass entity)
    {
        return new GroupClassResponseDto(
            entity.PublicId,
            entity.Name,
            entity.Instructor,
            entity.DateHour,
            entity.MaxMembers
        );
    }

    public static List<GroupClassResponseDto> ToDtos(List<GroupClass> entities)
    {
        return entities.Select(ToDto).ToList();
    }
}