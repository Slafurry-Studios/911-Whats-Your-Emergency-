using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Slafurry.UI.HUD.Health
{
    [RequireComponent(typeof(Slider))]
    public class PlayerHealthHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private Slider _slider;
        private bool _subscribed;

        private void Awake() => _slider = GetComponent<Slider>();

        private void Update()
        {
            if (!_subscribed && PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.OnChanged += OnChanged;
                OnChanged(PlayerHealth.Instance.Current, PlayerHealth.Instance.Max);
                _subscribed = true;
            }
        }

        private void OnChanged(float current, float max)
        {
            _slider.normalizedValue = max > 0f ? current / max : 0f;
            if (_text != null) _text.text = $"{current:F0}/{max:F0}";
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            if (PlayerHealth.Instance != null)
                PlayerHealth.Instance.OnChanged -= OnChanged;
            _subscribed = false;
        }
    }
}
