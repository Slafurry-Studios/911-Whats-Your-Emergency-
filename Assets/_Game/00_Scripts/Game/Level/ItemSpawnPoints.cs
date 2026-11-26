using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Puts the phone and the charging cable on random spawn points (children of this object)
/// at the start of each round, in two different rooms and away from the kidnapper.
/// </summary>
public class ItemSpawnPoints : MonoBehaviour
{
    [SerializeField] private Transform _phone;
    [SerializeField] private Transform _cable;
    [SerializeField] private Transform _kidnapper;
    [Tooltip("The phone never spawns closer than this to the kidnapper.")]
    [SerializeField] private float _minPhoneDistanceToKidnapper = 14f;
    [SerializeField] private float _minPhoneToCable = 10f;
    [Tooltip("-1 = random every round; any other value = repeatable layout (tests).")]
    [SerializeField] private int _seed = -1;

    private void Awake()
    {
        var points = new List<Transform>();
        foreach (Transform child in transform) points.Add(child);
        if (points.Count < 2 || _phone == null || _cable == null) return;

        var rng = _seed >= 0 ? new System.Random(_seed) : new System.Random();

        var phoneOptions = points.FindAll(p =>
            _kidnapper == null || Vector2.Distance(p.position, _kidnapper.position) >= _minPhoneDistanceToKidnapper);
        if (phoneOptions.Count == 0) phoneOptions = points;
        var phonePoint = phoneOptions[rng.Next(phoneOptions.Count)];

        var cableOptions = points.FindAll(p => p != phonePoint &&
            Vector2.Distance(p.position, phonePoint.position) >= _minPhoneToCable);
        if (cableOptions.Count == 0) cableOptions = points.FindAll(p => p != phonePoint);
        var cablePoint = cableOptions[rng.Next(cableOptions.Count)];

        _phone.position = phonePoint.position;
        _cable.position = cablePoint.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
        foreach (Transform child in transform) Gizmos.DrawWireSphere(child.position, 0.4f);
    }
}
