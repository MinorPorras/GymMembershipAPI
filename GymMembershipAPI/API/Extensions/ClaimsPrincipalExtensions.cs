using System.Security.Claims;

namespace GymMembershipAPI.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetMemberPublicIdFromToken(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("member_public_id")?.Value;
        if (string.IsNullOrEmpty(claim)) return null;
        return Guid.TryParse(claim, out var parsedGuid) ? parsedGuid : null;
    }
}