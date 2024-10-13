using CommunityToolkit.Mvvm.ComponentModel;
using DotsAndBoxes.Navigation;

namespace DotsAndBoxes.ViewModels;

public class MainViewModel : ObservableObject, IDisposable
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
            // CurrentViewModel.WriteToSnackBar(result.Message ?? "Произошла ошибка при навигации");
            return;
        }

        OnPropertyChanged(nameof(CurrentViewModel));
        // GoBackCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        _navigationService.OnNavigated -= OnNavigated;
    }
}