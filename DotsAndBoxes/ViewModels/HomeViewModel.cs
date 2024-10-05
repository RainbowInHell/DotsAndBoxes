using System.Collections.ObjectModel;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using MaterialDesignThemes.Wpf;

namespace DotsAndBoxes.ViewModels;

public class GameTypeSelectableItem
{
    public PackIconKind Icon { get; init; }

    public required string Name { get; init; }
}

[Route(Routes.Home)]
public class HomeViewModel : BaseViewModel
{
    #region Fields

    private GameTypeSelectableItem _selectedGameTypeItem;

    private string _firstUserName;

    private string _secondUserName;

    private bool _isAvailableOnLan;

    #endregion

    public HomeViewModel()
    {
        InitializeSelectableGameTypes();
    }

    #region Properties

    public override string ViewModelTitle => "Домашняя";

    public PackIconKind SelectedGameType => SelectedGameTypeItem.Icon;

    public ObservableCollection<GameTypeSelectableItem> GameTypes { get; private set; }

    public GameTypeSelectableItem SelectedGameTypeItem
    {
        get => _selectedGameTypeItem;
        set
        {
            if (_selectedGameTypeItem == value)
            {
                return;
            }

            _selectedGameTypeItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedGameType));
        }
    }

    public string FirstUserName
    {
        get => _firstUserName;
        set
        {
            if (_firstUserName == value)
            {
                return;
            }

            _firstUserName = value;
            OnPropertyChanged();
        }
    }

    public string SecondUserName
    {
        get => _secondUserName;
        set
        {
            if (_secondUserName == value)
            {
                return;
            }

            _secondUserName = value;
            OnPropertyChanged();
        }
    }

    public bool IsAvailableOnLan
    {
        get => _isAvailableOnLan;
        set
        {
            if (_isAvailableOnLan == value)
            {
                return;
            }

            _isAvailableOnLan = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    private void InitializeSelectableGameTypes()
    {
        GameTypes =
        [
            new GameTypeSelectableItem
            {
                Icon = PackIconKind.Account,
                Name = "Одиночный"
            },

            new GameTypeSelectableItem
            {
                Icon = PackIconKind.PersonMultiple,
                Name = "Вдвоем"
            },

            new GameTypeSelectableItem
            {
                Icon = PackIconKind.LanConnect,
                Name = "По сети"
            }
        ];

        SelectedGameTypeItem = GameTypes[0];
    }

    #endregion
}