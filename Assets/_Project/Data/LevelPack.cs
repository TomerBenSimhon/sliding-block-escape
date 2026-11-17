using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlidingBlock.Data
{
    // An ordered list of levels for the game. The game can't search folders in a build (no AssetDatabase),
    // so the order lives here.
    [CreateAssetMenu(fileName = "LevelPack", menuName = "Sliding Block/Level Pack")]
    public sealed class LevelPack : ScriptableObject
    {
        [SerializeField] private List<LevelData> _levelDatas = new();

        public int Count => _levelDatas.Count;

        public LevelData GetLevelData(int index)
        {
            if (index < 0 || index >= _levelDatas.Count)
                throw new ArgumentOutOfRangeException(
                    nameof(index), index, $"Index must be between 0 and {Count - 1}, but the pack has {Count} levels");

            if (_levelDatas[index] == null)
                throw new InvalidOperationException($"LevelData at index {index} is null. LevelPack: {name}");

            return _levelDatas[index];
        }
    }
}
