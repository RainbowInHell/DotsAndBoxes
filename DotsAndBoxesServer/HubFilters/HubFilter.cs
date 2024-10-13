using System.Collections;
using Microsoft.AspNetCore.SignalR;

namespace DotsAndBoxesServer.HubFilters;

public class HubFilter : IHubFilter
{
    private readonly ILogger<HubFilter> _logger;

    public HubFilter(ILogger<HubFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
    {
        var methodCallString = GetMethodCallDisplayString(invocationContext);

        try
        {
            _logger.LogInformation("Calling hub method '{method}'", methodCallString);
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception calling '{method}': {ex}", methodCallString, ex);
            throw;
        }
    }

    public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OnConnectedAsync exception: {ex}");
            throw;
        }
    }

    public async Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
    {
        try
        {
            await next(context, exception);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OnDisconnectedAsync exception: {ex}");
            throw;
        }
    }

    private static string GetMethodCallDisplayString(HubInvocationContext invocationContext)
    {
        return $"{invocationContext.HubMethodName}({string.Join(", ", invocationContext.HubMethodArguments.Select(GetReadableString))})";
    }

    private static string GetReadableString(object value)
    {
        return value switch
        {
            null => "null",
            string str => $"\"{str}\"",
            IEnumerable enumerable => $"{{ {string.Join(", ", enumerable.Cast<object>().Select(GetReadableString))} }}",
            _ => value.ToString()
        };
    }
}