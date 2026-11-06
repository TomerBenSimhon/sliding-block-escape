using System;
using SlidingBlock.Core;
using UnityEngine;

namespace SlidingBlock.Data
{
    [Serializable]
    public struct BlockData
    {
        [SerializeField] private Vector2Int _position;
        [SerializeField] private int _length;
        [SerializeField] private Axis _axis;
        [SerializeField] private bool _isTarget;

        public Vector2Int Position => _position;
        public int Length => _length;
        public Axis Axis => _axis;
        public bool IsTarget => _isTarget;

        public BlockData(Vector2Int position, int length, Axis axis, bool isTarget)
        {
            _position = position;
            _length = length;
            _axis = axis;
            _isTarget = isTarget;
        }

        public Block ToBlock(int id)
        {
            GridPos pos = new GridPos(_position.x, _position.y);
            return new Block(id, pos, _length, _axis, _isTarget);
        }
    }
}
