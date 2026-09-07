namespace GymMembershipAPI.Domain.results;

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
    
    public static readonly Error InvalidName = new(
        "MemberErrors.IncorrectNameFormat", 
        "The name must not be empty."
    );
    
    public static readonly Error InvalidMembershipType = new(
        "MemberErrors.InvalidMembershipType", 
        "Invalid membership type."
    );

    public static readonly Error InvalidDateRange = new(
        "MemberErrors.InvalidDateRange",
        "Start date must be before end date."
    );
}