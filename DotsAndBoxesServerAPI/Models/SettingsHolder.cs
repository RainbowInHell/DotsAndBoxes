namespace DotsAndBoxesServerAPI.Models;

public record struct SettingsHolder
{
    public bool DoNotDisturb { get; init; }

    public GridToPlayType GridToPlayType { get; init; }

    public GridToPlaySize GridToPlaySize { get; init; }
}