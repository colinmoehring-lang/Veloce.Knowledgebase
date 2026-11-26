using Microsoft.AspNetCore.Mvc.Controllers;
using Veloce.Knowledgebase.Services.Auth;

namespace Veloce.Knowledgebase.Extensions;

internal sealed class AuthIpRateLimitMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AuthRateLimitStore store)
    {
        var action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (action?.ControllerTypeInfo.AsType() == typeof(AuthController))
        {
            var ip = context.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown";
            if (!await store.AcquireAsync("ip", ip, 100, context.RequestAborted))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return;
            }
        }

        await next(context);
    }
}
