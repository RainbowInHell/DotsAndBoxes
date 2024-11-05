using Microsoft.Extensions.DependencyInjection;

namespace DotsAndBoxes;

public class NavigationService<T> : INavigationService<T> where T : class, INavigable
{
    private readonly RouteMap<T> _routeMap;

    private readonly IServiceProvider _serviceProvider;

    private IServiceScope _currentScope;

    public T CurrentNavigatedItem { get; private set; } = null!;

    public event Action<NavigationResult> OnNavigated;

    public NavigationService(IServiceProvider serviceProvider, RouteMap<T> routeMap)
    {
        _serviceProvider = serviceProvider;
        _routeMap = routeMap;
    }

    public NavigationResult Navigate(string path, DynamicDictionary parameters = null)
    {
        return NavigateInternal(path, parameters);
    }

    public Task<NavigationResult> NavigateAsync(string path, DynamicDictionary parameters = null)
    {
        return NavigateInternalAsync(path, parameters);
    }

    private NavigationResult NavigateInternal(string path, DynamicDictionary parameters = null)
    {
        var args = new NavigationArgs { Parameters = parameters };
        if (!TryGetViewModelTypeByPath(path, out var viewModelType))
        {
            return BuildUnsuccessfulResult(args);
        }

        DisposeCurrentScope();
        _currentScope = _serviceProvider.CreateScope();

        if (_currentScope.ServiceProvider.GetService(viewModelType) is not T viewModel)
        {
            return BuildUnsuccessfulResult(args);
        }

        var result = viewModel.OnNavigatedTo(args);
        if (result.IsSuccess)
        {
            CurrentNavigatedItem = viewModel;
        }

        OnNavigated?.Invoke(result);
        return result;
    }

    private async Task<NavigationResult> NavigateInternalAsync(string path, DynamicDictionary parameters = null)
    {
        var args = new NavigationArgs { Parameters = parameters };
        if (!TryGetViewModelTypeByPath(path, out var viewModelType))
        {
            return BuildUnsuccessfulResult(args);
        }

        DisposeCurrentScope();
        _currentScope = _serviceProvider.CreateScope();

        if (_currentScope.ServiceProvider.GetService(viewModelType) is not T viewModel)
        {
            return BuildUnsuccessfulResult(args);
        }

        var result = await viewModel.OnNavigatedToAsync(args).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            CurrentNavigatedItem = viewModel;
        }

        OnNavigated?.Invoke(result);
        return result;
    }

    private bool TryGetViewModelTypeByPath(string path, out Type viewModelType)
    {
        viewModelType = default;
        if (_routeMap[path] is null)
        {
            return false;
        }

        viewModelType = _routeMap[path];
        return true;
    }

    private NavigationResult BuildUnsuccessfulResult(NavigationArgs args, string message = "Такого пути не существует")
    {
        var result = new NavigationResult { IsSuccess = false, NavigationArgs = args, Message = message };
        OnNavigated?.Invoke(result);
        return result;
    }

    private void DisposeCurrentScope()
    {
        _currentScope?.Dispose();
        _currentScope = null;
    }
}
