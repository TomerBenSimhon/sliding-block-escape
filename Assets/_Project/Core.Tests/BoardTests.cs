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

        // --- CanMove (moves worked out by hand on the example board) ---

        [TestCase(1, 2)]   // block 1 up into (2,4), (2,5)
        [TestCase(1, -1)]  // block 1 down into (2,0)
        [TestCase(2, -3)]  // block 2 left through (2,4), (1,4), (0,4)
        [TestCase(2, 1)]   // block 2 right into (5,4)
        public void CanMove_PathClear_ReturnsTrue(int blockId, int distance)
        {
            var board = ExampleBoard();

            bool result = board.CanMove(new Move(blockId, distance));

            Assert.IsTrue(result);
        }

        [TestCase(0, 1)]   // (2,2) is block 1
        [TestCase(1, 3)]   // (2,6) is past the top edge
        [TestCase(2, 2)]   // (6,4) is past the right edge
        [TestCase(0, -1)]  // (-1,2) is past the left edge
        [TestCase(1, -2)]  // (2,-1) is past the bottom edge
        public void CanMove_PathBlockedOrOffBoard_ReturnsFalse(int blockId, int distance)
        {
            var board = ExampleBoard();

            bool result = board.CanMove(new Move(blockId, distance));

            Assert.IsFalse(result);
        }

        [Test]
        public void CanMove_DestinationFreeButPathBlocked_ReturnsFalse()
        {
            // The target's destination (4,2)-(5,2) is empty, but block 1 at (2,2) is in the way.
            // Blocks slide; they can't jump over each other.
            var board = ExampleBoard();

            bool result = board.CanMove(new Move(0, 4));

            Assert.IsFalse(result);
        }

        [Test]
        public void CanMove_InvalidBlockId_Throws()
        {
            var board = ExampleBoard();

            Assert.Throws<ArgumentOutOfRangeException>(() => board.CanMove(new Move(3, 1)));
        }

        // --- ApplyMove ---

        [Test]
        public void ApplyMove_LegalMove_UpdatesBlockPosition()
        {
            var board = ExampleBoard();

            board.ApplyMove(new Move(1, 2));

            Assert.AreEqual(new GridPos(2, 3), board.GetBlock(1).Position);
        }

        [Test]
        public void ApplyMove_ForwardMove_ClearsOldCellsAndMarksNewCells()
        {
            // Block 1 moves from (2,1)-(2,3) to (2,3)-(2,5). (2,3) is in both: it must still be marked.
            var board = ExampleBoard();

            board.ApplyMove(new Move(1, 2));

            Assert.AreEqual(Board.Empty, board.BlockAt(new GridPos(2, 1)));
            Assert.AreEqual(Board.Empty, board.BlockAt(new GridPos(2, 2)));
            Assert.AreEqual(1, board.BlockAt(new GridPos(2, 3)));
            Assert.AreEqual(1, board.BlockAt(new GridPos(2, 4)));
            Assert.AreEqual(1, board.BlockAt(new GridPos(2, 5)));
        }

        [Test]
        public void ApplyMove_BackwardMove_ClearsOldCellsAndMarksNewCells()
        {
            // Block 1 moves from (2,1)-(2,3) down to (2,0)-(2,2).
            var board = ExampleBoard();

            board.ApplyMove(new Move(1, -1));

            Assert.AreEqual(1, board.BlockAt(new GridPos(2, 0)));
            Assert.AreEqual(1, board.BlockAt(new GridPos(2, 2)));
            Assert.AreEqual(Board.Empty, board.BlockAt(new GridPos(2, 3)));
        }

        [Test]
        public void ApplyMove_IllegalMove_Throws()
        {
            var board = ExampleBoard();

            Assert.Throws<InvalidOperationException>(() => board.ApplyMove(new Move(0, 1)));
        }

        [Test]
        public void ApplyMove_IllegalMove_LeavesBoardUnchanged()
        {
            var board = ExampleBoard();

            Assert.Throws<InvalidOperationException>(() => board.ApplyMove(new Move(0, 1)));

            Assert.AreEqual(new GridPos(0, 2), board.GetBlock(0).Position);
            Assert.AreEqual(0, board.BlockAt(new GridPos(1, 2)));
            Assert.AreEqual(1, board.BlockAt(new GridPos(2, 2)));
        }

        // --- IsSolved ---

        [Test]
        public void IsSolved_StartingPosition_ReturnsFalse()
        {
            var board = ExampleBoard();

            Assert.IsFalse(board.IsSolved);
        }

        [Test]
        public void IsSolved_AfterTwoMoveSolution_ReturnsTrue()
        {
            var board = ExampleBoard();

            board.ApplyMove(new Move(1, 2));   // clear the target's row
            board.ApplyMove(new Move(0, 4));   // slide the target to the exit

            Assert.IsTrue(board.IsSolved);
        }

        [Test]
        public void IsSolved_TargetAtExit_ReturnsTrue()
        {
            var board = new Board(6, 6, new[] { B(0, 4, 2, 2, Axis.Horizontal, isTarget: true) });

            Assert.IsTrue(board.IsSolved);
        }

        [Test]
        public void IsSolved_TargetOneCellFromExit_ReturnsFalse()
        {
            // Far end at (4,2), one short of the last column: the off-by-one case.
            var board = new Board(6, 6, new[] { B(0, 3, 2, 2, Axis.Horizontal, isTarget: true) });

            Assert.IsFalse(board.IsSolved);
        }

        [Test]
        public void IsSolved_VerticalTargetAtTopEdge_ReturnsTrue()
        {
            var board = new Board(6, 6, new[] { B(0, 1, 4, 2, Axis.Vertical, isTarget: true) });

            Assert.IsTrue(board.IsSolved);
        }
    }
}
