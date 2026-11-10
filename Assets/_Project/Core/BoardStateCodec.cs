using System;

namespace SlidingBlock.Core
{
    // Packs a board state into one ulong: each block's coordinate along its axis, in a fixed number of bits.
    // The fixed parts of a level (size, lengths, axes, the other coordinate) are kept from the starting board.
    public sealed class BoardStateCodec
    {
        private readonly int _width;
        private readonly int _height;
        private readonly Block[] _startBlocks;   // the template: everything except the moving coordinate
        private readonly int _bitsPerBlock;
        private readonly ulong _mask;            // _bitsPerBlock ones, e.g. 0b111

        public BoardStateCodec(Board board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));

            _width = board.Width;
            _height = board.Height;

            _startBlocks = new Block[board.Blocks.Count];
            for (int i = 0; i < _startBlocks.Length; i++)
            {
                _startBlocks[i] = board.Blocks[i];
            }

            _bitsPerBlock = BitsNeeded(Math.Max(_width, _height));
            if (_startBlocks.Length * _bitsPerBlock > 64)
                throw new ArgumentException(
                    $"{_startBlocks.Length} blocks × {_bitsPerBlock} bits = {_startBlocks.Length * _bitsPerBlock} bits, but a key holds only 64",
                    nameof(board));

            _mask = (1UL << _bitsPerBlock) - 1;
        }

        public ulong Encode(Board board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (board.Width != _width || board.Height != _height || board.Blocks.Count != _startBlocks.Length)
                throw new ArgumentException(
                    $"Expected a {_width}x{_height} board with {_startBlocks.Length} blocks, " +
                    $"got {board.Width}x{board.Height} with {board.Blocks.Count}",
                    nameof(board));

            ulong key = 0;

            for (int i = 0; i < board.Blocks.Count; i++)
            {
                key |= (ulong)MovingCoordinate(board.Blocks[i]) << (i * _bitsPerBlock);
            }

            return key;
        }

        public Board Decode(ulong key)
        {
            var blocks = new Block[_startBlocks.Length];

            for (int i = 0; i < _startBlocks.Length; i++)
            {
                Block start = _startBlocks[i];
                int coordinate = (int)((key >> (i * _bitsPerBlock)) & _mask);

                // The other coordinate never changes, so it comes from the starting block.
                GridPos position = start.Axis switch
                {
                    Axis.Horizontal => new GridPos(coordinate, start.Position.Y),
                    Axis.Vertical => new GridPos(start.Position.X, coordinate),
                    _ => throw new ArgumentOutOfRangeException(nameof(start.Axis), start.Axis, "Invalid axis value")
                };

                blocks[i] = start.WithPosition(position);
            }

            return new Board(_width, _height, blocks);
        }

        private static int MovingCoordinate(Block block)
        {
            return block.Axis switch
            {
                Axis.Horizontal => block.Position.X,
                Axis.Vertical => block.Position.Y,
                _ => throw new ArgumentOutOfRangeException(nameof(block.Axis), block.Axis, "Invalid axis value")
            };
        }

        // The smallest number of bits that can hold every value from 0 to size - 1.
        private static int BitsNeeded(int size)
        {
            int bits = 1;

            while ((1 << bits) < size)
                bits++;

            return bits;
        }
    }
}
