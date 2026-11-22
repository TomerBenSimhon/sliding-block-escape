using System;
using SlidingBlock.Core;
using SlidingBlock.Data;
using SlidingBlock.Services;
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
        [SerializeField] private LevelPack _levelPack;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_levelPack == null)
                throw new InvalidOperationException($"{nameof(GameLifetimeScope)} has no " +
                                                    $"{nameof(LevelPack)} assigned. Assign a Level Pack in the Inspector.");
            if (_levelPack.Count == 0)
                throw new InvalidOperationException($"{nameof(GameLifetimeScope)} has a " +
                                                    $"{nameof(LevelPack)} with no levels. Assign levels in the Inspector.");

            PlayerPrefsLevelProgress progress = new PlayerPrefsLevelProgress();
            int levelIndex = progress.LoadCurrentLevelIndex();
            levelIndex = levelIndex < 0 || levelIndex >= _levelPack.Count ? 0 : levelIndex;
            LevelData currentLevelData = _levelPack.GetLevelData(levelIndex);

            Board board = currentLevelData.CreateBoard();
            BoardLayout boardLayout = new BoardLayout(board.Width, board.Height, _cellSize);
            CurrentLevel currentLevel = new CurrentLevel(levelIndex, _levelPack.Count);

            builder.RegisterInstance(board);
            builder.RegisterInstance(boardLayout);
            builder.RegisterInstance<ILevelProgress>(progress);
            builder.RegisterInstance(currentLevel);
            builder.RegisterComponent(_boardView);
            builder.RegisterComponent(_camera);
            builder.RegisterComponent(_winScreenView);
            builder.RegisterEntryPoint<BoardPresenter>().AsSelf();
            builder.RegisterEntryPoint<BoardInput>();
            builder.RegisterEntryPoint<WinScreenPresenter>();
        }
    }
}
