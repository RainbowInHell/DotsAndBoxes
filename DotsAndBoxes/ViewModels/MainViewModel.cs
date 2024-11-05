using CommunityToolkit.Mvvm.ComponentModel;

namespace DotsAndBoxes;

public sealed class MainViewModel : ObservableObject, IDisposable
{
    private readonly INavigationService<BaseViewModel> _navigationService;

    public MainViewModel(INavigationService<BaseViewModel> navigationService)
    {
        _navigationService = navigationService;
        _navigationService.OnNavigated += OnNavigated;

        _navigationService.Navigate(Routes.Home);
    }

    public BaseViewModel CurrentViewModel => _navigationService.CurrentNavigatedItem;

    private void OnNavigated(NavigationResult result)
    {
        if (!result.IsSuccess)
        {
            return;
        }

        OnPropertyChanged(nameof(CurrentViewModel));
    }

    public void Dispose()
    {
        _navigationService.OnNavigated -= OnNavigated;
    }
}