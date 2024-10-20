﻿using DotsAndBoxesUIComponents;

namespace DotsAndBoxes;

public class GameState
{
    public int GridSize { get; set; }

    public string Player1 { get; set; }
    public string Player2 { get; set; }
    public int TurnId { get; set; }
    public int[] Scores { get; set; }
    public bool IsEnded { get; set; }
    public int Length { get; set; }

    public List<RectangleStructure> PlacedRectangles { get; set; }
    public List<LineStructure> LineList { get; set; }

    public GameState()
    {
        Scores = new int[2];
        PlacedRectangles = new List<RectangleStructure>();
        LineList = new List<LineStructure>();
    }
}