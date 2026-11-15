using UnityEngine;

public class PhoneSignal : MonoBehaviour
{
    [SerializeField] private PhoneData _data;

    public static PhoneSignal Instance { get; private set; }

    public int Level { get; private set; }
    public int MaxLevel => _data.MaxSignalLevel;

    public event System.Action<int> OnChanged;

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
        Level = Mathf.Clamp(_data.StartSignalLevel, 0, _data.MaxSignalLevel);
        OnChanged?.Invoke(Level);
    }

    public void SetLevel(int level)
    {
        int clamped = Mathf.Clamp(level, 0, _data.MaxSignalLevel);
        if (clamped == Level) return;

        Level = clamped;
        OnChanged?.Invoke(Level);
    }
}
