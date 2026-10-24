using System;
using NUnit.Framework;

namespace SlidingBlock.Core.Tests
{
    public class BoardTests
    {
        // Short way to build a block: B(id, x, y, length, axis, isTarget)
        private static Block B(int id, int x, int y, int length, Axis axis, bool isTarget = false)
        {
            return new Block(id, new GridPos(x, y), length, axis, isTarget);
        }

        // The example board from the diagram
        private static Block[] ExampleBlocks()
        {
            return new[]
            {
                B(0, 0, 2, 2, Axis.Horizontal, isTarget: true),
                B(1, 2, 1, 3, Axis.Vertical),
                B(2, 3, 4, 2, Axis.Horizontal),
            };
        }

        private static Board ExampleBoard() => new Board(6, 6, ExampleBlocks());

        // --- Construction ---

        [Test]
        public void Constructor_ValidBoard_StoresSize()
        {
            var board = new Board(6, 5, ExampleBlocks());

            Assert.AreEqual(6, board.Width);
            Assert.AreEqual(5, board.Height);
        }

        [Test]
        public void Target_ReturnsTargetBlock()
        {
            var board = ExampleBoard();

            var target = board.Target;

            Assert.AreEqual(0, target.Id);
            Assert.IsTrue(target.IsTarget);
        }

        [Test]
        public void Constructor_CopiesBlocksArray()
        {
            var blocks = ExampleBlocks();
            var board = new Board(6, 6, blocks);

            blocks[1] = B(1, 5, 0, 3, Axis.Vertical);

            Assert.AreEqual(new GridPos(2, 1), board.GetBlock(1).Position);
        }

        [Test]
        public void Constructor_WidthZero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Board(0, 6, ExampleBlocks()));
        }

        [Test]
        public void Constructor_HeightZero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Board(6, 0, ExampleBlocks()));
        }

        [Test]
        public void Constructor_NullBlocks_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Board(6, 6, null));
        }

        [Test]
        public void Constructor_IdNotMatchingIndex_Throws()
        {
            var blocks = new[] { B(1, 0, 2, 2, Axis.Horizontal, isTarget: true) };

            Assert.Throws<ArgumentException>(() => new Board(6, 6, blocks));
        }

        [Test]
        public void Constructor_BlockOutsideBoard_Throws()
        {
            // Anchor (5, 2) is inside, but the second cell (6, 2) is past the right edge.
            var blocks = new[] { B(0, 5, 2, 2, Axis.Horizontal, isTarget: true) };

            Assert.Throws<ArgumentException>(() => new Board(6, 6, blocks));
        }

        [Test]
        public void Constructor_OverlappingBlocks_Throws()
        {
            // Both blocks cover (1, 2).
            var blocks = new[]
            {
                B(0, 0, 2, 2, Axis.Horizontal, isTarget: true),
                B(1, 1, 1, 3, Axis.Vertical),
            };

            Assert.Throws<ArgumentException>(() => new Board(6, 6, blocks));
        }

        [Test]
        public void Constructor_NoTarget_Throws()
        {
            var blocks = new[] { B(0, 0, 2, 2, Axis.Horizontal) };

            Assert.Throws<ArgumentException>(() => new Board(6, 6, blocks));
        }

        [Test]
        public void Constructor_TwoTargets_Throws()
        {
            var blocks = new[]
            {
                B(0, 0, 2, 2, Axis.Horizontal, isTarget: true),
                B(1, 0, 4, 2, Axis.Horizontal, isTarget: true),
            };

            Assert.Throws<ArgumentException>(() => new Board(6, 6, blocks));
        }

        // --- BlockAt ---

        [Test]
        public void BlockAt_CellsOfBlocks_ReturnBlockIds()
        {
            // Non-anchor cells on purpose: a bug that only marks each block's first cell would pass on anchors.
            var board = ExampleBoard();

            Assert.AreEqual(0, board.BlockAt(new GridPos(1, 2)));
            Assert.AreEqual(1, board.BlockAt(new GridPos(2, 3)));
            Assert.AreEqual(2, board.BlockAt(new GridPos(4, 4)));
        }

        [Test]
        public void BlockAt_EmptyCell_ReturnsEmpty()
        {
            var board = ExampleBoard();

            int result = board.BlockAt(new GridPos(5, 5));

            Assert.AreEqual(Board.Empty, result);
        }

        [TestCase(-1, 0)]
        [TestCase(6, 0)] // without a bounds check, index 6 would silently read cell (0, 1)
        [TestCase(0, 6)]
        [TestCase(0, -1)]
        public void BlockAt_OutsideBoard_Throws(int x, int y)
        {
            var board = ExampleBoard();

            Assert.Throws<ArgumentOutOfRangeException>(() => board.BlockAt(new GridPos(x, y)));
        }

        // --- IsInside ---

        [TestCase(0, 0)]
        [TestCase(5, 0)]
        [TestCase(0, 5)]
        [TestCase(5, 5)]
        public void IsInside_CornerCells_ReturnsTrue(int x, int y)
        {
            var board = ExampleBoard();

            bool result = board.IsInside(new GridPos(x, y));

            Assert.IsTrue(result);
        }

        [TestCase(-1, 0)]
        [TestCase(6, 0)]
        [TestCase(0, 6)]
        [TestCase(0, -1)]
        public void IsInside_JustOutside_ReturnsFalse(int x, int y)
        {
            var board = ExampleBoard();

            bool result = board.IsInside(new GridPos(x, y));

            Assert.IsFalse(result);
        }

        // --- GetBlock ---

        [Test]
        public void GetBlock_ValidId_ReturnsThatBlock()
        {
            var board = ExampleBoard();

            var block = board.GetBlock(1);

            Assert.AreEqual(1, block.Id);
            Assert.AreEqual(new GridPos(2, 1), block.Position);
        }

        [TestCase(-1)]
        [TestCase(3)] // the example board has ids 0-2
        public void GetBlock_InvalidId_Throws(int id)
        {
            var board = ExampleBoard();

            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetBlock(id));
        }
    }
}
