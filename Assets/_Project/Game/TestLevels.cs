using SlidingBlock.Core;

namespace SlidingBlock.Game
{
    // Temporary until LevelData (week 4)
    public static class TestLevels
    {
        public static Board CreateExampleBoard()
        {
            Block block0 = new Block(id: 0, position: new GridPos(0, 2), length: 2, axis: Axis.Horizontal, isTarget: true);
            Block block1 = new Block(id: 1, position: new GridPos(2, 1), length: 3, axis: Axis.Vertical, isTarget: false);
            Block block2 = new Block(id: 2, position: new GridPos(3, 4), length: 2, axis: Axis.Horizontal, isTarget: false);
            Block[] blocks = new[] { block0, block1, block2 };

            return new Board(width: 6, height: 6, blocks: blocks);
        }
    }
}
