using UnityEngine;
using Slafurry.Systems.InputHub;

public class PhoneFlashlight : MonoBehaviour
{
    [SerializeField] private GameObject _light;

    public static PhoneFlashlight Instance { get; private set; }

    public bool IsOn { get; private set; }

    public event System.Action<bool> OnChanged;

    private bool _inputSubscribed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_light != null) _light.SetActive(false);
    }

    private void Update()
    {
        if (_inputSubscribed) return;
        if (InputHub.Instance == null) return;

        Controls.OnFlashlightPressed += Toggle;
        _inputSubscribed = true;
    }

    private void Toggle()
    {
        if (!Controls.IsInputEnabled) return;
        if (!CanUsePhone()) return;

        if (!IsOn && PhoneBattery.Instance != null && PhoneBattery.Instance.Percent <= 0f)
            return;

        SetState(!IsOn);
    }

    private void SetState(bool on)
    {
        if (IsOn == on) return;

        IsOn = on;
        if (_light != null) _light.SetActive(on);
        OnChanged?.Invoke(on);
    }

    private static bool CanUsePhone() =>
        PhonePresence.Instance == null || PhonePresence.Instance.HasPhone;

    private void OnDestroy()
    {
        if (_inputSubscribed)
        {
            Controls.OnFlashlightPressed -= Toggle;
            _inputSubscribed = false;
        }

        if (Instance == this) Instance = null;
    }
}
