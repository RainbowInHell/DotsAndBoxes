using CommunityToolkit.Mvvm.ComponentModel;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace DotsAndBoxes.ViewModels;

[ViewModelLifetime(ServiceLifetime.Transient)]
public abstract partial class BaseViewModel : ObservableObject, INavigable, IDisposable
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

    public virtual void Dispose()
    {
        // TODO release managed resources here
    }
}