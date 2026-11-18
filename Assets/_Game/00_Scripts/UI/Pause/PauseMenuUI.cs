using UnityEngine;
using UnityEngine.UI;
using Slafurry.Systems.InputHub;
using Slafurry.Systems.Pause;
using Slafurry.Systems.Scene;
using Slafurry.UI.Menu;
using PauseStack = Slafurry.Systems.Pause.Pause;

namespace Slafurry.UI.Pause
{
    public class PauseMenuUI : MonoBehaviour
    {
        private const string PauseKey = "UI";

        [Header("Buttons")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _backToMenuButton;

        [Header("Panels")]
        [SerializeField] private GameObject _popupRoot;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Settings")]
        [SerializeField] private SettingsUI _settingsUI;

        [Header("Scenes")]
        [SerializeField] private SceneReference _mainMenuScene;

        private bool _isOpen;

        private void Awake()
        {
            if (_pauseButton != null) _pauseButton.onClick.AddListener(OnPausePressed);
            if (_resumeButton != null) _resumeButton.onClick.AddListener(OnResumePressed);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsPressed);
            if (_backToMenuButton != null) _backToMenuButton.onClick.AddListener(OnBackToMenuPressed);
            if (_settingsUI != null) _settingsUI.BackRequested += OnSettingsBack;

            if (_popupRoot != null) _popupRoot.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_mainPanel != null) _mainPanel.SetActive(true);
        }

        private void OnPausePressed() => ShowPopup();

        private void OnResumePressed() => HidePopup();

        private void OnSettingsPressed()
        {
            if (_mainPanel != null) _mainPanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(true);
        }

        private void OnSettingsBack()
        {
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_mainPanel != null) _mainPanel.SetActive(true);
        }

        private void OnBackToMenuPressed()
        {
            SceneReference menu = _mainMenuScene;
            HidePopup();
            SceneSystem.Load(menu);
        }

        private void ShowPopup()
        {
            if (_isOpen) return;
            _isOpen = true;

            if (PauseSystem.Instance != null) PauseStack.On(PauseKey);
            Controls.SetInputEnabled(false);

            if (_mainPanel != null) _mainPanel.SetActive(true);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_popupRoot != null) _popupRoot.SetActive(true);
            if (_pauseButton != null) _pauseButton.gameObject.SetActive(false);
        }

        private void HidePopup()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (PauseSystem.Instance != null) PauseStack.Off(PauseKey);
            Controls.SetInputEnabled(true);

            if (_popupRoot != null) _popupRoot.SetActive(false);
            if (_pauseButton != null) _pauseButton.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            if (_pauseButton != null) _pauseButton.onClick.RemoveListener(OnPausePressed);
            if (_resumeButton != null) _resumeButton.onClick.RemoveListener(OnResumePressed);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OnSettingsPressed);
            if (_backToMenuButton != null) _backToMenuButton.onClick.RemoveListener(OnBackToMenuPressed);
            if (_settingsUI != null) _settingsUI.BackRequested -= OnSettingsBack;

            if (_isOpen)
            {
                if (PauseSystem.Instance != null) PauseStack.Off(PauseKey);
                Controls.SetInputEnabled(true);
            }
        }
    }
}
