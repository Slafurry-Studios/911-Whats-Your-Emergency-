using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Slafurry.UI.HUD.Phone
{
    public class PhoneTabHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _signalText;
        [SerializeField] private TMP_Text _batteryText;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Button _callButton;
        [SerializeField] private TMP_Text _callLabel;

        private bool _signalSubscribed;
        private bool _batterySubscribed;
        private bool _volumeSubscribed;
        private bool _callSubscribed;
        private bool _syncingVolume;

        private void Awake()
        {
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
        }
    }
}
