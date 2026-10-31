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
        private Vector3 _pressWorld;     // where the finger went down, in world units
        private int _minDistance;       // how far the block may go left/down (0 or negative)
        private int _maxDistance;       // how far the block may go right/up (0 or positive)
        private float _dragOffset;      // where the block is right now, in cells (e.g. 1.38)

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

            Vector3 worldPos = ScreenToWorld(pointer.position.ReadValue());

            if (pointer.press.wasPressedThisFrame)
            {
                OnPress(worldPos);
            }
            else if (pointer.press.wasReleasedThisFrame)
            {
                OnRelease();
            }
            else if (pointer.press.isPressed)
            {
                OnDrag(worldPos);
            }
        }

        private Vector3 ScreenToWorld(Vector2 screenPosition)
        {
            return _camera.ScreenToWorldPoint(screenPosition);
        }

        private void OnPress(Vector3 worldPosition)
        {
            if (_boardPresenter.IsAnimating) return;

            GridPos cell = _boardLayout.WorldToCell(worldPosition);
            if (!_board.IsInside(cell)) return;

            int blockId = _board.BlockAt(cell);
            if (blockId == Board.Empty) return;

            _draggedBlockId = blockId;
            _pressWorld = worldPosition;
            _minDistance = -_board.MaxDistance(blockId, -1);
            _maxDistance = _board.MaxDistance(blockId, 1);
            _dragOffset = 0f;
        }

        private void OnDrag(Vector3 worldPosition)
        {
            if (_draggedBlockId == Board.Empty) return;

            var block = _board.GetBlock(_draggedBlockId);
            Vector3 delta = worldPosition - _pressWorld;
            float along = block.Axis == Axis.Horizontal ? delta.x : delta.y;
            _dragOffset = Mathf.Clamp(along / _boardLayout.CellSize, _minDistance, _maxDistance);

            _boardPresenter.PreviewDrag(_draggedBlockId, _dragOffset);
        }

        private void OnRelease()
        {
            if (_draggedBlockId == Board.Empty) return;

            int distance = Mathf.RoundToInt(_dragOffset);
            bool moved = _boardPresenter.EndDrag(_draggedBlockId, distance);
            if (moved && _board.IsSolved) Debug.Log("Board is solved");

            _draggedBlockId = Board.Empty;
        }
    }
}
