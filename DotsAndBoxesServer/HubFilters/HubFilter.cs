using System.Collections;
using Microsoft.AspNetCore.SignalR;

namespace DotsAndBoxesServer.HubFilters;

public class HubFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var methodCallString = GetMethodCallDisplayString(invocationContext);

        try
        {
            Console.WriteLine($"Invoking hub method: {methodCallString}");
            return await next(invocationContext);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to invoke hub method: {methodCallString}\nException: {e}");
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

    public async Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
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
        var methodCall = $"{invocationContext.HubMethodName}({string.Join(", ", invocationContext.HubMethodArguments.Select(GetReadableString))})";
        return methodCall;
    }

    private static string? GetReadableString(object? value)
    {
        return value switch
        {
            null => "null",
            string str => $"\"{str}\"",
            IEnumerable enumerable => $"{{ {string.Join(", ", enumerable.Cast<object?>().Select(GetReadableString))} }}",
            _ => value.ToString()
        };
    }
}