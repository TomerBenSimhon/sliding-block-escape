using System;
using System.Collections.Generic;

namespace SlidingBlock.Core
{
    // The outcome of BoardSolver.Solve: whether the board can be solved, the shortest move list,
    // and how many states the search looked at (for the editor's Validate report).
    public sealed class SolveResult
    {
        public bool IsSolvable { get; }
        public IReadOnlyList<Move> Moves { get; }   // empty when unsolvable or already solved
        public int StatesExplored { get; }

        public int MoveCount => Moves.Count;        // = par when solvable

        public SolveResult(bool isSolvable, IReadOnlyList<Move> moves, int statesExplored)
        {
            Moves = moves ?? throw new ArgumentNullException(nameof(moves));
            IsSolvable = isSolvable;
            StatesExplored = statesExplored;
        }
    }
}
