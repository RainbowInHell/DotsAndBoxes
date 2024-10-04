using Microsoft.Extensions.DependencyInjection;

namespace DotsAndBoxes.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ViewModelLifetimeAttribute(ServiceLifetime lifetime) : Attribute
{
    public ServiceLifetime Lifetime { get; } = lifetime;
}