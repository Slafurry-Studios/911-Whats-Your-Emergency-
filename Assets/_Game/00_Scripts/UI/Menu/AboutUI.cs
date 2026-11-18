using UnityEngine;
using UnityEngine.UI;
using Slafurry.Systems.Scene;

namespace Slafurry.UI.Menu
{
    public class AboutUI : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private SceneReference _backScene;

        private void Awake()
        {
            if (_backButton != null) _backButton.onClick.AddListener(OnBackPressed);
        }

        private void OnBackPressed() => SceneSystem.Load(_backScene);

        private void OnDestroy()
        {
            if (_backButton != null) _backButton.onClick.RemoveListener(OnBackPressed);
        }
    }
}
