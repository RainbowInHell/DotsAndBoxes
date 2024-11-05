namespace DotsAndBoxes;

public class NavigationResult
{
    public required bool IsSuccess { get; init; }

    public string Message { get; init; }

    public required NavigationArgs NavigationArgs { get; init; }
}
