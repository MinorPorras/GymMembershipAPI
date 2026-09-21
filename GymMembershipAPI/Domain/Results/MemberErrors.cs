namespace GymMembershipAPI.Domain.Results;

public class MemberErrors
{
    public static readonly Error NotFound = new(
        "MemberErrors.NotFound",
        "The requested member was not found."
    );

    public static readonly Error EmailAlreadyExists = new(
        "MemberErrors.AlreadyExists",
        "A Member with this email direction already exists."
    );
}