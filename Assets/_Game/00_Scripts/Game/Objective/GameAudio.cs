using UnityEngine;
using Slafurry.Systems.Audio;

/// <summary>
/// Names of the SFX categories/effects and music tracks set up in Main SFX / Music Data,
/// plus a helper that plays 3D sounds on the listener's z plane (the camera sits at z=-10,
/// which would otherwise add 10 units to every distance in this top-down game).
/// </summary>
public static class GameAudio
{
    public static class Kidnapper
    {
        public const string Category = "Kidnapper";
        public const string Footstep = "footstep";
        public const string DoorForce = "door_force";
        public const string Break = "break";
    }

    public static class Ambience
    {
        public const string Category = "Ambience";
        public const string Night = "night";
        public const string Crickets = "crickets";
        public const string Owl = "owl";
        public const string WoodCreak = "wood_creak";
        public const string DoorCreak = "door_creak";
        public const string Room = "room";
    }

    public static class Phone
    {
        public const string Category = "Phone";
        public const string LowBattery = "lowbat";
        public const string Interference = "interference";
    }

    public static class Police
    {
        public const string Category = "Police";
        public const string Announcer1 = "announcer1";
        public const string Announcer2 = "announcer2";
        public const string Gunshot = "gunshot";
        public static readonly string[] Reports = { "report1", "report2", "report3", "report4" };
    }

    public static class Music
    {
        public const string Explore = "Explore";
        public const string Suspense = "Suspense";
        public const string Danger = "Danger";
        public const string Stinger = "Stinger";
        public const string Ending = "Ending";
    }

    /// <summary>Play a positional sound, flattened onto the listener's z plane.</summary>
    public static void Play3D(string category, string effect, Vector2 position)
    {
        if (!Audio.IsReady) return;
        var listener = Camera.main;
        float z = listener != null ? listener.transform.position.z : 0f;
        Audio.PlaySFX3D(category, effect, new Vector3(position.x, position.y, z));
    }

    public static void Play2D(string category, string effect, bool loop = false, float fadeIn = -1f)
    {
        if (Audio.IsReady) Audio.PlaySFX2D(category, effect, loop, fadeIn);
    }

    public static void Stop(string category, string effect, float fadeOut = -1f)
    {
        if (Audio.IsReady) Audio.StopSFX(category, effect, fadeOut);
    }

    public static void PlayMusic(string track, float fade)
    {
        if (Audio.IsReady) Audio.PlayMusic(track, fade);
    }
}
