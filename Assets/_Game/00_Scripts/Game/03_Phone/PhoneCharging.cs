using UnityEngine;

public class PhoneCharging : MonoBehaviour
{
    [SerializeField] private PhoneData _data;

    public static PhoneCharging Instance { get; private set; }

    public bool IsCharging { get; private set; }
    public bool IsConnected { get; private set; }
    public float Ampere { get; private set; }
    public Vector2 BestDirection { get; private set; } = Vector2.up;
    public PhoneData Data => _data;

    public event System.Action<bool> OnSessionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (!IsCharging || !IsConnected) return;
        if (Ampere <= 0f) return;

        var battery = PhoneBattery.Instance;
        if (battery == null || battery.Percent >= 100f) return;

        battery.SetPercent(battery.Percent + Ampere * _data.ChargeRatePerAmpere * Time.deltaTime);
    }

    public void Begin()
    {
        if (IsCharging) return;

        IsCharging = true;
        OnSessionChanged?.Invoke(true);
    }

    public void End()
    {
        if (!IsCharging) return;

        Disconnect();
        IsCharging = false;
        OnSessionChanged?.Invoke(false);
    }

    public void Connect()
    {
        if (IsConnected) return;

        IsConnected = true;
        BestDirection = RandomDirection();
        Ampere = 0f;
    }

    public void Disconnect()
    {
        IsConnected = false;
        Ampere = 0f;
    }

    public void UpdateAmpere(Vector2 offsetFromSlot)
    {
        if (!IsConnected)
        {
            Ampere = 0f;
            return;
        }

        if (offsetFromSlot.sqrMagnitude < 0.0001f)
        {
            Ampere = 0f;
            return;
        }

        float dot = Vector2.Dot(offsetFromSlot.normalized, BestDirection);
        float t = (dot + 1f) * 0.5f;
        Ampere = t * _data.MaxAmpere;
    }

    private static Vector2 RandomDirection()
    {
        Vector2 dir = Random.insideUnitCircle;
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.up;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
