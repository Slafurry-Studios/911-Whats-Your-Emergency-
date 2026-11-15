using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] private PlayerStatsData _data;

    public static PlayerStamina Instance { get; private set; }

    public float Current { get; private set; }
    public float Max => _data.MaxStamina;

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
        Current = _data.StartStamina;
        OnChanged?.Invoke(Current, Max);
    }

    private void Update()
    {
        if (_data.StaminaRegenPerSecond <= 0f) return;
        if (Current >= Max) return;

        Regen(_data.StaminaRegenPerSecond * Time.deltaTime);
    }

    public bool TryDrain(float amount)
    {
        if (amount <= 0f) return false;
        if (Current < amount) return false;

        float previous = Current;
        Current -= amount;

        if (Current != previous)
            OnChanged?.Invoke(Current, Max);

        if (Current <= 0f)
            OnDepleted?.Invoke();

        return true;
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
