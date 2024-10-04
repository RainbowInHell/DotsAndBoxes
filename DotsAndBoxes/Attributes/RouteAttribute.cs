namespace DotsAndBoxes.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RouteAttribute(string route) : Attribute
{
    public string Route { get; } = route;
}