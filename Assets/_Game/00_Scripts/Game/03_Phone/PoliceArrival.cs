using UnityEngine;

/// <summary>
/// Tracks how close the police are (Progress 0..1). Progress only advances while the
/// player is on a 911 call, scaled by phone signal. It is kept after hanging up
/// (police already dispatched). Fires OnArrived once at 1.
/// </summary>
public class PoliceArrival : MonoBehaviour
{
    [Tooltip("Seconds of calling at full signal until the police arrive.")]
    [SerializeField] private float _secondsToArrive = 180f;
    [Tooltip("Only advance while PhoneCall.IsOnCall.")]
    [SerializeField] private bool _requireCall = true;
    [Tooltip("Multiply speed by signal level / max signal (no signal = no progress).")]
    [SerializeField] private bool _scaleBySignal = true;

    public static PoliceArrival Instance { get; private set; }

    public float Progress { get; private set; }
    public bool HasArrived { get; private set; }

    public event System.Action<float> OnChanged;
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
        if (HasArrived || _secondsToArrive <= 0f) return;
        if (_requireCall && (PhoneCall.Instance == null || !PhoneCall.Instance.IsOnCall)) return;

        float rate = 1f / _secondsToArrive;
        if (_scaleBySignal && PhoneSignal.Instance != null)
            rate *= PhoneSignal.Instance.MaxLevel > 0
                ? (float)PhoneSignal.Instance.Level / PhoneSignal.Instance.MaxLevel
                : 0f;
        if (rate <= 0f) return;

        SetProgress(Progress + rate * Time.deltaTime);
    }

    public void SetProgress(float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (Mathf.Approximately(clamped, Progress)) return;

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
