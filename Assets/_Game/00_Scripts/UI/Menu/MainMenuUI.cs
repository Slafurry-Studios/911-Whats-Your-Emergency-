using UnityEngine;
using UnityEngine.UI;
using Slafurry.Systems.Scene;

namespace Slafurry.UI.Menu
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _aboutButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        [Header("Scenes")]
        [SerializeField] private SceneReference _startScene;
        [SerializeField] private SceneReference _aboutScene;
        [SerializeField] private SceneReference _settingsScene;

        [Header("Exit")]
        [SerializeField] private ExitConfirmPopup _exitPopup;

        private void Awake()
        {
            if (_startButton != null) _startButton.onClick.AddListener(OnStartPressed);
            if (_aboutButton != null) _aboutButton.onClick.AddListener(OnAboutPressed);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsPressed);
            if (_exitButton != null) _exitButton.onClick.AddListener(OnExitPressed);
        }

        private void OnStartPressed() => SceneSystem.Load(_startScene);

        private void OnAboutPressed() => SceneSystem.Load(_aboutScene);

        private void OnSettingsPressed() => SceneSystem.Load(_settingsScene);

        private void OnExitPressed()
        {
            if (_exitPopup != null) _exitPopup.Show();
        }

        private void OnDestroy()
        {
            if (_startButton != null) _startButton.onClick.RemoveListener(OnStartPressed);
            if (_aboutButton != null) _aboutButton.onClick.RemoveListener(OnAboutPressed);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OnSettingsPressed);
            if (_exitButton != null) _exitButton.onClick.RemoveListener(OnExitPressed);
        }
    }
}
