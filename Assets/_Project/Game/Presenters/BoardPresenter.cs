using System;
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
                BlockView blockView = _boardView.CreateBlockView();
                blockView.SetTarget(block.IsTarget);
                _blockViews[block.Id] = blockView;
                UpdateBlockView(block.Id);
            }
        }

        public bool TryMove(Move move)
        {
            if (!_board.CanMove(move)) return false;
            _board.ApplyMove(move);
            UpdateBlockView(move.BlockId);
            return true;
        }

        private void UpdateBlockView(int blockId)
        {
            Block block = _board.GetBlock(blockId);
            BlockView blockView = _blockViews[blockId];
            blockView.SetBounds(_boardLayout.BlockCenter(block), _boardLayout.BlockSize(block));
        }
    }
}
