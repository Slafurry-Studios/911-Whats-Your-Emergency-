using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerStatsData _data;

    public static PlayerHealth Instance { get; private set; }

    public float Current { get; private set; }
    public float Max => _data.MaxHealth;

    public event System.Action<float, float> OnChanged;
    public event System.Action OnDied;

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
        Current = _data.StartHealth;
        OnChanged?.Invoke(Current, Max);
    }

    private void Update()
    {
        if (_data.HealthRegenPerSecond <= 0f) return;
        if (Current >= Max) return;

        Heal(_data.HealthRegenPerSecond * Time.deltaTime);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        float previous = Current;
        Current = Mathf.Min(Current + amount, Max);

        if (Current != previous)
            OnChanged?.Invoke(Current, Max);
    }

    public void Damage(float amount)
    {
        if (amount <= 0f) return;

        float previous = Current;
        Current = Mathf.Max(Current - amount, 0f);

        if (Current != previous)
            OnChanged?.Invoke(Current, Max);

        if (Current <= 0f)
            OnDied?.Invoke();
    }
}
