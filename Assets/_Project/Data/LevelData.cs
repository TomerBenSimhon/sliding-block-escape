using System.Collections.Generic;
using SlidingBlock.Core;
using UnityEngine;

namespace SlidingBlock.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Sliding Block/Level Data")]
    public sealed class LevelData : ScriptableObject
    {
        [SerializeField] private int _width = 6;
        [SerializeField] private int _height = 6;
        [SerializeField] private List<BlockData> _blockDatas = new();

        public int Width => _width;
        public int Height => _height;
        public IReadOnlyList<BlockData> BlockDatas => _blockDatas;

        public Board CreateBoard()
        {
            Block[] blocks = new Block[_blockDatas.Count];
            for (int i = 0; i < _blockDatas.Count; i++)
            {
                BlockData blockData = _blockDatas[i];
                blocks[i] = blockData.ToBlock(i);
            }

            return new Board(_width, _height, blocks);
        }
    }
}
