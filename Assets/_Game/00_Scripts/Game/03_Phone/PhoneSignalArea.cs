using System.Collections.Generic;
using UnityEngine;

public class PhoneSignalArea : MonoBehaviour
{
    public static readonly List<PhoneSignalArea> Active = new();

    [SerializeField] private float _areaSize = 5f;
    [SerializeField] private float _power = 4f;
    [SerializeField] private float _minSignal = 1f;
    [SerializeField] private float _maxSignal = 4f;

    public float AreaSize => _areaSize;

    private void OnEnable()
    {
        if (!Active.Contains(this)) Active.Add(this);
    }

    private void OnDisable() => Active.Remove(this);

    public bool TryGetSignal(Vector2 position, out float signal)
    {
        signal = 0f;

        float distance = Vector2.Distance(transform.position, position);
        if (distance > _areaSize) return false;

        float t = _areaSize > 0f ? 1f - distance / _areaSize : 1f;
        float lo = Mathf.Min(_minSignal, _maxSignal);
        float hi = Mathf.Max(_minSignal, _maxSignal);

        signal = Mathf.Clamp(_power * t, lo, hi);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _areaSize);

        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, _areaSize);
    }
}
