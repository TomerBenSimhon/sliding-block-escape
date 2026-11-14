using System;
using System.Collections.Generic;
using NUnit.Framework;
using static SlidingBlock.Core.Tests.TestBoards;

namespace SlidingBlock.Core.Tests
{
    public class BoardSolverTests
    {
        // --- Solvable boards ---

        [Test]
        public void Solve_ExampleBoard_FindsTwoMoveSolution()
        {
            // Block 1 must go up 2 to clear the target's row, then the target slides 4 to the exit.
            var result = BoardSolver.Solve(ExampleBoard());

            Assert.IsTrue(result.IsSolvable);
            CollectionAssert.AreEqual(new List<Move> { new Move(1, 2), new Move(0, 4) }, result.Moves);
            Assert.AreEqual(2, result.MoveCount);
        }

        [Test]
        public void Solve_ExampleBoard_MovesActuallySolveIt()
        {
            // Replays the answer on a fresh board: catches a path that's in the wrong order or skips a move.
            var result = BoardSolver.Solve(ExampleBoard());
            var board = ExampleBoard();

            foreach (var move in result.Moves)
            {
                board.ApplyMove(move);
            }

            Assert.IsTrue(board.IsSolved);
        }

        [Test]
        public void Solve_NeedsBlockerToMoveTwice_FindsShortestSolution()
        {
            // 4x4 board. The vertical block V is in the target's row and can't go down (it's at the
            // bottom). To go up 2 and clear row 1, it needs (2,3) free, but the horizontal block H
            // is there, and H only frees it by sliding left 2. So: H -2, V +2, target +3.
            //   y=3  . . H H
            //   y=2  . . . .
            //   y=1  T . V .
            //   y=0  . . V .
            var board = new Board(4, 4, new[]
            {
                B(0, 0, 1, 1, Axis.Horizontal, isTarget: true),
                B(1, 2, 0, 2, Axis.Vertical),
                B(2, 2, 3, 2, Axis.Horizontal),
            });

            var result = BoardSolver.Solve(board);

            Assert.IsTrue(result.IsSolvable);
            CollectionAssert.AreEqual(new List<Move> { new Move(2, -2), new Move(1, 2), new Move(0, 3) }, result.Moves);
        }

        [Test]
        public void Solve_AlreadySolved_ReturnsZeroMoves()
        {
            // The target's far end is already on the right edge.
            var board = new Board(6, 6, new[] { B(0, 4, 2, 2, Axis.Horizontal, isTarget: true) });

            var result = BoardSolver.Solve(board);

            Assert.IsTrue(result.IsSolvable);
            Assert.AreEqual(0, result.MoveCount);
        }

        // --- Unsolvable boards ---

        [Test]
        public void Solve_Unsolvable_ReturnsNotSolvable()
        {
            // 3x1 board: a vertical block can never move in a 1-high board, so it blocks the exit forever.
            var board = new Board(3, 1, new[]
            {
                B(0, 0, 0, 1, Axis.Horizontal, isTarget: true),
                B(1, 2, 0, 1, Axis.Vertical),
            });

            var result = BoardSolver.Solve(board);

            Assert.IsFalse(result.IsSolvable);
            Assert.AreEqual(0, result.MoveCount);
        }

        [Test]
        public void Solve_Unsolvable_ExploresEveryReachableState()
        {
            // The target can only be at x=0 or x=1: exactly 2 states, and BFS must try both before giving up.
            var board = new Board(3, 1, new[]
            {
                B(0, 0, 0, 1, Axis.Horizontal, isTarget: true),
                B(1, 2, 0, 1, Axis.Vertical),
            });

            var result = BoardSolver.Solve(board);

            Assert.AreEqual(2, result.StatesExplored);
        }

        // --- Input handling ---

        [Test]
        public void Solve_DoesNotChangeInputBoard()
        {
            // The solver works on decoded copies; the board the game is showing must stay as it was.
            var board = ExampleBoard();

            BoardSolver.Solve(board);

            Assert.AreEqual(new GridPos(0, 2), board.GetBlock(0).Position);
            Assert.AreEqual(new GridPos(2, 1), board.GetBlock(1).Position);
            Assert.AreEqual(new GridPos(3, 4), board.GetBlock(2).Position);
        }

        [Test]
        public void Solve_NullBoard_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => BoardSolver.Solve(null));
        }

        // --- SolveResult ---

        [Test]
        public void SolveResult_NullMoves_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SolveResult(true, null, 1));
        }
    }
}
