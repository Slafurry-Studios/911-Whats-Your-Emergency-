using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData", menuName = "Game/Player/Stats Data")]
public class PlayerStatsData : ScriptableObject
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float startHealth = 100f;
    [SerializeField] private float healthRegenPerSecond = 0f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float startStamina = 100f;
    [SerializeField] private float staminaRegenPerSecond = 5f;

    [Header("Sanity")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float startSanity = 100f;
    [SerializeField] private float sanityRegenPerSecond = 0f;

    [Header("Sprint")]
    [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float staminaDrainPerSecond = 10f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    public float MaxHealth => maxHealth;
    public float StartHealth => startHealth;
    public float HealthRegenPerSecond => healthRegenPerSecond;
    public float MaxStamina => maxStamina;
    public float StartStamina => startStamina;
    public float StaminaRegenPerSecond => staminaRegenPerSecond;
    public float MaxSanity => maxSanity;
    public float StartSanity => startSanity;
    public float SanityRegenPerSecond => sanityRegenPerSecond;
    public float SprintSpeedMultiplier => sprintSpeedMultiplier;
    public float StaminaDrainPerSecond => staminaDrainPerSecond;
    public float MoveSpeed => moveSpeed;
}
