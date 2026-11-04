using System;
using UnityEngine;
using UnityEngine.UI;

namespace SlidingBlock.Game
{
    public sealed class WinScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject _winScreenPanel;
        [SerializeField] private Button _restartButton;

        public event Action RestartClicked;

        private void Awake()
        {
            _restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnDestroy()
        {
            _restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        public void Show()
        {
            _winScreenPanel.SetActive(true);
        }

        public void Hide()
        {
            _winScreenPanel.SetActive(false);
        }

        private void OnRestartClicked()
        {
            RestartClicked?.Invoke();
        }
    }
}
