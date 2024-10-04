namespace DotsAndBoxes.Navigation;

public interface INavigable
{
    public NavigationResult OnNavigatedTo(NavigationArgs args);
}