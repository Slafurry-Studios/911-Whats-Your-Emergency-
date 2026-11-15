using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhoneData", menuName = "Game/Phone Data")]
public class PhoneData : ScriptableObject
{
    [Header("Signal")]
    [SerializeField] private int startSignalLevel = 4;
    [SerializeField] private int maxSignalLevel = 4;

    [Header("Battery")]
    [SerializeField] private float startBatteryPercent = 100f;
    [SerializeField] private float batteryDrainPerSecond = 0.5f;
    [SerializeField, Range(0f, 1f)] private float hiddenDrainMultiplier = 0.25f;

    [Header("Volume")]
    [SerializeField] private float startVolume = 0.5f;
    [SerializeField] private float volumeStep = 0.1f;

    [Header("Charging")]
    [SerializeField] private float maxAmpere = 2f;
    [SerializeField] private float chargeRatePerAmpere = 2f;
    [SerializeField] private float cableSnapRadius = 40f;

    [Header("Presence")]
    [SerializeField] private List<ItemData> phoneItems = new();

    public int StartSignalLevel => startSignalLevel;
    public int MaxSignalLevel => maxSignalLevel;
    public float StartBatteryPercent => startBatteryPercent;
    public float BatteryDrainPerSecond => batteryDrainPerSecond;
    public float HiddenDrainMultiplier => hiddenDrainMultiplier;
    public float StartVolume => startVolume;
    public float VolumeStep => volumeStep;
    public float MaxAmpere => maxAmpere;
    public float ChargeRatePerAmpere => chargeRatePerAmpere;
    public float CableSnapRadius => cableSnapRadius;
    public IReadOnlyList<ItemData> PhoneItems => phoneItems;
}
