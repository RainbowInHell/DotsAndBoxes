using DotsAndBoxes.Navigation;

namespace DotsAndBoxes.Navigation;

public class NavigationService<T> : INavigationService<T> where T : class, INavigable
{
    private readonly Stack<KeyValuePair<string, T>> _routeHistory;

    private readonly Dictionary<NavigationMethod, Action<Stack<KeyValuePair<string, T>>>>
        _navigationMethodCallbacks = new()
        {
            { NavigationMethod.Pop, x => x.Pop() },
            { NavigationMethod.Clear, x => x.Clear() },
        };

    public T CurrentNavigatedItem { get; private set; } = null!;

    public bool CanGoBack => _routeHistory.Count != 1;

    private readonly IServiceProvider _serviceProvider;
    private readonly RouteMap<T> _routeMap;

    public NavigationService(IServiceProvider serviceProvider, RouteMap<T> routeMap)
    {
        _routeHistory = new Stack<KeyValuePair<string, T>>();
        _serviceProvider = serviceProvider;
        _routeMap = routeMap;
    }

    public NavigationResult Navigate(string path, DynamicDictionary? parameters = null)
    {
        return NavigateInternal(path, NavigationMethod.Default, parameters);
    }

    public NavigationResult PopAndNavigate(string path, DynamicDictionary? parameters = null)
    {
        return NavigateInternal(path, NavigationMethod.Pop, parameters);
    }

    public NavigationResult ClearAndNavigate(string path, DynamicDictionary? parameters = null)
    {
        return NavigateInternal(path, NavigationMethod.Clear, parameters);
    }

    public void GoBack()
    {
        if (!CanGoBack) return;
        _routeHistory.Pop();
        var viewModel = _routeHistory.Peek();
        var args = new NavigationArgs { Destination = viewModel.Key, NavigationMode = NavigationMode.Back };

        var result = viewModel.Value.OnNavigatedTo(args);
        CurrentNavigatedItem = viewModel.Value;

        OnNavigated?.Invoke(result);
    }

    private NavigationResult NavigateInternal(string path, NavigationMethod method = NavigationMethod.Default,
        DynamicDictionary? parameters = null)
    {
        var args = new NavigationArgs { Destination = path, Parameters = parameters ?? new DynamicDictionary() };
        if (!PathExists(path))
            return BuildUnsuccessfulResult(args);

        var viewModelType = _routeMap[path]!;
        if (_serviceProvider.GetService(viewModelType) is not T viewModel)
            return BuildUnsuccessfulResult(args);

        var result = viewModel.OnNavigatedTo(args);

        if (result.IsSuccess)
        {
            _navigationMethodCallbacks.TryGetValue(method, out var value);
            value?.Invoke(_routeHistory);
            _routeHistory.Push(new KeyValuePair<string, T>(path, viewModel));
            CurrentNavigatedItem = viewModel;
        }

        OnNavigated?.Invoke(result);

        return result;
    }

    private bool PathExists(string path) => _routeMap[path] is not null;

    private NavigationResult BuildUnsuccessfulResult(NavigationArgs args, string message = "Такого пути не существует")
    {
        var result= new NavigationResult { IsSuccess = false, NavigationArgs = args, Message = message };
        OnNavigated?.Invoke(result);
        return result;
    }

    public event Action<NavigationResult>? OnNavigated;
}