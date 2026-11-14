using System;
using SlidingBlock.Core;
using SlidingBlock.Data;
using UnityEditor;
using UnityEngine;

namespace SlidingBlock.Editor
{
    // The only place that writes to a LevelData asset. Edits go through SerializedObject,
    // so Undo and saving work and LevelData stays read-only for game code.
    // Call Update() before reading or editing in a GUI pass and Apply() at the end of it.
    public sealed class LevelDataEditing
    {
        private const int NewBlockLength = 2;

        private readonly SerializedObject _serializedLevelData;
        private readonly SerializedProperty _blockDatas;

        public LevelData LevelData { get; }
        public int BlockCount => _blockDatas.arraySize;

        public LevelDataEditing(LevelData levelData)
        {
            // Not "?? throw": Unity objects override == null (a deleted asset counts as null), and ?? ignores that.
            if (levelData == null)
                throw new ArgumentNullException(nameof(levelData));

            LevelData = levelData;
            _serializedLevelData = new SerializedObject(levelData);
            _blockDatas = FindProperty(_serializedLevelData, LevelData.BlockDatasFieldName);
        }

        // Pulls the asset's current values in (they may have changed in the Inspector or by Undo).
        public void Update()
        {
            _serializedLevelData.Update();
        }

        // Writes all edits of this pass into the asset, records them for Undo and marks the asset dirty.
        // Returns true if anything changed.
        public bool Apply()
        {
            return _serializedLevelData.ApplyModifiedProperties();
        }

        // The block as the serialized data holds it right now, including edits made earlier in this GUI pass
        // (the LevelData object itself only sees them after Apply()).
        public BlockData GetBlock(int index)
        {
            return new BlockData(
                BlockField(index, BlockData.PositionFieldName).vector2IntValue,
                BlockField(index, BlockData.LengthFieldName).intValue,
                (Axis)BlockField(index, BlockData.AxisFieldName).enumValueIndex,
                BlockField(index, BlockData.IsTargetFieldName).boolValue);
        }

        // Appends a 2-long horizontal block at (0, 0) and returns its index.
        public int AddBlock()
        {
            int index = _blockDatas.arraySize;
            _blockDatas.arraySize++;

            // Growing the array copies the last block into the new slot, so set every field.
            SetPosition(index, Vector2Int.zero);
            SetLength(index, NewBlockLength);
            SetAxis(index, Axis.Horizontal);
            SetIsTarget(index, false);

            return index;
        }

        public void DeleteBlock(int index)
        {
            ValidateIndex(index);
            _blockDatas.DeleteArrayElementAtIndex(index);
        }

        // The setters only write when the value really changed, so calling them every GUI pass
        // with an unchanged value doesn't create Undo entries.
        public void SetPosition(int index, Vector2Int position)
        {
            SerializedProperty property = BlockField(index, BlockData.PositionFieldName);

            if (property.vector2IntValue != position)
                property.vector2IntValue = position;
        }

        public void SetLength(int index, int length)
        {
            SerializedProperty property = BlockField(index, BlockData.LengthFieldName);

            if (property.intValue != length)
                property.intValue = length;
        }

        public void SetAxis(int index, Axis axis)
        {
            SerializedProperty property = BlockField(index, BlockData.AxisFieldName);

            if (property.enumValueIndex != (int)axis)
                property.enumValueIndex = (int)axis;
        }

        public void SetIsTarget(int index, bool isTarget)
        {
            SerializedProperty property = BlockField(index, BlockData.IsTargetFieldName);

            if (property.boolValue != isTarget)
                property.boolValue = isTarget;
        }

        // One field of one block, e.g. block 1's "_length".
        private SerializedProperty BlockField(int index, string fieldName)
        {
            ValidateIndex(index);

            return FindRelative(_blockDatas.GetArrayElementAtIndex(index), fieldName);
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= BlockCount)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be between 0 and {BlockCount - 1}");
        }

        private static SerializedProperty FindProperty(SerializedObject serializedObject, string name)
        {
            SerializedProperty property = serializedObject.FindProperty(name);

            if (property == null)
                throw new InvalidOperationException($"{serializedObject.targetObject.GetType().Name} has no serialized field '{name}'. Was it renamed?");

            return property;
        }

        private static SerializedProperty FindRelative(SerializedProperty parent, string name)
        {
            SerializedProperty property = parent.FindPropertyRelative(name);

            if (property == null)
                throw new InvalidOperationException($"{parent.propertyPath} has no serialized field '{name}'. Was it renamed?");

            return property;
        }
    }
}
