using System.Windows;
using System.Windows.Threading;

namespace DotsAndBoxesUIComponents;

public static class DispatcherHelper
{
    internal static bool IsDebugMode { get; set; } = false;

    public static void InvokeMethodInCorrectThread(Action method)
    {
        if (method == null)
        {
            return;
        }

        if (Application.Current != null && Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess() && !IsDebugMode)
        {
            Application.Current.Dispatcher.Invoke(method);
        }
        else
        {
            method.Invoke();
        }
    }

    public static async Task InvokeMethodInCorrectThreadAsync(Action method)
    {
        if (method == null)
        {
            return;
        }

        if (Application.Current != null && Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            await Application.Current.Dispatcher.BeginInvoke(method);
        }
        else
        {
            method.Invoke();
        }
    }

    /// <summary>
    /// Уход в поток закрепленный за диспатчером.
    /// <example>
    /// await AsyncHelper.RedirectTo(dispatcher);
    /// </example>
    /// </summary>
    public static DispatcherRedirector RedirectTo(Dispatcher d)
    {
        return new DispatcherRedirector(d);
    }

    /// <summary>
    /// Уход в главный поток.
    /// После вызова произойдет переход в главный ГУИ-поток.
    /// <example>
    /// await DispatcherHelper.RedirectToGuiThread();
    /// </example>
    /// </summary>
    public static DispatcherRedirector RedirectToGuiThread()
    {
        return RedirectTo(Application.Current?.Dispatcher);
    }
}