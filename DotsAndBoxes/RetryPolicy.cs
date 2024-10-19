using Microsoft.AspNetCore.SignalR.Client;

namespace DotsAndBoxes;

public class RetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return retryContext.PreviousRetryCount switch
        {
            0 => TimeSpan.FromSeconds(1),
            1 => TimeSpan.FromSeconds(2),
            2 => TimeSpan.FromSeconds(5),
            _ => TimeSpan.FromSeconds(10)
        };
    }
}