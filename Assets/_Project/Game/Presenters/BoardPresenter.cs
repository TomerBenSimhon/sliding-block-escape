using System;
using Cysharp.Threading.Tasks;
using SlidingBlock.Core;
using VContainer.Unity;

namespace SlidingBlock.Game
{
    public sealed class BoardPresenter : IStartable
    {
        private readonly Board _board;
        private readonly BoardLayout _boardLayout;
        private readonly BoardView _boardView;
        private readonly BlockView[] _blockViews;

        public bool IsAnimating { get; private set; }

        public BoardPresenter(Board board, BoardLayout boardLayout, BoardView boardView)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _boardLayout = boardLayout ?? throw new ArgumentNullException(nameof(boardLayout));
            _boardView = boardView ?? throw new ArgumentNullException(nameof(boardView));
            _blockViews = new BlockView[board.Blocks.Count];
        }

        public void Start()
        {
            _boardView.Clear();
            _boardView.SetSize(_boardLayout.WorldSize);
            foreach (Block block in _board.Blocks)
            {
                var blockView = _boardView.CreateBlockView();
                blockView.SetTarget(block.IsTarget);
                _blockViews[block.Id] = blockView;
                UpdateBlockView(block.Id);
            }
        }

        public bool TryMove(Move move)
        {
            if (IsAnimating || !_board.CanMove(move)) return false;
            _board.ApplyMove(move);
            AnimateBlockViewAsync(move.BlockId).Forget();
            return true;
        }

        public void PreviewDrag(int blockId, float cellOffset)
        {
            var block = _board.GetBlock(blockId);
            var blockView = _blockViews[blockId];

            blockView.SetPosition(_boardLayout.BlockCenter(block, cellOffset));
        }

        public bool EndDrag(int blockId, int distance)
        {
            if (distance != 0 && TryMove(new Move(blockId, distance))) return true;

            AnimateBlockViewAsync(blockId).Forget();
            return false;
        }

        private async UniTaskVoid AnimateBlockViewAsync(int blockId)
        {
            IsAnimating = true;
            var block = _board.GetBlock(blockId);
            var blockView = _blockViews[blockId];
            try
            {
                await blockView.AnimateTo(_boardLayout.BlockCenter(block));
            }
            finally
            {
                IsAnimating = false;
            }
        }

        private void UpdateBlockView(int blockId)
        {
            var block = _board.GetBlock(blockId);
            var blockView = _blockViews[blockId];
            blockView.SetBounds(_boardLayout.BlockCenter(block), _boardLayout.BlockSize(block));
        }
    }
}
