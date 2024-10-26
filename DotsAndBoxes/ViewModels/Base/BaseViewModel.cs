using CommunityToolkit.Mvvm.ComponentModel;
using DotsAndBoxes.Navigation;

namespace DotsAndBoxes.ViewModels;

public abstract partial class BaseViewModel : ObservableObject, INavigable
{
    [ObservableProperty]
    private string _viewModelTitle;

    public virtual NavigationResult OnNavigatedTo(NavigationArgs args)
    {
        return new NavigationResult { IsSuccess = true, NavigationArgs = args };
    }

    public virtual Task<NavigationResult> OnNavigatedToAsync(NavigationArgs args)
    {
        return Task.FromResult(new NavigationResult { IsSuccess = true, NavigationArgs = args });
    }
}