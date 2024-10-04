using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace DotsAndBoxes.ViewModels;

[ViewModelLifetime(ServiceLifetime.Transient)]
public abstract class BaseViewModel : INavigable
{
    public abstract string ViewModelTitle { get; }

    public virtual NavigationResult OnNavigatedTo(NavigationArgs args)
    {
        return new NavigationResult { IsSuccess = true, NavigationArgs = args };
    }
}