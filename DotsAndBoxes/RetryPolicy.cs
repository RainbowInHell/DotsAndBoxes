using Microsoft.AspNetCore.SignalR.Client;

namespace DotsAndBoxes;

public class CustomRetryPolicy : IRetryPolicy
{
    public Action OnRetry { get; set; }

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
                OnRetry?.Invoke();
                return TimeSpan.FromSeconds(2);
            }
            default:
            {
                return null;
            }
        }
    }
}
