namespace GymMembershipAPI.Domain.results;

public record Error(string code, string message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static Error Unknown (string message) => new("General.Unknown", message);
    public static readonly Error InternalServerError = new("General.InternalServerError", "Internal Server Error");
    public static readonly Error NotImplemented = new("General.NotImplemented", "Feature not implemented");
    public static readonly Error BadGateway = new("General.BadGateway", "Bad Gateway");
    public static readonly Error ServiceUnavailable = new("General.ServiceUnavailable", "Service Unavailable");
    public static readonly Error GatewayTimeout = new("General.GatewayTimeout", "Gateway Timeout");
    public static readonly Error NullPublicId = new("General.NullPublicId", "Public Id cannot be null or empty.");
    public static readonly Error InvalidGuid = new("General.InvalidGuid", "The id does not have a valid GUID.");
}