namespace SlidingBlock.Core.Tests
{
    // Shared builders for test boards, used by BoardTests, BoardStateCodecTests and later the solver tests.
    internal static class TestBoards
    {
        // Short way to build a block: B(id, x, y, length, axis, isTarget)
        public static Block B(int id, int x, int y, int length, Axis axis, bool isTarget = false)
        {
            return new Block(id, new GridPos(x, y), length, axis, isTarget);
        }

        // The example board from the diagram
        public static Block[] ExampleBlocks()
        {
            return new[]
            {
                B(0, 0, 2, 2, Axis.Horizontal, isTarget: true),
                B(1, 2, 1, 3, Axis.Vertical),
                B(2, 3, 4, 2, Axis.Horizontal),
            };
        }

        public static Board ExampleBoard() => new Board(6, 6, ExampleBlocks());
    }
}
