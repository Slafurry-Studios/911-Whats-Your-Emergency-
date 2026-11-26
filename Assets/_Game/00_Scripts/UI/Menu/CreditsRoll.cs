using UnityEngine;
using UnityEngine.InputSystem;
using Slafurry.Systems.Scene;

namespace Slafurry.UI.Menu
{
    /// <summary>Scrolls the credits up; when they've passed (or on Esc / click) goes to the menu.</summary>
    public class CreditsRoll : MonoBehaviour
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private float _speed = 60f;          // canvas units per second
        [SerializeField] private float _endY = 1400f;          // content y when done
        [SerializeField] private float _skipDelay = 1f;
        [SerializeField] private SceneReference _menu;

        private float _start;
        private bool _done;

        private void Start()
        {
            _start = Time.unscaledTime;
            // Scroll until the whole content has left the screen (measured after layout).
            Canvas.ForceUpdateCanvases();
            if (_content != null && _content.parent is RectTransform parent)
                _endY = Mathf.Max(_endY, _content.rect.height + parent.rect.height);
        }

        private void Update()
        {
            if (_done || _content == null) return;
            _content.anchoredPosition += Vector2.up * (_speed * Time.unscaledDeltaTime);

            bool skip = Time.unscaledTime - _start > _skipDelay &&
                        ((Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame ||
                                                       Keyboard.current.spaceKey.wasPressedThisFrame)) ||
                         (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame));

            if (skip || _content.anchoredPosition.y >= _endY)
            {
                _done = true;
                if (_menu != null) SceneSystem.Load(_menu);
            }
        }
    }
}
