using System;
using SlidingBlock.Core;
using SlidingBlock.Data;
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
        [SerializeField] private WinScreenView _winScreenView;
        [SerializeField] private LevelData _levelData;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_levelData == null)
                throw new InvalidOperationException($"{nameof(GameLifetimeScope)} has no " +
                                                    $"{nameof(LevelData)} assigned. Assign a level in the Inspector.");
            
            Board board = _levelData.CreateBoard();
            BoardLayout boardLayout = new BoardLayout(board.Width, board.Height, _cellSize);

            builder.RegisterInstance(board);
            builder.RegisterInstance(boardLayout);
            builder.RegisterComponent(_boardView);
            builder.RegisterComponent(_camera);
            builder.RegisterComponent(_winScreenView);
            builder.RegisterEntryPoint<BoardPresenter>().AsSelf();
            builder.RegisterEntryPoint<BoardInput>();
            builder.RegisterEntryPoint<WinScreenPresenter>();
        }
    }
}
