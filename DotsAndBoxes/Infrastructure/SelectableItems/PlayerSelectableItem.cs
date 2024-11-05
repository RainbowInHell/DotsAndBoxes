using CommunityToolkit.Mvvm.ComponentModel;
using DotsAndBoxesServerAPI;

namespace DotsAndBoxes;

public partial class PlayerSelectableItem : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private PlayerStatus _status;

    [ObservableProperty]
    private GridType _preferredGridType;

    [ObservableProperty]
    private GridSize _preferredGridSize;

    [ObservableProperty]
    private bool _canBeChallenged = true;

    [ObservableProperty]
    private bool _wasChallenged;

    public PlayerSelectableItem(Player player)
    {
        Name = player.Name;
        Status = player.Status;
        PreferredGridType = GridType.Default;
        PreferredGridSize = GridSize.FiveToFive;
    }
    
    partial void OnStatusChanged(PlayerStatus value)
    {
        CanBeChallenged = value == PlayerStatus.Challenged && WasChallenged || value == PlayerStatus.FreeToPlay;
    }
}