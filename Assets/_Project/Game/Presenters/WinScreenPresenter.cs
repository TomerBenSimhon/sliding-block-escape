using System;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace SlidingBlock.Game
{
    public sealed class WinScreenPresenter : IStartable, IDisposable
    {
        private readonly WinScreenView _winScreenView;
        private readonly BoardPresenter _boardPresenter;

        public WinScreenPresenter(WinScreenView winScreenView, BoardPresenter boardPresenter)
        {
            _winScreenView = winScreenView ?? throw new ArgumentNullException(nameof(winScreenView));
            _boardPresenter = boardPresenter ?? throw new ArgumentNullException(nameof(boardPresenter));
        }

        public void Start()
        {
            _winScreenView.Hide();
            _winScreenView.RestartClicked += OnRestartClicked;
            _boardPresenter.Solved += OnBoardSolved;
        }

        public void Dispose()
        {
            _winScreenView.RestartClicked -= OnRestartClicked;
            _boardPresenter.Solved -= OnBoardSolved;
        }

        private void OnBoardSolved()
        {
            _winScreenView.Show();
        }

        private void OnRestartClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
