using System;
using UnityEngine;
using UnityEngine.UI;

namespace SlidingBlock.Game
{
    public sealed class WinScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject _winScreenPanel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextButton;

        public event Action RestartClicked;
        public event Action NextClicked;

        private void Awake()
        {
            _restartButton.onClick.AddListener(OnRestartClicked);
            _nextButton.onClick.AddListener(OnNextClicked);
        }

        private void OnDestroy()
        {
            _restartButton.onClick.RemoveListener(OnRestartClicked);
            _nextButton.onClick.RemoveListener(OnNextClicked);
        }

        public void Show(bool showNextButton)
        {
            _winScreenPanel.SetActive(true);
            _nextButton.gameObject.SetActive(showNextButton);
        }

        public void Hide()
        {
            _winScreenPanel.SetActive(false);
        }

        private void OnRestartClicked()
        {
            RestartClicked?.Invoke();
        }

        private void OnNextClicked()
        {
            NextClicked?.Invoke();
        }
    }
}
