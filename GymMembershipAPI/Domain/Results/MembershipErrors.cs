namespace GymMembershipAPI.Domain.Results;

public class MembershipErrors
{
    public static readonly Error NotFound = new(
        "MembershipErrors.NotFound",
        "The requested membership was not found."
    );

    public static readonly Error MemberNotFound = new(
        "MembershipErrors.MemberNotFound",
        "The requested membership member was not found."
    );

    public static readonly Error MembershipTypeNotFound = new(
        "MembershipErrors.MemberNotFound",
        "The requested membership member was not found."
    );

    public static readonly Error AlreadyCancelled = new(
        "MembershipErrors.AlreadyCancelled",
        "The requested membership member was already cancelled."
    );
}