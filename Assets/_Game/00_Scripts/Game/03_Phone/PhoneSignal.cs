using UnityEngine;

public class PhoneSignal : MonoBehaviour
{
    [SerializeField] private PhoneData _data;
    [SerializeField] private Transform _player;

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

    private void Update()
    {
        if (PhoneSignalArea.Active.Count == 0) return;

        Transform player = ResolvePlayer();
        if (player == null) return;

        float best = 0f;
        bool covered = false;

        foreach (var area in PhoneSignalArea.Active)
        {
            if (!area.TryGetSignal(player.position, out float signal)) continue;
            if (!covered || signal > best)
            {
                best = signal;
                covered = true;
            }
        }

        SetLevel(Mathf.RoundToInt(best));
    }

    private Transform ResolvePlayer()
    {
        if (_player != null) return _player;
        if (PlayerInteract.Instance != null) return PlayerInteract.Instance.transform;
        return null;
    }

    public void SetLevel(int level)
    {
        int clamped = Mathf.Clamp(level, 0, _data.MaxSignalLevel);
        if (clamped == Level) return;

        Level = clamped;
        OnChanged?.Invoke(Level);
    }
}
