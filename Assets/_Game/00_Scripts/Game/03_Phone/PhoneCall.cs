using UnityEngine;

public class PhoneCall : MonoBehaviour
{
    public static PhoneCall Instance { get; private set; }

    public bool IsOnCall { get; private set; }

    public event System.Action<bool> OnCallChanged;

    /// <summary>A call can only be placed with battery left.</summary>
    public bool CanCall => PhoneBattery.Instance == null || PhoneBattery.Instance.Percent > 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Call()
    {
        if (IsOnCall) return;
        if (!CanCall) return;

        IsOnCall = true;
        OnCallChanged?.Invoke(true);
    }

    public void HangUp()
    {
        if (!IsOnCall) return;

        IsOnCall = false;
        OnCallChanged?.Invoke(false);
    }

    public void ToggleCall()
    {
        if (IsOnCall) HangUp();
        else Call();
    }
}
