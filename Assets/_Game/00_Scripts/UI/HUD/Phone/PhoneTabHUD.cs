using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Slafurry.Systems.InputHub;

namespace Slafurry.UI.HUD.Phone
{
    public class PhoneTabHUD : MonoBehaviour
    {
        [SerializeField] private ItemData _phoneData;
        [SerializeField] private TMP_Text _batteryText;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Button _callButton;
        [SerializeField] private TMP_Text _callLabel;
        [SerializeField] private Toggle _checkbox;

        [Header("Tab Slide")]
        [SerializeField] private float _hiddenOffsetY = -400f;
        [SerializeField] private float _slideSpeed = 1000f;

        private bool _batterySubscribed;
        private bool _volumeSubscribed;
        private bool _callSubscribed;
        private bool _checkboxSubscribed;
        private bool _tabSubscribed;
        private bool _syncingVolume;
        private bool _isOpen = true;
        private bool _hasPhone;

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

            if (_checkbox != null)
                _checkbox.onValueChanged.AddListener(OnCheckboxChanged);

            if (PlayerInventory.Instance != null)
                PlayerInventory.Instance.OnInventoryChanged += OnInventoryChanged;
        }

        private void OnInventoryChanged()
        {
            _hasPhone = _phoneData != null && PlayerInventory.Instance != null &&
                        PlayerInventory.Instance.HasItem(_phoneData);
            if (!_hasPhone)
            {
                _isOpen = false;
                UnsubscribeAll();
            }
        }

        private void UnsubscribeAll()
        {
            if (_batterySubscribed && PhoneBattery.Instance != null)
                PhoneBattery.Instance.OnChanged -= OnBatteryChanged;
            _batterySubscribed = false;

            if (_volumeSubscribed && PhoneVolume.Instance != null)
                PhoneVolume.Instance.OnChanged -= OnVolumeChanged;
            _volumeSubscribed = false;

            if (_callSubscribed && PhoneCall.Instance != null)
                PhoneCall.Instance.OnCallChanged -= OnCallChanged;
            _callSubscribed = false;

            if (_tabSubscribed)
            {
                Controls.OnTabPressed -= OnTabPressed;
                _tabSubscribed = false;
            }
        }

        private void Update()
        {
            if (PlayerInventory.Instance == null) return;

            bool hasPhone = _phoneData != null && PlayerInventory.Instance.HasItem(_phoneData);
            if (hasPhone != _hasPhone)
            {
                _hasPhone = hasPhone;
                if (!_hasPhone)
                {
                    _isOpen = false;
                    UnsubscribeAll();
                }
            }

            if (!_hasPhone)
            {
                Slide();
                return;
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

            if (!_checkboxSubscribed && _checkbox != null)
            {
                _checkboxSubscribed = true;
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

        private void OnBatteryChanged(float percent)
        {
            if (_batteryText != null)
                _batteryText.text = $"{percent:F0}%";
            RefreshCallButton();
        }

        // The 911 call is one-way: once placed it can't be ended, and it needs battery to start.
        private void RefreshCallButton()
        {
            bool onCall = PhoneCall.Instance != null && PhoneCall.Instance.IsOnCall;
            bool canCall = PhoneCall.Instance != null && PhoneCall.Instance.CanCall;
            if (_callButton != null) _callButton.interactable = !onCall && canCall;
            if (_callLabel != null) _callLabel.text = onCall ? "Calling..." : canCall ? "Call" : "No Battery";
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
            if (PhoneCall.Instance == null || PhoneCall.Instance.IsOnCall) return;

            PhoneCall.Instance.Call();
            RefreshCallButton();
        }

        private void OnCallChanged(bool isOnCall) => RefreshCallButton();

        private void OnCheckboxChanged(bool isOn)
        {
            // Placeholder — wire up checkbox behaviour here when needed.
        }

        private void OnDestroy()
        {
            if (PlayerInventory.Instance != null)
                PlayerInventory.Instance.OnInventoryChanged -= OnInventoryChanged;

            UnsubscribeAll();

            if (_volumeSlider != null)
                _volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderChanged);

            if (_callButton != null)
                _callButton.onClick.RemoveListener(OnCallPressed);

            if (_checkbox != null)
                _checkbox.onValueChanged.RemoveListener(OnCheckboxChanged);

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
