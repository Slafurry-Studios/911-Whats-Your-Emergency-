using UnityEngine;
using Slafurry.Systems.InputHub;

public class PhoneVolume : MonoBehaviour
{
    [SerializeField] private PhoneData _data;

    public static PhoneVolume Instance { get; private set; }

    public float Volume { get; private set; }

    public event System.Action<float> OnChanged;

    private bool _inputSubscribed;

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
        Volume = Mathf.Clamp01(_data.StartVolume);
        OnChanged?.Invoke(Volume);
    }

    private void Update()
    {
        if (_inputSubscribed) return;
        if (InputHub.Instance == null) return;

        Controls.OnVolIncPressed += OnVolInc;
        Controls.OnVolDecPressed += OnVolDec;
        _inputSubscribed = true;
    }

    private void OnVolInc()
    {
        if (!Controls.IsInputEnabled) return;
        if (!CanUsePhone()) return;

        SetVolume(Volume + _data.VolumeStep);
    }

    private void OnVolDec()
    {
        if (!Controls.IsInputEnabled) return;
        if (!CanUsePhone()) return;

        SetVolume(Volume - _data.VolumeStep);
    }

    private static bool CanUsePhone() =>
        PhonePresence.Instance == null || PhonePresence.Instance.HasPhone;

    public void SetVolume(float volume)
    {
        float clamped = Mathf.Clamp01(volume);
        if (Mathf.Approximately(clamped, Volume)) return;

        Volume = clamped;
        OnChanged?.Invoke(Volume);
    }

    private void OnDestroy()
    {
        if (!_inputSubscribed) return;

        Controls.OnVolIncPressed -= OnVolInc;
        Controls.OnVolDecPressed -= OnVolDec;
        _inputSubscribed = false;

        if (Instance == this) Instance = null;
    }
}
