using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Slafurry.UI.HUD.Sanity
{
    [RequireComponent(typeof(Slider))]
    public class PlayerSanityHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private Slider _slider;
        private bool _subscribed;

        private void Awake() => _slider = GetComponent<Slider>();

        private void Update()
        {
            if (!_subscribed && PlayerSanity.Instance != null)
            {
                PlayerSanity.Instance.OnChanged += OnChanged;
                OnChanged(PlayerSanity.Instance.Current, PlayerSanity.Instance.Max);
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
            if (PlayerSanity.Instance != null)
                PlayerSanity.Instance.OnChanged -= OnChanged;
            _subscribed = false;
        }
    }
}
