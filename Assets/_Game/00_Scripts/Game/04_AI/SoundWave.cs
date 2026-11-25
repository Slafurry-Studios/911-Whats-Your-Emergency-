using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slafurry.Core.Abstract;
using Slafurry.Core.Interface;

/// <summary>
/// Scene-bound sound hub (LocalSingleton). Gameplay code calls Emit();
/// every ISoundListener inside the radius hears it — e.g. kidnapper AI sensor.
/// EmitAt() is the same thing without needing a SoundWave in the scene.
/// </summary>
public class SoundWave : LocalSingleton<SoundWave>
{
    protected override void OnSingletonAwake() { }

    public override IEnumerator Initialize() { yield return null; }

    public override void PostInitialize() { }

    public void Emit(Transform source, float radius, float intensity)
    {
        if (source == null) return;
        EmitAt(source.position, radius, intensity);
    }

    public void Emit(Vector2 position, float radius, float intensity) => EmitAt(position, radius, intensity);

    /// <summary>Notify every ISoundListener within radius (loudness falls off with distance).</summary>
    public static void EmitAt(Vector2 position, float radius, float intensity)
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
