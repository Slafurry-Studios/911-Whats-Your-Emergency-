using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slafurry.Core.Abstract;
using Slafurry.Core.Interface;

/// <summary>
/// Scene-bound sound hub (LocalSingleton). Gameplay code calls Emit();
/// every ISoundListener inside the radius hears it — e.g. kidnapper AI sensor.
/// </summary>
public class SoundWave : LocalSingleton<SoundWave>
{
    protected override void OnSingletonAwake() { }

    public override IEnumerator Initialize() { yield return null; }
    public override void PostInitialize() { }

    /// <summary>Emit sound at a source transform.</summary>
    /// <param name="source">Sound origin.</param>
    /// <param name="radius">Hear radius (circle cast).</param>
    /// <param name="intensity">Base loudness; received intensity falls off linearly to 0 at radius edge.</param>
    public void Emit(Transform source, float radius, float intensity)
    {
        if (source == null) return;
        Emit(source.position, radius, intensity);
    }

    /// <summary>Emit sound at a world position.</summary>
    public void Emit(Vector2 position, float radius, float intensity)
    {
        if (radius <= 0f || intensity <= 0f) return;

        var hits = Physics2D.OverlapCircleAll(position, radius);
        if (hits.Length == 0) return;

        var heard = new HashSet<ISoundListener>(); // one notification per listener, not per collider

        foreach (var hit in hits)
        {
            var listener = hit.GetComponentInParent<ISoundListener>();
            if (listener == null || !heard.Add(listener)) continue;

            float dist = Vector2.Distance(position, hit.ClosestPoint(position));
            float received = intensity * Mathf.Clamp01(1f - dist / radius);
            if (received <= 0.0001f) continue;

            listener.OnSoundHeard(position, received);
        }
    }
}
