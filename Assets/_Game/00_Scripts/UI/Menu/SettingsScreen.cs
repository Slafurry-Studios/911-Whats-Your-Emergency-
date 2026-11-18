using UnityEngine;
using Slafurry.Systems.Scene;

namespace Slafurry.UI.Menu
{
    public class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private SettingsUI _settingsUI;
        [SerializeField] private SceneReference _backScene;

        private void OnEnable()
        {
            if (_settingsUI != null) _settingsUI.BackRequested += OnBack;
        }

        private void OnDisable()
        {
            if (_settingsUI != null) _settingsUI.BackRequested -= OnBack;
        }

        private void OnBack() => SceneSystem.Load(_backScene);
    }
}
