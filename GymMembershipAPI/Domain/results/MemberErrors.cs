namespace GymMembershipAPI.Domain.results;

public class MemberErrors
{
    public static readonly Error NotFound = new(
        "MemberErrors.NotFound",
        "The requested member was not found."
    );
    
    public static readonly Error EmptyOrNullName = new(
        "MemberErrors.EmptyOrNullEmail",
        "The name must not be empty."
    );

    public static readonly Error EmailAlreadyExists = new(
        "MemberErrors.AlreadyExists",
        "A Member with this email direction already exists."
    );

    public static readonly Error InvalidEmailFormat = new(
        "MemberErrors.InvalidEmailFormat",
        "The email address format is not valid."
    );
    
    public static readonly Error EmptyOrNullEmail = new(
        "MemberErrors.EmptyOrNullEmail",
        "The email must not be empty."
    );
    
    public static readonly Error MembershipTypeNotFound = new(
        "MemberErrors.MembershipTypeNotFound",
        "The specified membership type does not exists."
    );
}