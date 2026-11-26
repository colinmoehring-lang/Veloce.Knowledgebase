using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Veloce.Knowledgebase.Services.Auth;

namespace Veloce.Knowledgebase.Extensions;

internal sealed class AuthRateLimitFilter(AuthRateLimitStore store) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller is not AuthController)
        {
            await next();
            return;
        }

        context.ActionArguments.TryGetValue("input", out var input);
        var email = input switch
        {
            SignUpRequestDto dto => dto.Email,
            VerifyEmailRequestDto dto => dto.Email,
            LogInRequestDto dto => dto.Email,
            ForgotPasswordRequestDto dto => dto.Email,
            ResetPasswordRequestDto dto => dto.Email,
            _ => null
        };
        var cancellationToken = context.HttpContext.RequestAborted;
        if (email is not null && !await store.AcquireAsync("email", email.Trim().ToLowerInvariant(), 10, cancellationToken))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
            return;
        }

        await next();
    }
}
