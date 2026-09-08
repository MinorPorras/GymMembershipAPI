namespace GymMembershipAPI.Domain.results;

public class GroupClassErrors
{
    public static readonly Error NotFound = new(
        "GroupClassErrors.NotFound",
        "The group class was not found."
    );
    
    public static readonly Error EmptyOrNullName = new(
        "GroupClassErrors.EmptyOrNullName",
        "The name of the class must not be null or empty."
    );

    public static readonly Error EmptyOrNullInstructor = new(
        "GroupClassErrors.EmptyOrNullInstructor",
        "The Instructor's name must not be null or empty."
    );

    public static readonly Error NameAlredyExists = new(
        "GroupClassErrors.NameAlredyExists",
        "The class name already exists."
    );

    public static readonly Error DateAlredyInUse = new(
        "GroupClassErrors.DateAlredyInUse",
        "The selected date and hour are already in use."
    );
    
    public static readonly Error InvalidMaxMembers = new(
        "GroupClassErrors.InvalidMaxMembers",
        "The maximum number of members has to be greater than zero."
    );
}