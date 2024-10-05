using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace DotsAndBoxesUIComponents;

public readonly struct DispatcherRedirector : INotifyCompletion
{
    private readonly Dispatcher _dispatcher;

    public DispatcherRedirector(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    #region awaiter

    public DispatcherRedirector GetAwaiter()
    {
        // combined awaiter and awaitable
        return this;
    }

    #endregion

    #region awaitable

    public bool IsCompleted => _dispatcher != null && _dispatcher.CheckAccess();

    public void OnCompleted(Action continuation)
    {
        _dispatcher?.BeginInvoke(continuation);
    }

    public void GetResult()
    {
        // Для работы await-а
    }

    #endregion
}