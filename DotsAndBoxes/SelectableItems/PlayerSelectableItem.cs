using CommunityToolkit.Mvvm.ComponentModel;
using DotsAndBoxesServerAPI.Models;

namespace DotsAndBoxes.SelectableItems;

public partial class PlayerSelectableItem : ObservableObject
{
    [ObservableProperty]
    private string _name;

    private PlayerStatus _status;
    public PlayerStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();

            CanBeChallenged = (_status == PlayerStatus.Challenged && WasChallenged) || _status == PlayerStatus.FreeToPlay;
        }
    }

    [ObservableProperty]
    private bool _canBeChallenged = true;

    [ObservableProperty]
    private bool _wasChallenged;

    [ObservableProperty]
    private GridToPlayType _preferredGridType;

    [ObservableProperty]
    private GridToPlaySize _preferredGridSize;

    public PlayerSelectableItem(Player player)
    {
        Name = player.Name;
        Status = player.Status;
        PreferredGridType = GridToPlayType.Default;
        PreferredGridSize = GridToPlaySize.FiveToFive;
    }
}