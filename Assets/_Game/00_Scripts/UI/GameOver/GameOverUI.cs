using UnityEngine;
using UnityEngine.UI;
using Slafurry.Systems.Scene;

namespace Slafurry.UI.GameOver
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("Scenes")]
        [SerializeField] private SceneReference _retryScene;
        [SerializeField] private SceneReference _mainMenuScene;

        private void Awake()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetryPressed);
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(OnMainMenuPressed);
        }

        private void OnRetryPressed() => SceneSystem.Load(_retryScene);

        private void OnMainMenuPressed() => SceneSystem.Load(_mainMenuScene);

        private void OnDestroy()
        {
            if (_retryButton != null) _retryButton.onClick.RemoveListener(OnRetryPressed);
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveListener(OnMainMenuPressed);
        }
    }
}
