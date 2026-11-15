using UnityEngine;

public class PlayerSanity : MonoBehaviour
{
    [SerializeField] private PlayerStatsData _data;

    public static PlayerSanity Instance { get; private set; }

    public float Current { get; private set; }
    public float Max => _data.MaxSanity;

    public event System.Action<float, float> OnChanged;
    public event System.Action OnDepleted;

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
        Current = _data.StartSanity;
        OnChanged?.Invoke(Current, Max);
    }

    private void Update()
    {
        if (_data.SanityRegenPerSecond <= 0f) return;
        if (Current >= Max) return;

        Regen(_data.SanityRegenPerSecond * Time.deltaTime);
    }

    public void Damage(float amount)
    {
        if (amount <= 0f) return;

        float previous = Current;
        Current = Mathf.Max(Current - amount, 0f);

        if (Current != previous)
            OnChanged?.Invoke(Current, Max);

        if (Current <= 0f)
            OnDepleted?.Invoke();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        float previous = Current;
        Current = Mathf.Min(Current + amount, Max);

        if (Current != previous)
            OnChanged?.Invoke(Current, Max);
    }

    public void Regen(float amount)
    {
        if (amount <= 0f) return;

        float previous = Current;
        Current = Mathf.Min(Current + amount, Max);

        if (Current != previous)
            OnChanged?.Invoke(Current, Max);
    }
}
