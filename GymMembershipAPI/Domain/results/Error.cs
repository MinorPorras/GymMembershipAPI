namespace GymMembershipAPI.Domain.results;

public record Error(string code, string message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}