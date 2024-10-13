using DotsAndBoxesCommon.Attributes;

namespace DotsAndBoxesServerAPI.Models;

public enum GridToPlaySize
{
    [ExtendedDisplayName("3x3")]
    ThreeToThree,

    [ExtendedDisplayName("5x5")]
    FiveToFive,

    [ExtendedDisplayName("6x6")]
    SixToSix,

    [ExtendedDisplayName("7x7")]
    SevenToSeven
}