using GymMembershipAPI.Domain.Results;

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
}