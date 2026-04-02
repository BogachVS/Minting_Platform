using Microsoft.AspNetCore.SignalR;
using PiggyGame.Common.Exceptions;

namespace PiggyGame.Hubs.Filters;

public class HubExceptionsFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            var message = $"Exception calling '{invocationContext.HubMethodName}': {ex.Message}";
            await invocationContext.Hub.Clients.Caller.SendAsync("Exception", message);

            if (ex is not EntityWasNotFoundException &&
                ex is not ArgumentException &&
                ex is not InvalidOperationException)
            {
                throw;
            }

            return Task.CompletedTask;
        }
    }
}