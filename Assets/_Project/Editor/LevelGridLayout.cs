using System;
using SlidingBlock.Core;
using SlidingBlock.Data;
using UnityEngine;

namespace SlidingBlock.Editor
{
    // Maps between grid cells and GUI pixels for the LevelEditorWindow, like BoardLayout does for world space.
    // Grid y goes up, GUI y goes down, so rows are flipped. Width and height aren't validated on purpose:
    // the editor must still draw a broken level so it can be fixed.
    public sealed class LevelGridLayout
    {
        public Rect Area { get; }
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }

        public LevelGridLayout(Rect area, int width, int height, float cellSize)
        {
            if (cellSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "Cell size must be greater than 0");

            Area = area;
            Width = width;
            Height = height;
            CellSize = cellSize;
        }

        public Rect CellRect(int x, int y)
        {
            int screenRow = Height - 1 - y;

            return new Rect(Area.x + x * CellSize, Area.y + screenRow * CellSize, CellSize, CellSize);
        }

        // One rect covering all the block's cells. Its top-left corner is the cell that is
        // left-most (x of the anchor) and top-most (y of the far end, for vertical blocks).
        public Rect BlockRect(BlockData blockData)
        {
            bool isHorizontal = blockData.Axis == Axis.Horizontal;
            int topY = isHorizontal ? blockData.Position.y : blockData.Position.y + blockData.Length - 1;
            Rect topLeftCell = CellRect(blockData.Position.x, topY);

            float width = isHorizontal ? blockData.Length * CellSize : CellSize;
            float height = isHorizontal ? CellSize : blockData.Length * CellSize;

            return new Rect(topLeftCell.x, topLeftCell.y, width, height);
        }

        // The reverse of CellRect: GUI point → grid cell (may be outside the grid).
        public Vector2Int MouseToCell(Vector2 mouse)
        {
            Vector2 local = mouse - Area.position;
            int x = Mathf.FloorToInt(local.x / CellSize);
            int screenRow = Mathf.FloorToInt(local.y / CellSize);

            return new Vector2Int(x, Height - 1 - screenRow);
        }

        // Clamps an anchor position so every cell of the block stays inside the grid.
        public Vector2Int ClampInside(BlockData blockData, Vector2Int position)
        {
            bool isHorizontal = blockData.Axis == Axis.Horizontal;
            int maxX = Width - (isHorizontal ? blockData.Length : 1);
            int maxY = Height - (isHorizontal ? 1 : blockData.Length);

            return new Vector2Int(Mathf.Clamp(position.x, 0, maxX), Mathf.Clamp(position.y, 0, maxY));
        }
    }
}
