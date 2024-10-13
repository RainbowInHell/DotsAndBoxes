namespace DotsAndBoxesServerAPI.Models;

public class Player
{
    public string Name { get; set; }

    public PlayerStatus Status { get; set; }

    public SettingsHolder Settings { get; set; }
}