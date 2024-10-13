using CommunityToolkit.Mvvm.ComponentModel;
using DotsAndBoxesServerAPI.Models;

namespace DotsAndBoxes.SelectableItems;

public partial class PlayerSelectableItem : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private PlayerStatus _status;

    [ObservableProperty]
    private GridToPlayType _preferredGridType;

    [ObservableProperty]
    private GridToPlaySize _preferredGridSize;

    [ObservableProperty]
    private bool _canBeChallenged = true;

    [ObservableProperty]
    private bool _wasChallenged;

    public PlayerSelectableItem(Player player)
    {
        Name = player.Name;
        Status = player.Status;
        PreferredGridType = GridToPlayType.Default;
        PreferredGridSize = GridToPlaySize.FiveToFive;
    }

    partial void OnStatusChanged(PlayerStatus value)
    {
        CanBeChallenged = value == PlayerStatus.Challenged && WasChallenged || value == PlayerStatus.FreeToPlay;
    }
}