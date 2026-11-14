using System;
using System.Collections.Generic;

namespace SlidingBlock.Core
{
    // Breadth-first search over board states. BFS explores every board 1 move away, then 2, and so on,
    // so the first solved board it reaches uses the fewest moves possible (the level's par).
    public static class BoardSolver
    {
        public static SolveResult Solve(Board board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));

            var codec = new BoardStateCodec(board);
            ulong startKey = codec.Encode(board);

            if (board.IsSolved)
                return new SolveResult(true, Array.Empty<Move>(), 1);

            var queue = new Queue<ulong>();
            var cameFrom = new Dictionary<ulong, (ulong previous, Move move)>();   // also the visited set
            var moves = new List<Move>();                                          // reused for every state
            int statesExplored = 0;

            queue.Enqueue(startKey);
            cameFrom[startKey] = default;   // marks the start as visited; it has no previous state, so the value is never read

            while (queue.Count > 0)
            {
                ulong key = queue.Dequeue();
                statesExplored++;

                Board currentBoard = codec.Decode(key);
                currentBoard.GetLegalMoves(moves);

                foreach (Move move in moves)
                {
                    // Try the move, read where it leads, then undo it so the next move starts from the same board.
                    currentBoard.ApplyMove(move);
                    ulong nextKey = codec.Encode(currentBoard);
                    bool isSolved = currentBoard.IsSolved;
                    currentBoard.ApplyMove(move.Inverse);

                    if (cameFrom.ContainsKey(nextKey))
                        continue;

                    cameFrom[nextKey] = (key, move);

                    // Checked when the state is discovered, not when it's dequeued: still the shortest path,
                    // because every move costs exactly 1, and it finishes a whole layer earlier.
                    if (isSolved)
                        return new SolveResult(true, BuildPath(cameFrom, startKey, nextKey), statesExplored);

                    queue.Enqueue(nextKey);
                }
            }

            return new SolveResult(false, Array.Empty<Move>(), statesExplored);
        }

        // Walks the breadcrumbs back from the solved state to the start, then reverses them into play order.
        private static List<Move> BuildPath(Dictionary<ulong, (ulong previous, Move move)> cameFrom, ulong startKey, ulong endKey)
        {
            var path = new List<Move>();
            ulong key = endKey;

            while (key != startKey)
            {
                var step = cameFrom[key];
                path.Add(step.move);
                key = step.previous;
            }

            path.Reverse();
            return path;
        }
    }
}
