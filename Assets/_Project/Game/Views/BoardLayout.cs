using System;
using SlidingBlock.Core;
using UnityEngine;

namespace SlidingBlock.Game
{
    public sealed class BoardLayout
    {
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public Vector3 Origin { get; }

        public BoardLayout(int width, int height, float cellSize)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero");
            if (cellSize <= 0) throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "CellSize must be greater than zero");
            
            Width = width;
            Height = height;
            CellSize = cellSize;
            Origin = new Vector3(-width * cellSize / 2, -height * cellSize / 2, 0);
        }

        public Vector3 CellToWorld(GridPos cell)
        {
            float x = Origin.x + (cell.X + 0.5f) * CellSize;
            float y = Origin.y + (cell.Y + 0.5f) * CellSize;
            return new Vector3(x, y, 0);
        }

        public Vector3 BlockCenter(Block block)
        {
            Vector3 blockPos = CellToWorld(block.Position);
            Vector3 blockFarEnd = CellToWorld(block.CellAtFarEnd);
            return (blockPos + blockFarEnd) / 2;
        }

        public Vector2 BlockSize(Block block)
        {
            float length = block.Length * CellSize;
            float thickness = CellSize;
            return block.Axis == Axis.Horizontal ? new Vector2(length, thickness) : new Vector2(thickness, length);
        }

        public GridPos WorldToCell(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos.x - Origin.x) / CellSize);
            int y = Mathf.FloorToInt((worldPos.y - Origin.y) / CellSize);
            return new GridPos(x, y);
        }
    }
}
