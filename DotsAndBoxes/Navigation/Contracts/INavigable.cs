namespace DotsAndBoxes.Navigation;

public interface INavigable
{
    public NavigationResult OnNavigatedTo(NavigationArgs args);

    public Task<NavigationResult> OnNavigatedToAsync(NavigationArgs args);
}