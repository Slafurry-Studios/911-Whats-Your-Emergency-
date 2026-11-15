using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Slafurry.UI.HUD.Stamina
{
    [RequireComponent(typeof(Slider))]
    public class PlayerStaminaHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private Slider _slider;
        private bool _subscribed;

        private void Awake() => _slider = GetComponent<Slider>();

        private void Update()
        {
            if (!_subscribed && PlayerStamina.Instance != null)
            {
                PlayerStamina.Instance.OnChanged += OnChanged;
                OnChanged(PlayerStamina.Instance.Current, PlayerStamina.Instance.Max);
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
            if (PlayerStamina.Instance != null)
                PlayerStamina.Instance.OnChanged -= OnChanged;
            _subscribed = false;
        }
    }
}
