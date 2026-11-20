using UnityEngine;

namespace Slafurry.Core.Interface
{
    /// <summary>Implemented by anything that can hear a <see cref="SoundWave"/> emission (e.g. AI sensor).</summary>
    public interface ISoundListener
    {
        /// <summary>A sound reached this listener.</summary>
        /// <param name="origin">World position of the sound source.</param>
        /// <param name="intensity">Received loudness after distance falloff (0..base intensity).</param>
        void OnSoundHeard(Vector2 origin, float intensity);
    }
}
