using System;

namespace SlidingBlock.Game
{
    // Which level of the pack is being played. Registered in the scope so presenters can ask
    // "is this the last level?" without knowing about the pack or the save data.
    public sealed class CurrentLevel
    {
        public int Index { get; }
        public int LevelCount { get; }
        public bool IsLast => Index == LevelCount - 1;

        public CurrentLevel(int index, int levelCount)
        {
            if (levelCount < 1)
                throw new ArgumentOutOfRangeException(nameof(levelCount), levelCount, "Level count must be at least 1");

            if (index < 0 || index >= levelCount)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be between 0 and {levelCount - 1}");

            Index = index;
            LevelCount = levelCount;
        }
    }
}
