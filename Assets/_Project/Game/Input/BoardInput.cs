using System;
using SlidingBlock.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace SlidingBlock.Game
{
    public sealed class BoardInput : ITickable
    {
        private readonly Camera _camera;
        private readonly BoardLayout _boardLayout;
        private readonly Board _board;
        private readonly BoardPresenter _boardPresenter;
        
        private int _draggedBlockId = Board.Empty;
        private GridPos _pressCell;

        public BoardInput(Camera camera, BoardLayout boardLayout, Board board, BoardPresenter boardPresenter)
        {
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            _boardLayout = boardLayout ?? throw new ArgumentNullException(nameof(boardLayout));
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _boardPresenter = boardPresenter ?? throw new ArgumentNullException(nameof(boardPresenter));
        }
        
        public void Tick()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;
            
            if (pointer.press.wasPressedThisFrame)
            {
                GridPos cell = ScreenToCell(pointer.position.ReadValue());
                OnPress(cell);
            }
            else if (pointer.press.wasReleasedThisFrame)
            {
                GridPos cell = ScreenToCell(pointer.position.ReadValue());
                OnRelease(cell);
            }
        }

        private GridPos ScreenToCell(Vector2 screenPosition)
        {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            return _boardLayout.WorldToCell(worldPosition);
        }

        private void OnPress(GridPos cell)
        {
            if (!_board.IsInside(cell)) return;
            _draggedBlockId = _board.BlockAt(cell);
            _pressCell = cell;
        }

        private void OnRelease(GridPos cell)
        {
            if (_draggedBlockId == Board.Empty) return;
            
            Block draggedBlock = _board.GetBlock(_draggedBlockId);
            GridPos delta = cell - _pressCell;
            int distance = draggedBlock.Axis == Axis.Horizontal ? delta.X : delta.Y;

            while (distance != 0 && !_boardPresenter.TryMove(new Move(_draggedBlockId, distance)))
            {
                distance -= Math.Sign(distance);
            }
            
            if (distance != 0 && _board.IsSolved) Debug.Log("Board is solved");
            _draggedBlockId = Board.Empty;
        }
    }
}
