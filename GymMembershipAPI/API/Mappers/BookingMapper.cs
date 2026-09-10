using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.Domain.Entities;

namespace GymMembershipAPI.API.Mappers;

public class BookingMapper
{
    public static Booking ToEntity(BookingRequestDto dto) => new Booking()
    {
        BookingDate = dto.BookingDate,
        State = "Confirmada"
    };

    public static BookingResponseDto ToDto(Booking entity) => new(
        PublicId: entity.PublicId,
        MemberPublicId: entity.Member.PublicId,
        GroupClassPublicId: entity.GroupClass.PublicId,
        BookingDate: entity.BookingDate,
        State: entity.State
    );

    public static IEnumerable<BookingResponseDto> ToDto(IEnumerable<Booking> entities) => entities.Select(ToDto);
}