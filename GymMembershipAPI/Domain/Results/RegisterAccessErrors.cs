namespace GymMembershipAPI.Domain.Results;

public class RegisterAccessErrors
{
    public static readonly Error NotFound = new Error(
        "RegisterAccess.NotFound",
        "The requested access was not found."
    );
}