using UnityEngine;
using UnityEngine.UI;

namespace Slafurry.UI.Menu
{
    public class ExitConfirmPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private void Awake()
        {
            if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirmPressed);
            if (_cancelButton != null) _cancelButton.onClick.AddListener(OnCancelPressed);

            Hide();
        }

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void OnConfirmPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        private void OnCancelPressed() => Hide();

        private void OnDestroy()
        {
            if (_confirmButton != null) _confirmButton.onClick.RemoveListener(OnConfirmPressed);
            if (_cancelButton != null) _cancelButton.onClick.RemoveListener(OnCancelPressed);
        }
    }
}
