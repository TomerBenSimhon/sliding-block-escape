using SlidingBlock.Core;
using SlidingBlock.Data;
using UnityEditor;
using UnityEngine;

namespace SlidingBlock.Editor
{
    // Editor-only window that shows a LevelData as a grid. Read-only for now; editing comes next.
    public sealed class LevelEditorWindow : EditorWindow
    {
        private const float CellSize = 48f;
        private const float Gap = 2f;

        private static readonly Color CellColor = new Color(0.2f, 0.2f, 0.2f);
        private static readonly Color BlockColor = new Color(0.25f, 0.5f, 0.85f);
        private static readonly Color TargetColor = new Color(0.85f, 0.25f, 0.25f);

        [SerializeField] private LevelData _levelData;

        private GUIStyle _centeredLabel;

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

            EditorGUILayout.LabelField($"{_levelData.Width} × {_levelData.Height}, {_levelData.BlockDatas.Count} blocks");
            EditorGUILayout.Space();

            Rect area = GUILayoutUtility.GetRect(_levelData.Width * CellSize, _levelData.Height * CellSize);

            DrawCells(area);
            DrawBlocks(area);
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

        private void DrawBlocks(Rect area)
        {
            for (int i = 0; i < _levelData.BlockDatas.Count; i++)
            {
                BlockData blockData = _levelData.BlockDatas[i];
                Rect rect = BlockRect(area, blockData);
                Color color = blockData.IsTarget ? TargetColor : BlockColor;

                EditorGUI.DrawRect(Shrink(rect, Gap * 2), color);
                GUI.Label(rect, i.ToString(), _centeredLabel);
            }
        }

        // Grid y goes up, GUI y goes down, so the row on screen is flipped.
        private Rect CellRect(Rect area, int x, int y)
        {
            int screenRow = _levelData.Height - 1 - y;

            return new Rect(area.x + x * CellSize, area.y + screenRow * CellSize, CellSize, CellSize);
        }

        // One rect covering all of the block's cells. Its top-left corner is the cell that is
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
