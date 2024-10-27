using Microsoft.AspNetCore.SignalR.Client;

namespace DotsAndBoxes.SignalR;

public class CustomRetryPolicy : IRetryPolicy
{
    public event Action<long> OnRetry;

    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        switch (retryContext.PreviousRetryCount)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            {
                OnRetry?.Invoke(retryContext.PreviousRetryCount);
                return TimeSpan.FromSeconds(2);
            }
            case 5:
            {
                OnRetry?.Invoke(retryContext.PreviousRetryCount);
                break;
            }
        }

        return null;
    }
}