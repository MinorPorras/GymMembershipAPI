namespace GymMembershipAPI.Domain.results;

public class BookingErrors
{
    public static readonly Error NotFound = new Error(
        "BookingError.NotFound",
        "The especified booking was not found."
    );

    public static readonly Error MemberNotFound = new Error(
        "BookingError.MemberNotFound",
        "The member could not be found."
    );

    public static readonly Error GroupClassNotFound = new Error(
        "BookingError.GroupClassNotFound",
        "The class could not be found."
    );

    public static readonly Error NoActiveMembership = new Error(
        "BookingError.NoActiveMembership",
        "The member has no active membership."
    );

    public static readonly Error ClassIsFull = new Error(
        "BookingError.ClassIsFull",
        "The class is already full."
    );

    public static readonly Error AlreadyBooked = new Error(
        "BookingError.AlreadyBooked",
        "The member is already booked for this class."
    );

    public static readonly Error AlreadyCancelled = new Error(
        "BookingError.AlreadyCancelled",
        "The booking for this class was already cancelled"
    );
}