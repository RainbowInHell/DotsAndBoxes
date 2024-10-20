namespace DotsAndBoxes.Navigation;

public interface INavigationService<out T> where T : class, INavigable
{
    public T CurrentNavigatedItem { get; }

    public NavigationResult Navigate(string path, DynamicDictionary parameters = null);

    public Task<NavigationResult> NavigateAsync(string path, DynamicDictionary parameters = null);

    public event Action<NavigationResult> OnNavigated;
}