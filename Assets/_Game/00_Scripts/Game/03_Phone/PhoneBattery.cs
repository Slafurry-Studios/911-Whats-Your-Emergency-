using UnityEngine;

public class PhoneBattery : MonoBehaviour
{
    [SerializeField] private PhoneData _data;

    public static PhoneBattery Instance { get; private set; }

    public float Percent { get; private set; }

    public event System.Action<float> OnChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Percent = Mathf.Clamp(_data.StartBatteryPercent, 0f, 100f);
        OnChanged?.Invoke(Percent);
    }

    private void Update()
    {
        if (Percent <= 0f) return;
        if (_data.BatteryDrainPerSecond <= 0f) return;
        // The phone only drains once the player actually has it.
        if (PhonePresence.Instance != null && !PhonePresence.Instance.HasPhone) return;

        float drain = _data.BatteryDrainPerSecond;
        if (PlayerHide.Instance != null && PlayerHide.Instance.IsHidden)
            drain *= _data.HiddenDrainMultiplier;

        SetPercent(Percent - drain * Time.deltaTime);
    }

    public void SetPercent(float percent)
    {
        float clamped = Mathf.Clamp(percent, 0f, 100f);
        if (Mathf.Approximately(clamped, Percent)) return;

        Percent = clamped;
        OnChanged?.Invoke(Percent);
    }
}
