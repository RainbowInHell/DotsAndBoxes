namespace DotsAndBoxesServerAPI.Models;

public class Player
{
    public string ConnectionId { get; set; }

    public string Name { get; set; }

    public bool CanBeChallenged { get; set; }
}