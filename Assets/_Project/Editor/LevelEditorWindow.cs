using System;
using SlidingBlock.Core;
using SlidingBlock.Data;
using UnityEditor;
using UnityEngine;

namespace SlidingBlock.Editor
{
    // Editor-only window to view and edit a LevelData: select, drag, edit, add and delete blocks.
    public sealed class LevelEditorWindow : EditorWindow
    {
        private const float CellSize = 48f;
        private const float Gap = 2f;
        private const int NoSelection = -1;

        private const string BlockDatasField = "_blockDatas";
        private const string PositionField = "_position";
        private const string LengthField = "_length";
        private const string AxisField = "_axis";
        private const string IsTargetField = "_isTarget";

        private static readonly Color CellColor = new Color(0.2f, 0.2f, 0.2f);
        private static readonly Color BlockColor = new Color(0.25f, 0.5f, 0.85f);
        private static readonly Color TargetColor = new Color(0.85f, 0.25f, 0.25f);
        private static readonly Color SelectedColor = new Color(0.95f, 0.8f, 0.2f);

        [SerializeField] private LevelData _levelData;
        [SerializeField] private int _selectedIndex = NoSelection;

        private SerializedObject _serializedLevelData;
        private GUIStyle _centeredLabel;

        // Drag state. Not serialized: a drag can't survive a recompile anyway.
        private Vector2Int _dragStartCell;       // the cell under the mouse when the drag started
        private Vector2Int _dragStartPosition;   // the block's anchor when the drag started
        private int _dragUndoGroup;              // merged into one undo step on release

        [MenuItem("Sliding Block/Level Editor")]
        private static void Open()
        {
            GetWindow<LevelEditorWindow>("Level Editor");
        }

        // The window can't know when the asset is edited in the Inspector, so redraw about 10 times a second.
        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            _centeredLabel ??= new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };

            _levelData = (LevelData)EditorGUILayout.ObjectField("Level", _levelData, typeof(LevelData), false);

            if (_levelData == null)
            {
                EditorGUILayout.HelpBox("Pick a level to edit.", MessageType.Info);
                return;
            }

            if (_serializedLevelData == null || _serializedLevelData.targetObject != _levelData)
                _serializedLevelData = new SerializedObject(_levelData);

            _serializedLevelData.Update();
            SerializedProperty blockDatas = FindProperty(_serializedLevelData, BlockDatasField);

            if (_selectedIndex >= blockDatas.arraySize)
                _selectedIndex = NoSelection;

            int selected = _selectedIndex;

            EditorGUILayout.LabelField($"{_levelData.Width} × {_levelData.Height}, {_levelData.BlockDatas.Count} blocks");
            EditorGUILayout.Space();

            Rect area = GUILayoutUtility.GetRect(_levelData.Width * CellSize, _levelData.Height * CellSize);

            DrawCells(area);
            DrawBlocks(area, selected);
            HandleMouse(area, blockDatas);
            DrawAddButton(blockDatas);

            if (selected != NoSelection)
                DrawSelectedPanel(blockDatas, selected);

            _serializedLevelData.ApplyModifiedProperties();
        }

        private void DrawCells(Rect area)
        {
            for (int y = 0; y < _levelData.Height; y++)
            {
                for (int x = 0; x < _levelData.Width; x++)
                {
                    Rect cell = CellRect(area, x, y);
                    EditorGUI.DrawRect(Shrink(cell, Gap), CellColor);
                }
            }
        }

        private void DrawBlocks(Rect area, int selected)
        {
            for (int i = 0; i < _levelData.BlockDatas.Count; i++)
            {
                BlockData blockData = _levelData.BlockDatas[i];
                Rect rect = BlockRect(area, blockData);
                Color color = i == selected ? SelectedColor :
                    blockData.IsTarget ? TargetColor : BlockColor;

                EditorGUI.DrawRect(Shrink(rect, Gap * 2), color);
                GUI.Label(rect, i.ToString(), _centeredLabel);
            }
        }

        // Left-click selects the block under the mouse (empty grid space deselects); dragging a block moves it.
        private void HandleMouse(Rect area, SerializedProperty blockDatas)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            Event e = Event.current;

            switch (e.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    HandleMouseDown(area, e, controlId);
                    break;
                case EventType.MouseDrag:
                    HandleMouseDrag(area, e, controlId, blockDatas);
                    break;
                case EventType.MouseUp:
                    HandleMouseUp(e, controlId);
                    break;
            }
        }

        private void HandleMouseDown(Rect area, Event e, int controlId)
        {
            if (e.button != 0 || !area.Contains(e.mousePosition))
                return;

            _selectedIndex = NoSelection;

            for (int i = _levelData.BlockDatas.Count - 1; i >= 0; i--)
            {
                if (BlockRect(area, _levelData.BlockDatas[i]).Contains(e.mousePosition))
                {
                    _selectedIndex = i;
                    break;
                }
            }

            if (_selectedIndex != NoSelection)
            {
                _dragStartCell = MouseToCell(area, e.mousePosition);
                _dragStartPosition = _levelData.BlockDatas[_selectedIndex].Position;
                _dragUndoGroup = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("Move Block");
                GUIUtility.hotControl = controlId;
            }

            e.Use();
            Repaint();
        }

        private void HandleMouseDrag(Rect area, Event e, int controlId, SerializedProperty blockDatas)
        {
            if (GUIUtility.hotControl != controlId) // not our drag → ignore
                return;

            BlockData blockData = _levelData.BlockDatas[_selectedIndex];
            Vector2Int delta = MouseToCell(area, e.mousePosition) - _dragStartCell;
            Vector2Int newPosition = ClampInside(blockData, _dragStartPosition + delta);

            SerializedProperty position = FindRelative(blockDatas.GetArrayElementAtIndex(_selectedIndex), PositionField);

            if (position.vector2IntValue != newPosition) // only write real changes
                position.vector2IntValue = newPosition;

            e.Use();
            Repaint();
        }

        private void HandleMouseUp(Event e, int controlId)
        {
            if (GUIUtility.hotControl != controlId)
                return;

            GUIUtility.hotControl = 0;
            Undo.CollapseUndoOperations(_dragUndoGroup);

            e.Use();
        }

        private void DrawAddButton(SerializedProperty blockDatas)
        {
            EditorGUILayout.Space();

            if (!GUILayout.Button("Add block"))
                return;

            int index = blockDatas.arraySize;
            blockDatas.arraySize++;

            SerializedProperty blockData = blockDatas.GetArrayElementAtIndex(index);
            FindRelative(blockData, PositionField).vector2IntValue = Vector2Int.zero;
            FindRelative(blockData, LengthField).intValue = 2;
            FindRelative(blockData, AxisField).enumValueIndex = (int)Axis.Horizontal;
            FindRelative(blockData, IsTargetField).boolValue = false;

            _selectedIndex = index;
        }

        private void DrawSelectedPanel(SerializedProperty blockDatas, int selected)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Block {selected}", EditorStyles.boldLabel);

            SerializedProperty blockData = blockDatas.GetArrayElementAtIndex(selected);

            EditorGUILayout.PropertyField(FindRelative(blockData, PositionField));
            EditorGUILayout.PropertyField(FindRelative(blockData, LengthField));
            EditorGUILayout.PropertyField(FindRelative(blockData, AxisField));
            EditorGUILayout.PropertyField(FindRelative(blockData, IsTargetField));

            EditorGUILayout.Space();

            if (GUILayout.Button("Delete block"))
            {
                blockDatas.DeleteArrayElementAtIndex(selected);
                _selectedIndex = NoSelection;
            }
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

        // The reverse of CellRect: GUI point → grid cell (may be outside the grid).
        private Vector2Int MouseToCell(Rect area, Vector2 mouse)
        {
            Vector2 local = mouse - area.position;
            int x = Mathf.FloorToInt(local.x / CellSize);
            int screenRow = Mathf.FloorToInt(local.y / CellSize);

            return new Vector2Int(x, _levelData.Height - 1 - screenRow);
        }

        // Clamps an anchor position so every cell of the block stays inside the grid.
        private Vector2Int ClampInside(BlockData blockData, Vector2Int position)
        {
            bool isHorizontal = blockData.Axis == Axis.Horizontal;
            int maxX = _levelData.Width - (isHorizontal ? blockData.Length : 1);
            int maxY = _levelData.Height - (isHorizontal ? 1 : blockData.Length);

            return new Vector2Int(Mathf.Clamp(position.x, 0, maxX), Mathf.Clamp(position.y, 0, maxY));
        }

        // Grid y goes up, GUI y goes down, so the row on screen is flipped.
        private Rect CellRect(Rect area, int x, int y)
        {
            int screenRow = _levelData.Height - 1 - y;

            return new Rect(area.x + x * CellSize, area.y + screenRow * CellSize, CellSize, CellSize);
        }

        // One rect covering all the block's cells. Its top-left corner is the cell that is
        // left-most (x of the anchor) and top-most (y of the far end, for vertical blocks).
        private Rect BlockRect(Rect area, BlockData blockData)
        {
            bool isHorizontal = blockData.Axis == Axis.Horizontal;
            int topY = isHorizontal ? blockData.Position.y : blockData.Position.y + blockData.Length - 1;
            Rect topLeftCell = CellRect(area, blockData.Position.x, topY);

            float width = isHorizontal ? blockData.Length * CellSize : CellSize;
            float height = isHorizontal ? CellSize : blockData.Length * CellSize;

            return new Rect(topLeftCell.x, topLeftCell.y, width, height);
        }

        private static Rect Shrink(Rect rect, float amount)
        {
            return new Rect(rect.x + amount, rect.y + amount, rect.width - 2 * amount, rect.height - 2 * amount);
        }
    }
}
