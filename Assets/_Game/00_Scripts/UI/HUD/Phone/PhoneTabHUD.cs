using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Slafurry.Systems.InputHub;

namespace Slafurry.UI.HUD.Phone
{
    public class PhoneTabHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _signalText;
        [SerializeField] private TMP_Text _batteryText;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Button _callButton;
        [SerializeField] private TMP_Text _callLabel;

        [Header("Tab Slide")]
        [SerializeField] private float _hiddenOffsetY = -400f;
        [SerializeField] private float _slideSpeed = 1000f;

        private bool _signalSubscribed;
        private bool _batterySubscribed;
        private bool _volumeSubscribed;
        private bool _callSubscribed;
        private bool _tabSubscribed;
        private bool _syncingVolume;
        private bool _isOpen = true;

        private RectTransform _rect;
        private Vector2 _shownPos;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _shownPos = _rect.anchoredPosition;

            if (_volumeSlider != null)
                _volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);

            if (_callButton != null)
                _callButton.onClick.AddListener(OnCallPressed);
        }

        private void Update()
        {
            if (!_signalSubscribed && PhoneSignal.Instance != null)
            {
                PhoneSignal.Instance.OnChanged += OnSignalChanged;
                OnSignalChanged(PhoneSignal.Instance.Level);
                _signalSubscribed = true;
            }

            if (!_batterySubscribed && PhoneBattery.Instance != null)
            {
                PhoneBattery.Instance.OnChanged += OnBatteryChanged;
                OnBatteryChanged(PhoneBattery.Instance.Percent);
                _batterySubscribed = true;
            }

            if (!_volumeSubscribed && PhoneVolume.Instance != null)
            {
                PhoneVolume.Instance.OnChanged += OnVolumeChanged;
                OnVolumeChanged(PhoneVolume.Instance.Volume);
                _volumeSubscribed = true;
            }

            if (!_callSubscribed && PhoneCall.Instance != null)
            {
                PhoneCall.Instance.OnCallChanged += OnCallChanged;
                OnCallChanged(PhoneCall.Instance.IsOnCall);
                _callSubscribed = true;
            }

            if (!_tabSubscribed && InputHub.Instance != null)
            {
                Controls.OnTabPressed += OnTabPressed;
                _tabSubscribed = true;
            }

            Slide();
        }

        private void Slide()
        {
            Vector2 target = _isOpen
                ? _shownPos
                : _shownPos + Vector2.up * _hiddenOffsetY;

            _rect.anchoredPosition = Vector2.MoveTowards(
                _rect.anchoredPosition, target, _slideSpeed * Time.deltaTime);
        }

        private void OnTabPressed()
        {
            if (!Controls.IsInputEnabled) return;

            _isOpen = !_isOpen;
        }

        private void OnSignalChanged(int level)
        {
            if (_signalText != null)
                _signalText.text = $"{level}/{PhoneSignal.Instance.MaxLevel}";
        }

        private void OnBatteryChanged(float percent)
        {
            if (_batteryText != null)
                _batteryText.text = $"{percent:F0}%";
        }

        private void OnVolumeChanged(float volume)
        {
            if (_volumeSlider == null) return;

            _syncingVolume = true;
            _volumeSlider.SetValueWithoutNotify(volume);
            _syncingVolume = false;
        }

        private void OnVolumeSliderChanged(float value)
        {
            if (_syncingVolume) return;
            if (PhoneVolume.Instance == null) return;

            PhoneVolume.Instance.SetVolume(value);
        }

        private void OnCallPressed()
        {
            if (PhoneCall.Instance == null) return;

            PhoneCall.Instance.ToggleCall();
        }

        private void OnCallChanged(bool isOnCall)
        {
            if (_callLabel != null)
                _callLabel.text = isOnCall ? "End" : "Call";
        }

        private void OnDestroy()
        {
            if (_volumeSlider != null)
                _volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderChanged);

            if (_callButton != null)
                _callButton.onClick.RemoveListener(OnCallPressed);

            if (_signalSubscribed && PhoneSignal.Instance != null)
                PhoneSignal.Instance.OnChanged -= OnSignalChanged;

            if (_batterySubscribed && PhoneBattery.Instance != null)
                PhoneBattery.Instance.OnChanged -= OnBatteryChanged;

            if (_volumeSubscribed && PhoneVolume.Instance != null)
                PhoneVolume.Instance.OnChanged -= OnVolumeChanged;

            if (_callSubscribed && PhoneCall.Instance != null)
                PhoneCall.Instance.OnCallChanged -= OnCallChanged;

            if (_tabSubscribed)
            {
                Controls.OnTabPressed -= OnTabPressed;
                _tabSubscribed = false;
            }
        }
    }
}
