using System;
using NUnit.Framework;
using static SlidingBlock.Core.Tests.TestBoards;

namespace SlidingBlock.Core.Tests
{
    public class BoardStateCodecTests
    {
        // --- Encode ---

        [Test]
        public void Encode_ExampleBoard_PacksMovingCoordinates()
        {
            // 6x6 → 3 bits per block. Moving coordinates: block 0 x=0, block 1 y=1, block 2 x=3.
            // Slots (block 2 | block 1 | block 0): 011 | 001 | 000 = 0b011001000 = 200.
            var board = ExampleBoard();
            var codec = new BoardStateCodec(board);

            ulong key = codec.Encode(board);

            Assert.AreEqual(0b011_001_000UL, key);
        }

        [Test]
        public void Encode_AfterMove_ChangesKey()
        {
            // Block 1 moves up 2: y=1 → y=3, so its slot becomes 011. 011 | 011 | 000 = 216.
            var board = ExampleBoard();
            var codec = new BoardStateCodec(board);

            board.ApplyMove(new Move(1, 2));
            ulong key = codec.Encode(board);

            Assert.AreEqual(0b011_011_000UL, key);
        }

        [Test]
        public void Encode_BoardOfAnotherSize_Throws()
        {
            var codec = new BoardStateCodec(ExampleBoard());
            var otherBoard = new Board(7, 6, ExampleBlocks());

            Assert.Throws<ArgumentException>(() => codec.Encode(otherBoard));
        }

        [Test]
        public void Encode_NullBoard_Throws()
        {
            var codec = new BoardStateCodec(ExampleBoard());

            Assert.Throws<ArgumentNullException>(() => codec.Encode(null));
        }

        // --- Decode ---

        [Test]
        public void Decode_ThenEncode_ReturnsSameKey()
        {
            var board = ExampleBoard();
            var codec = new BoardStateCodec(board);
            ulong key = codec.Encode(board);

            ulong roundTrip = codec.Encode(codec.Decode(key));

            Assert.AreEqual(key, roundTrip);
        }

        [Test]
        public void Decode_RestoresPositions()
        {
            // 216 = 011 | 011 | 000: block 0 x=0, block 1 y=3, block 2 x=3.
            // The fixed coordinates (y for horizontal, x for vertical) come from the starting board.
            var codec = new BoardStateCodec(ExampleBoard());

            var board = codec.Decode(0b011_011_000UL);

            Assert.AreEqual(new GridPos(0, 2), board.GetBlock(0).Position);
            Assert.AreEqual(new GridPos(2, 3), board.GetBlock(1).Position);
            Assert.AreEqual(new GridPos(3, 4), board.GetBlock(2).Position);
        }

        [Test]
        public void Decode_KeepsLengthAxisAndTarget()
        {
            var codec = new BoardStateCodec(ExampleBoard());

            var board = codec.Decode(0b011_011_000UL);
            var block = board.GetBlock(1);

            Assert.AreEqual(3, block.Length);
            Assert.AreEqual(Axis.Vertical, block.Axis);
            Assert.IsFalse(block.IsTarget);
            Assert.AreEqual(0, board.Target.Id);
        }

        [Test]
        public void Decode_HighSlots_ReadCorrectly()
        {
            // 12 blocks × 3 bits = 36 bits: block 11's slot (bits 33-35) is past the 32 bits of an int.
            // Catches a shift done on int instead of ulong (it would land in the wrong slot).
            // Blocks 0-10 fill rows 0-1; block 11 sits alone in row 3, so it can slide to x=5.
            var blocks = new Block[12];
            blocks[0] = B(0, 0, 0, 1, Axis.Horizontal, isTarget: true);
            for (int i = 1; i < 11; i++)
                blocks[i] = B(i, i % 6, i / 6, 1, Axis.Horizontal);
            blocks[11] = B(11, 0, 3, 1, Axis.Horizontal);

            var board = new Board(6, 6, blocks);
            var codec = new BoardStateCodec(board);
            board.ApplyMove(new Move(11, 5));

            var decoded = codec.Decode(codec.Encode(board));

            Assert.AreEqual(new GridPos(5, 3), decoded.GetBlock(11).Position);
            Assert.AreEqual(new GridPos(0, 0), decoded.GetBlock(0).Position);
        }

        // --- Constructor ---

        [Test]
        public void Constructor_NullBoard_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BoardStateCodec(null));
        }

        [Test]
        public void Constructor_TooManyBlocks_Throws()
        {
            // A 1000-wide board needs 10 bits per block (2^10 = 1024). 7 blocks × 10 bits = 70 > 64.
            var blocks = new Block[7];
            blocks[0] = B(0, 0, 0, 1, Axis.Horizontal, isTarget: true);
            for (int i = 1; i < 7; i++)
                blocks[i] = B(i, i, 0, 1, Axis.Horizontal);

            var board = new Board(1000, 1, blocks);

            Assert.Throws<ArgumentException>(() => new BoardStateCodec(board));
        }

        [Test]
        public void Constructor_SixBlocksOnThousandWideBoard_Fits()
        {
            // 6 blocks × 10 bits = 60 ≤ 64: the edge just below the limit must work.
            var blocks = new Block[6];
            blocks[0] = B(0, 0, 0, 1, Axis.Horizontal, isTarget: true);
            for (int i = 1; i < 6; i++)
                blocks[i] = B(i, i, 0, 1, Axis.Horizontal);

            var board = new Board(1000, 1, blocks);

            Assert.DoesNotThrow(() => new BoardStateCodec(board));
        }
    }
}
