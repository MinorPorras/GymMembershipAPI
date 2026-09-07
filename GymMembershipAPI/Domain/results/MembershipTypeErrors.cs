using GymMembershipAPI.Domain.results;

namespace GymMembershipAPI.Domain.Results;

public static class MembershipTypeErrors
{
    public static readonly Error NotFound = new(
        "MembershipType.NotFound", 
        "The requested membership type was not found."
    );
    
    public static readonly Error AlreadyExists = new(
        "MembershipType.AlreadyExists", 
        "A membership type with this name already exists."
    );
    
    public static readonly Error InvalidName = new(
        "MembershipType.IncorrectNameFormat", 
        "The name must not be empty."
    );
    
    public static readonly Error InvalidPrice = new(
        "MembershipType.InvalidPrice", 
        "The price must be higher than 0."
    );
    
    public static readonly Error InvalidMonthDuration = new(
        "MembershipType.InvalidPrice", 
        "The duration in months must be higher than 0."
    );
}