namespace DotsAndBoxesServerAPI;

public record struct SettingsHolder
{
    public bool DoNotDisturb { get; init; }

    public GridType GridType { get; init; }

    public GridSize GridSize { get; init; }
}