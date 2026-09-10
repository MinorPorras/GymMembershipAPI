namespace GymMembershipAPI.Domain.results;

public class RegisterAccessErrors
{
    public static readonly Error NotFound = new Error(
        "RegisterAccess.NotFound",
        "The requested access was not found."
    );

    public static readonly Error InvalidDate = new Error(
        "RegisterAccess.InvalidDate",
        "The date was not valid, it must be from today o newer"
    );
}