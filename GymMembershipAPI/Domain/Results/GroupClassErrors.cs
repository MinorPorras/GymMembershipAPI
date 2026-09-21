namespace GymMembershipAPI.Domain.Results;

public class GroupClassErrors
{
    public static readonly Error NotFound = new(
        "GroupClassErrors.NotFound",
        "The group class was not found."
    );
    public static readonly Error NameAlreadyExists = new(
        "GroupClassErrors.NameAlredyExists",
        "The class name already exists."
    );
    
    public static readonly Error InvalidDate = new(
        "GroupClassErrors.InvalidDate",
        "The selected date must be today or in the future."
    );
}