namespace DotsAndBoxes.Navigation;

public interface INavigable
{
    bool DisposeOnNavigate { get; }

    public NavigationResult OnNavigatedTo(NavigationArgs args);

    public Task<NavigationResult> OnNavigatedToAsync(NavigationArgs args);
}