using System;
using SlidingBlock.Core;
using SlidingBlock.Data;
using UnityEditor;
using UnityEngine;

namespace SlidingBlock.Editor
{
    // Editor-only window to view and edit a LevelData: select, drag, edit, add and delete blocks,
    // and validate the level with the solver.
    // It draws and handles input; all writes to the asset go through LevelDataEditing.
    public sealed class LevelEditorWindow : EditorWindow
    {
        #region Constants

        private const float CellSize = 48f;
        private const float Gap = 2f;
        private const int NoSelection = -1;

        private static readonly Color CellColor = new Color(0.2f, 0.2f, 0.2f);
        private static readonly Color BlockColor = new Color(0.25f, 0.5f, 0.85f);
        private static readonly Color TargetColor = new Color(0.85f, 0.25f, 0.25f);
        private static readonly Color SelectedColor = new Color(0.95f, 0.8f, 0.2f);

        #endregion

        #region State

        [SerializeField] private LevelData _levelData;
        [SerializeField] private int _selectedIndex = NoSelection;

        private LevelDataEditing _editing;
        private GUIStyle _centeredLabel;

        // The last Validate result; null = nothing to show. Cleared when the level is switched or edited.
        private string _validationMessage;
        private MessageType _validationType;

        // Drag state. Not serialized: a drag can't survive a recompile anyway.
        private Vector2Int _dragStartCell;       // the cell under the mouse when the drag started
        private Vector2Int _dragStartPosition;   // the block's anchor when the drag started
        private int _dragUndoGroup;              // merged into one undo step on release

        #endregion

        #region Window

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
            DrawToolbar();

            if (_levelData == null)
            {
                EditorGUILayout.HelpBox("Pick a level to edit.", MessageType.Info);
                return;
            }

            if (_editing == null || _editing.LevelData != _levelData)
            {
                _editing = new LevelDataEditing(_levelData);
                _validationMessage = null;
            }

            _editing.Update();

            if (_selectedIndex >= _editing.BlockCount)
                _selectedIndex = NoSelection;

            int selected = _selectedIndex;

            string par = _editing.ParMoves > 0 ? $"par {_editing.ParMoves}" : "par not validated";
            EditorGUILayout.LabelField($"{_levelData.Width} × {_levelData.Height}, {_editing.BlockCount} blocks, {par}");
            EditorGUILayout.Space();

            Rect area = GUILayoutUtility.GetRect(_levelData.Width * CellSize, _levelData.Height * CellSize);
            LevelGridLayout grid = new LevelGridLayout(area, _levelData.Width, _levelData.Height, CellSize);

            DrawCells(grid);
            DrawBlocks(grid, selected);
            HandleMouse(grid);
            DrawAddButton();

            if (selected != NoSelection)
                DrawSelectedPanel(selected);

            // An edit makes the old par and report wrong: reset both. Done before Apply, so the edit
            // and the par reset land in the same Undo step.
            if (_editing.HasChanges)
            {
                _editing.SetParMoves(0);
                _validationMessage = null;
            }

            _editing.Apply();
            DrawValidation();   // after Apply: CreateBoard reads the LevelData object, which is only up to date now
        }

        #endregion

        #region Toolbar: new, duplicate, save

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("New"))
                CreateNewLevel();

            using (new EditorGUI.DisabledScope(_levelData == null))
            {
                if (GUILayout.Button("Duplicate"))
                    DuplicateLevel();
            }

            using (new EditorGUI.DisabledScope(_levelData == null || !LevelAssets.HasUnsavedChanges(_levelData)))
            {
                if (GUILayout.Button("Save"))
                    LevelAssets.Save(_levelData);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewLevel()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "New level", "Level_New", "asset", "Choose where to save the new level.", LevelAssets.LevelsFolder);

            if (!string.IsNullOrEmpty(path)) // empty = the user pressed Cancel
                OpenLevel(LevelAssets.CreateNew(path));

            GUIUtility.ExitGUI(); // always, even after Cancel: the dialog interrupted this pass
        }

        private void DuplicateLevel()
        {
            OpenLevel(LevelAssets.Duplicate(_levelData));
            GUIUtility.ExitGUI();
        }

        // Switches the window to another level. _editing is rebuilt on the next pass (OnGUI sees a different LevelData).
        private void OpenLevel(LevelData level)
        {
            _levelData = level;
            _selectedIndex = NoSelection;
            EditorGUIUtility.PingObject(level); // highlights the asset in the Project window
        }

        #endregion

        #region Grid drawing

        private void DrawCells(LevelGridLayout grid)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    EditorGUI.DrawRect(Shrink(grid.CellRect(x, y), Gap), CellColor);
                }
            }
        }

        // Block values are always read through _editing, so they include edits made earlier in this pass.
        private void DrawBlocks(LevelGridLayout grid, int selected)
        {
            for (int i = 0; i < _editing.BlockCount; i++)
            {
                BlockData blockData = _editing.GetBlock(i);
                Rect rect = grid.BlockRect(blockData);
                Color color = i == selected ? SelectedColor :
                    blockData.IsTarget ? TargetColor : BlockColor;

                EditorGUI.DrawRect(Shrink(rect, Gap * 2), color);
                GUI.Label(rect, i.ToString(), _centeredLabel);
            }
        }

        private static Rect Shrink(Rect rect, float amount)
        {
            return new Rect(rect.x + amount, rect.y + amount, rect.width - 2 * amount, rect.height - 2 * amount);
        }

        #endregion

        #region Mouse: select and drag

        // Left-click selects the block under the mouse (empty grid space deselects); dragging a block moves it.
        private void HandleMouse(LevelGridLayout grid)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            Event e = Event.current;

            switch (e.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    HandleMouseDown(grid, e, controlId);
                    break;
                case EventType.MouseDrag:
                    HandleMouseDrag(grid, e, controlId);
                    break;
                case EventType.MouseUp:
                    HandleMouseUp(e, controlId);
                    break;
            }
        }

        private void HandleMouseDown(LevelGridLayout grid, Event e, int controlId)
        {
            if (e.button != 0 || !grid.Area.Contains(e.mousePosition))
                return;

            _selectedIndex = NoSelection;

            for (int i = _editing.BlockCount - 1; i >= 0; i--)
            {
                if (grid.BlockRect(_editing.GetBlock(i)).Contains(e.mousePosition))
                {
                    _selectedIndex = i;
                    break;
                }
            }

            if (_selectedIndex != NoSelection)
            {
                _dragStartCell = grid.MouseToCell(e.mousePosition);
                _dragStartPosition = _editing.GetBlock(_selectedIndex).Position;
                _dragUndoGroup = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("Move Block");
                GUIUtility.hotControl = controlId;
            }

            e.Use();
            Repaint();
        }

        private void HandleMouseDrag(LevelGridLayout grid, Event e, int controlId)
        {
            if (GUIUtility.hotControl != controlId) // not our drag → ignore
                return;

            BlockData blockData = _editing.GetBlock(_selectedIndex);
            Vector2Int delta = grid.MouseToCell(e.mousePosition) - _dragStartCell;
            Vector2Int newPosition = grid.ClampInside(blockData, _dragStartPosition + delta);

            _editing.SetPosition(_selectedIndex, newPosition);

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

        #endregion

        #region Panel: add, edit, delete

        private void DrawAddButton()
        {
            EditorGUILayout.Space();

            if (GUILayout.Button("Add block"))
                _selectedIndex = _editing.AddBlock();
        }

        // Typed fields show the current values; the setters only write when a value changed.
        private void DrawSelectedPanel(int selected)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Block {selected}", EditorStyles.boldLabel);

            // Read through _editing, not _levelData: a drag earlier in this same pass may have changed
            // the position, and the LevelData object only sees that after Apply().
            BlockData blockData = _editing.GetBlock(selected);

            _editing.SetPosition(selected, EditorGUILayout.Vector2IntField("Position", blockData.Position));
            _editing.SetLength(selected, EditorGUILayout.IntField("Length", blockData.Length));
            _editing.SetAxis(selected, (Axis)EditorGUILayout.EnumPopup("Axis", blockData.Axis));
            _editing.SetIsTarget(selected, EditorGUILayout.Toggle("Is Target", blockData.IsTarget));

            EditorGUILayout.Space();

            if (GUILayout.Button("Delete block"))
            {
                _editing.DeleteBlock(selected);
                _selectedIndex = NoSelection;
            }
        }

        #endregion

        #region Validation: solve and report

        private void DrawValidation()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Validate"))
                Validate();

            if (_validationMessage != null)
                EditorGUILayout.HelpBox(_validationMessage, _validationType);
        }

        // Builds a board from the level, solves it and stores a one-box report.
        // A broken level (overlaps, no target, too many blocks) throws an ArgumentException: shown, not crashed.
        private void Validate()
        {
            int par = 0;

            try
            {
                Board board = _levelData.CreateBoard();

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                SolveResult result = BoardSolver.Solve(board);
                stopwatch.Stop();

                double ms = stopwatch.Elapsed.TotalMilliseconds;

                if (!result.IsSolvable)
                {
                    _validationType = MessageType.Error;
                    _validationMessage =
                        $"Unsolvable: explored all {result.StatesExplored} reachable states in {ms:0.0} ms";
                }
                else if (result.MoveCount == 0)
                {
                    _validationType = MessageType.Warning;
                    _validationMessage = "Already solved at the start (0 moves): move the target away from the exit.";
                }
                else
                {
                    _validationType = MessageType.Info;
                    _validationMessage =
                        $"Solvable in {result.MoveCount} moves (par)\n" +
                        $"{result.StatesExplored} states explored in {ms:0.0} ms\n" +
                        $"Solution: {string.Join(", ", result.Moves)}";
                    par = result.MoveCount;
                }
            }
            catch (ArgumentException e)
            {
                _validationType = MessageType.Error;
                _validationMessage = $"Invalid level: {e.Message}";
            }

            // Validate runs after OnGUI's Apply, so apply here too; the next pass's Update() would drop it otherwise.
            _editing.SetParMoves(par);
            _editing.Apply();
        }

        #endregion
    }
}
