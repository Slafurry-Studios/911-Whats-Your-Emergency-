using UnityEngine;

/// <summary>
/// How close the police are (Progress 0..1). Starts advancing once the 911 call has been
/// placed and never stops after that (hanging up doesn't matter). While the phone battery
/// is empty it keeps advancing, only slower. Fires OnArrived once at 1 (the win).
/// Progress is also the kidnapper's aggression.
/// </summary>
public class PoliceArrival : MonoBehaviour
{
    [Tooltip("Seconds from the call until the police arrive, with a charged battery.")]
    [SerializeField] private float _secondsToArrive = 180f;
    [Tooltip("Speed multiplier while the phone battery is empty.")]
    [SerializeField, Range(0f, 1f)] private float _deadBatteryMultiplier = 0.5f;

    public static PoliceArrival Instance { get; private set; }

    public float Progress { get; private set; }
    public bool CallStarted { get; private set; }
    public bool HasArrived { get; private set; }

    public event System.Action<float> OnChanged;
    public event System.Action OnCallStarted;
    public event System.Action OnArrived;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() => OnChanged?.Invoke(Progress);

    private void Update()
    {
        if (!CallStarted)
        {
            if (PhoneCall.Instance == null || !PhoneCall.Instance.IsOnCall) return;
            CallStarted = true;
            OnCallStarted?.Invoke();
        }

        if (HasArrived || _secondsToArrive <= 0f) return;

        float rate = 1f / _secondsToArrive;
        if (PhoneBattery.Instance != null && PhoneBattery.Instance.Percent <= 0f)
            rate *= _deadBatteryMultiplier;

        SetProgress(Progress + rate * Time.deltaTime);
    }

    public void SetProgress(float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (Mathf.Approximately(clamped, Progress) && clamped < 1f) return;

        Progress = clamped;
        OnChanged?.Invoke(Progress);

        if (Progress >= 1f && !HasArrived)
        {
            HasArrived = true;
            OnArrived?.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
