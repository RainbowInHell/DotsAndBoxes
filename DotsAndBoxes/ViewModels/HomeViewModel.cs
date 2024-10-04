using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;

namespace DotsAndBoxes.ViewModels;

[Route(Routes.Home)]
public class HomeViewModel : BaseViewModel
{
    public override string ViewModelTitle { get; } = "Home";
}