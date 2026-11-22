using System;
using SlidingBlock.Services;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace SlidingBlock.Game
{
    public sealed class WinScreenPresenter : IStartable, IDisposable
    {
        private readonly WinScreenView _winScreenView;
        private readonly BoardPresenter _boardPresenter;
        private readonly CurrentLevel _currentLevel;
        private readonly ILevelProgress _levelProgress;

        public WinScreenPresenter(
            WinScreenView winScreenView,
            BoardPresenter boardPresenter,
            CurrentLevel currentLevel,
            ILevelProgress levelProgress)
        {
            _winScreenView = winScreenView ?? throw new ArgumentNullException(nameof(winScreenView));
            _boardPresenter = boardPresenter ?? throw new ArgumentNullException(nameof(boardPresenter));
            _currentLevel = currentLevel ?? throw new ArgumentNullException(nameof(currentLevel));
            _levelProgress = levelProgress ?? throw new ArgumentNullException(nameof(levelProgress));
        }

        public void Start()
        {
            _winScreenView.Hide();
            _winScreenView.RestartClicked += OnRestartClicked;
            _winScreenView.NextClicked += OnNextClicked;
            _boardPresenter.Solved += OnBoardSolved;
        }

        public void Dispose()
        {
            _winScreenView.RestartClicked -= OnRestartClicked;
            _winScreenView.NextClicked -= OnNextClicked;
            _boardPresenter.Solved -= OnBoardSolved;
        }

        private void OnBoardSolved()
        {
            _winScreenView.Show(showNextButton: !_currentLevel.IsLast);
        }

        private void OnRestartClicked()
        {
            ReloadScene();
        }

        private void OnNextClicked()
        {
            _levelProgress.SaveCurrentLevelIndex(_currentLevel.Index + 1);
            ReloadScene();
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
