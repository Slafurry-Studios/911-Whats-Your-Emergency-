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

    [Header("Volume")]
    [SerializeField] private float startVolume = 0.5f;

    [Header("Presence")]
    [SerializeField] private List<ItemData> phoneItems = new();

    public int StartSignalLevel => startSignalLevel;
    public int MaxSignalLevel => maxSignalLevel;
    public float StartBatteryPercent => startBatteryPercent;
    public float StartVolume => startVolume;
    public IReadOnlyList<ItemData> PhoneItems => phoneItems;
}
