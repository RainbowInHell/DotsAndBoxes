using DotsAndBoxes.Navigation;

namespace DotsAndBoxes.ViewModels;

public class MainViewModel
{
    private readonly INavigationService<BaseViewModel> _navigationService;

    public MainViewModel(INavigationService<BaseViewModel> navigationService)
    {
        _navigationService = navigationService;
        _navigationService.Navigate(Routes.Home);
    }

    public BaseViewModel CurrentViewModel => _navigationService.CurrentNavigatedItem;
}