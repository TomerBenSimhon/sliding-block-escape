using System;
using System.Collections.Generic;

namespace SlidingBlock.Core
{
    public sealed class Board
    {
        public const int Empty = -1;

        private readonly Block[] _blocks;
        private readonly int[] _cells; // block id per cell, row by row from the bottom; Empty if free
        private readonly int _targetId;

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<Block> Blocks => _blocks;
        public Block Target => _blocks[_targetId];

        public Board(int width, int height, Block[] blocks)
        {
            if (width < 1) throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be at least 1");
            if (height < 1) throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be at least 1");
            if (blocks == null) throw new ArgumentNullException(nameof(blocks));

            Width = width;
            Height = height;
            _blocks = (Block[])blocks.Clone();
            _cells = new int[width * height];
            Array.Fill(_cells, Empty);
            _targetId = Empty;

            for (int i = 0; i < _blocks.Length; i++)
            {
                var block = _blocks[i];
                if (block.Id != i)
                    throw new ArgumentException($"Block at index {i} has id {block.Id}; ids must match their index");

                for (int j = 0; j < block.Length; j++)
                {
                    var cell = block.CellAt(j);
                    if (!IsInside(cell))
                        throw new ArgumentException($"Block {block.Id} is outside the {width}x{height} board at {cell}");

                    int index = IndexOf(cell);
                    if (_cells[index] != Empty)
                        throw new ArgumentException($"Block {block.Id} overlaps block {_cells[index]} at {cell}");

                    _cells[index] = block.Id;
                }

                if (!block.IsTarget) continue;
                if (_targetId != Empty)
                    throw new ArgumentException($"Blocks {_targetId} and {block.Id} are both targets; a board needs exactly one");
                _targetId = block.Id;
            }

            if (_targetId == Empty) throw new ArgumentException("A board needs exactly one target block, found none");
        }

        public bool IsInside(GridPos pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        }

        public int BlockAt(GridPos pos)
        {
            if (!IsInside(pos)) throw new ArgumentOutOfRangeException(nameof(pos), pos, "Position is outside the board");
            return _cells[IndexOf(pos)];
        }

        public Block GetBlock(int id)
        {
            if (id < 0 || id >= _blocks.Length)
                throw new ArgumentOutOfRangeException(nameof(id), id, $"Block id must be between 0 and {_blocks.Length - 1}");
            return _blocks[id];
        }

        public bool CanMove(Move move)
        {
            var block = GetBlock(move.BlockId);
            GridPos start = move.Distance > 0 ? block.CellAtFarEnd : block.Position;
            GridPos stepDir = block.Direction * Math.Sign(move.Distance);
            int steps = Math.Abs(move.Distance);

            for (int k = 1; k <= steps; k++)
            {
                var cell = start + (stepDir * k);
                if (!IsInside(cell)) return false;
                if (BlockAt(cell) != Empty) return false;
            }

            return true;
        }

        public void ApplyMove(Move move)
        {
            if (!CanMove(move))
                throw new InvalidOperationException($"Illegal move {move}: the path is blocked or leaves the board");

            var block = GetBlock(move.BlockId);
            FillCells(block, Empty);

            var moved = block.WithPosition(block.Position + block.Direction * move.Distance);
            _blocks[move.BlockId] = moved;
            FillCells(moved, move.BlockId);
        }

        // Solved when the target's far end touches the edge in its direction (the exit).
        public bool IsSolved => !IsInside(Target.CellAtFarEnd + Target.Direction);

        private int IndexOf(GridPos pos) => pos.Y * Width + pos.X;

        private void FillCells(Block block, int value)
        {
            for (int i = 0; i < block.Length; i++)
            {
                _cells[IndexOf(block.CellAt(i))] = value;
            }
        }
    }
}
