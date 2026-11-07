using System;
using SlidingBlock.Core;
using UnityEngine;

namespace SlidingBlock.Data
{
    [Serializable]
    public struct BlockData
    {
        // Field names for editor code that edits levels through SerializedObject.
        // nameof keeps them in sync if a field is renamed (and won't compile if it is removed).
        public const string PositionFieldName = nameof(_position);
        public const string LengthFieldName = nameof(_length);
        public const string AxisFieldName = nameof(_axis);
        public const string IsTargetFieldName = nameof(_isTarget);

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
