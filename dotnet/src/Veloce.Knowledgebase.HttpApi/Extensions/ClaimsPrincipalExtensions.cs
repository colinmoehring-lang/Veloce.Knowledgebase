using System.Security.Claims;

namespace Veloce.Knowledgebase;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var rawId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? principal.FindFirstValue("id");

        return Guid.TryParse(rawId, out var id) ? id : null;
    }

    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        return principal.GetUserId()
            ?? throw new UnauthorizedAccessException("User is not authenticated or user id claim is missing.");
    }
}
