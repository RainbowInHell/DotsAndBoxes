using System.ComponentModel;

namespace DotsAndBoxesCommon.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field)]
public class ExtendedDisplayNameAttribute(string name) : DisplayNameAttribute
{
    public override string DisplayName => name;
}