using SlidingBlock.Core;
using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace SlidingBlock.Game
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private Camera _camera;
        [SerializeField, Min(0.1f)] private float _cellSize = 0.65f;

        protected override void Configure(IContainerBuilder builder)
        {
            Board board = TestLevels.CreateExampleBoard();
            BoardLayout boardLayout = new BoardLayout(board.Width, board.Height, _cellSize);

            builder.RegisterInstance(board);
            builder.RegisterInstance(boardLayout);
            builder.RegisterComponent(_boardView);
            builder.RegisterComponent(_camera);
            builder.RegisterEntryPoint<BoardPresenter>().AsSelf();
            builder.RegisterEntryPoint<BoardInput>();
        }
    }
}
