using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.Domain.Entities;

namespace GymMembershipAPI.API.Mappers;

public class BookingMapper
{
    public static BookingResponseDto ToResponseDto(Booking entity) => new(
        PublicId: entity.PublicId,
        MemberPublicId: entity.Member.PublicId,
        GroupClassPublicId: entity.GroupClass.PublicId,
        ClassDate: entity.GroupClass.DateHour,
        BookingDate: entity.CreatedAt,
        State: entity.State
    );

    public static IEnumerable<BookingResponseDto> ToResponseDtos(IEnumerable<Booking> entities) =>
        entities.Select(ToResponseDto);
}