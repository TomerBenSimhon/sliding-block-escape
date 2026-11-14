using System.Collections.Generic;
using SlidingBlock.Core;
using UnityEngine;

namespace SlidingBlock.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Sliding Block/Level Data")]
    public sealed class LevelData : ScriptableObject
    {
        // Field names for editor code that edits levels through SerializedObject (see BlockData).
        public const string BlockDatasFieldName = nameof(_blockDatas);
        public const string ParMovesFieldName = nameof(_parMoves);

        [SerializeField] private int _width = 6;
        [SerializeField] private int _height = 6;
        [SerializeField] private List<BlockData> _blockDatas = new();
        [SerializeField] private int _parMoves;   // fewest moves to solve, set by the editor's Validate; 0 = not validated

        public int Width => _width;
        public int Height => _height;
        public IReadOnlyList<BlockData> BlockDatas => _blockDatas;
        public int ParMoves => _parMoves;
        public bool HasParMoves => _parMoves > 0;

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
