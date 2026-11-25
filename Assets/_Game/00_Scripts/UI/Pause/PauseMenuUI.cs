using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Slafurry.Systems.InputHub;
using Slafurry.Systems.Pause;
using Slafurry.Systems.Scene;
using Slafurry.UI.Menu;
using PauseStack = Slafurry.Systems.Pause.Pause;

namespace Slafurry.UI.Pause
{
    /// <summary>
    /// In-game pause: HUD pause button or Escape opens the popup (time stops, gameplay input off).
    /// Escape again goes back from Settings, then resumes.
    /// </summary>
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

        [Header("Input")]
        [Tooltip("Escape toggles the pause menu.")]
        [SerializeField] private bool _escapeToggles = true;
        [Tooltip("Gameplay UI is mouse-only. WASD is also the UI 'Navigate' input, so with keyboard " +
                 "navigation on, moving the player can move the UI selection.")]
        [SerializeField] private bool _disableKeyboardNavigation = true;
        [Tooltip("Logs who opened the popup (with stack trace) to the Console.")]
        [SerializeField] private bool _logOpenSource = true;

        private bool _isOpen;
        private bool _inputWasEnabled;
        private PlayerThrow _playerThrow;

        public bool IsOpen => _isOpen;

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

            if (_pauseButton != null)
                _pauseButton.navigation = new Navigation { mode = Navigation.Mode.None };
        }

        private void Update()
        {
            // The active EventSystem can change when scenes load/unload, so keep enforcing this.
            if (_disableKeyboardNavigation && EventSystem.current != null)
                EventSystem.current.sendNavigationEvents = false;

            if (_escapeToggles && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                OnEscape();
        }

        private void OnEscape()
        {
            if (!_isOpen)
            {
                // Escape first cancels a throw aim (PlayerThrow handles that).
                if (_playerThrow == null) _playerThrow = FindObjectOfType<PlayerThrow>();
                if (_playerThrow != null && _playerThrow.IsAiming) return;

                // Don't pause over a modal that already took input (e.g. charging screen).
                if (!Controls.IsInputEnabled) return;

                ShowPopup("Escape key");
                return;
            }

            if (_settingsPanel != null && _settingsPanel.activeSelf) OnSettingsBack();
            else HidePopup();
        }

        // A clicked button stays selected; clear it so no keyboard input can re-submit it.
        private static void ClearSelection()
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void OnPausePressed() => ShowPopup("Pause button onClick");

        private void OnResumePressed() => HidePopup();

        private void OnSettingsPressed()
        {
            if (_mainPanel != null) _mainPanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(true);
            ClearSelection();
        }

        private void OnSettingsBack()
        {
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_mainPanel != null) _mainPanel.SetActive(true);
            ClearSelection();
        }

        private void OnBackToMenuPressed()
        {
            SceneReference menu = _mainMenuScene;
            HidePopup();
            SceneSystem.Load(menu);
        }

        private void ShowPopup(string source)
        {
            if (_isOpen) return;
            _isOpen = true;

            if (_logOpenSource)
                Debug.Log($"[Pause] Popup opened by: {source} (frame {Time.frameCount}). " +
                          "If this was not you, the stack trace below shows the caller.", this);

            _inputWasEnabled = Controls.IsInputEnabled;
            if (PauseSystem.Instance != null) PauseStack.On(PauseKey);
            Controls.SetInputEnabled(false);

            if (_mainPanel != null) _mainPanel.SetActive(true);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_popupRoot != null) _popupRoot.SetActive(true);
            if (_pauseButton != null) _pauseButton.gameObject.SetActive(false);
            ClearSelection();
        }

        private void HidePopup()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (PauseSystem.Instance != null) PauseStack.Off(PauseKey);
            // Only give input back if pausing took it (charging screen etc. keep theirs).
            if (_inputWasEnabled) Controls.SetInputEnabled(true);

            if (_popupRoot != null) _popupRoot.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_pauseButton != null) _pauseButton.gameObject.SetActive(true);
            ClearSelection();
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
                if (_inputWasEnabled) Controls.SetInputEnabled(true);
            }
        }
    }
}
