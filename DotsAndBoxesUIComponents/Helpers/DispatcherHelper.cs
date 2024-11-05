using System.Windows;

namespace DotsAndBoxesUIComponents;

public static class DispatcherHelper
{
    public static void InvokeMethodInCorrectThread(Action method)
    {
        if (method == null)
        {
            return;
        }

        if (Application.Current != null && Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
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
}