using System;
using UnityEngine;

namespace SlidingBlock.Services
{
    // Saves the current level index with PlayerPrefs (the registry in the Editor, an XML file on Android).
    public sealed class PlayerPrefsLevelProgress : ILevelProgress
    {
        private const string CurrentLevelIndexKey = "CurrentLevelIndex";
        private const int FirstLevelIndex = 0;

        public int LoadCurrentLevelIndex()
        {
            return PlayerPrefs.GetInt(CurrentLevelIndexKey, FirstLevelIndex);
        }

        public void SaveCurrentLevelIndex(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Level index can't be negative");

            PlayerPrefs.SetInt(CurrentLevelIndexKey, index);
            PlayerPrefs.Save(); // write to disk now: a phone may kill the app without a clean quit
        }
    }
}
