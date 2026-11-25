using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Slafurry.UI.HUD.Phone
{
    /// <summary>Shows PoliceArrival.Progress on a slider (same style as health/stamina).</summary>
    [RequireComponent(typeof(Slider))]
    public class PoliceArrivalHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private Slider _slider;
        private bool _subscribed;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _slider.normalizedValue = 0f;
        }

        private void Update()
        {
            if (!_subscribed && PoliceArrival.Instance != null)
            {
                PoliceArrival.Instance.OnChanged += OnChanged;
                OnChanged(PoliceArrival.Instance.Progress);
                _subscribed = true;
            }
        }

        private void OnChanged(float progress)
        {
            _slider.normalizedValue = progress;
            if (_text != null) _text.text = $"{progress * 100f:F0}%";
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            if (PoliceArrival.Instance != null)
                PoliceArrival.Instance.OnChanged -= OnChanged;
            _subscribed = false;
        }
    }
}
