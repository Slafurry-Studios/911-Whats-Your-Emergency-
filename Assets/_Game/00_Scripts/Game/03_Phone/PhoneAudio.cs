using UnityEngine;

/// <summary>Phone sounds: low-battery beeps at 15% and 5%, interference when the 911 call connects.</summary>
public class PhoneAudio : MonoBehaviour
{
    [SerializeField] private float[] _lowBatteryWarnings = { 15f, 5f };

    private float _lastPercent = 100f;
    private bool _batterySubscribed;
    private bool _callSubscribed;

    public static event System.Action OnLowBattery; // HUD can blink

    private void Update()
    {
        if (!_batterySubscribed && PhoneBattery.Instance != null)
        {
            _lastPercent = PhoneBattery.Instance.Percent;
            PhoneBattery.Instance.OnChanged += OnBattery;
            _batterySubscribed = true;
        }
        if (!_callSubscribed && PhoneCall.Instance != null)
        {
            PhoneCall.Instance.OnCallChanged += OnCall;
            _callSubscribed = true;
        }
    }

    private void OnBattery(float percent)
    {
        foreach (float w in _lowBatteryWarnings)
        {
            if (_lastPercent > w && percent <= w)
            {
                GameAudio.Play2D(GameAudio.Phone.Category, GameAudio.Phone.LowBattery);
                OnLowBattery?.Invoke();
                break;
            }
        }
        _lastPercent = percent;
    }

    private void OnCall(bool onCall)
    {
        if (onCall) GameAudio.Play2D(GameAudio.Phone.Category, GameAudio.Phone.Interference);
    }

    private void OnDestroy()
    {
        if (_batterySubscribed && PhoneBattery.Instance != null) PhoneBattery.Instance.OnChanged -= OnBattery;
        if (_callSubscribed && PhoneCall.Instance != null) PhoneCall.Instance.OnCallChanged -= OnCall;
    }
}
