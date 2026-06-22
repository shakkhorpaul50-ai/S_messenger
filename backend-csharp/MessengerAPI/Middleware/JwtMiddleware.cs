using MessengerAPI.Helpers;
using System.Security.Claims;

namespace MessengerAPI.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, JwtHelper jwtHelper)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..].Trim();

            try
            {
                var principal = jwtHelper.ValidateToken(token);

                if (principal != null)
                {
                    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var usernameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;

                    if (userIdClaim != null && int.TryParse(userIdClaim, out var userId))
                        context.Items["UserId"] = userId;

                    if (usernameClaim != null)
                        context.Items["Username"] = usernameClaim;
                }
            }
            catch
            {
            }
        }

        await _next(context);
    }
}
