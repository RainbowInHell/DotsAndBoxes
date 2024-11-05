namespace DotsAndBoxes;

public interface INavigable
{
    public NavigationResult OnNavigatedTo(NavigationArgs args);

    public Task<NavigationResult> OnNavigatedToAsync(NavigationArgs args);
}