namespace SlidingBlock.Services
{
    // Saves and loads which level the player is on, so the game continues where they left off.
    // The scope and the presenters only see this interface, so a test or a different save service
    // can replace the PlayerPrefs version without changing them.
    // Doesn't know the level pack: the caller checks that a loaded index is still in range.
    public interface ILevelProgress
    {
        // Returns the first level's index (0) when nothing has been saved yet.
        int LoadCurrentLevelIndex();

        void SaveCurrentLevelIndex(int index);
    }
}
