using System.ComponentModel;
using System.Runtime.CompilerServices;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace DotsAndBoxes.ViewModels;

[ViewModelLifetime(ServiceLifetime.Transient)]
public abstract class BaseViewModel : INotifyPropertyChanged, INavigable
{
    public abstract string ViewModelTitle { get; }

    public virtual NavigationResult OnNavigatedTo(NavigationArgs args)
    {
        return new NavigationResult { IsSuccess = true, NavigationArgs = args };
    }
    // [NotifyPropertyChangedInvocator]
    public virtual void OnPropertyChanged([CallerMemberName] string fieldName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(fieldName));
    
    public event PropertyChangedEventHandler PropertyChanged;
}