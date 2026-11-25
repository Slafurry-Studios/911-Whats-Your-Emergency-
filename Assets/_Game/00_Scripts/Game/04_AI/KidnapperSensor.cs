using UnityEngine;
using Slafurry.Core.Interface;

/// <summary>
/// Kidnapper SENSOR — what the kidnapper knows. Sees the player (range, view cone, walls),
/// remembers where he last saw them, notices them hiding, and hears SoundWave noises.
/// Makes no decisions; KidnapperBrain reads this every frame.
/// </summary>
public class KidnapperSensor : MonoBehaviour, ISoundListener
{
    [Header("Sight")]
    [SerializeField] private float _viewDistance = 9f;
    [SerializeField] private float _viewDistanceAggro = 14f;
    [SerializeField, Range(10f, 360f)] private float _viewAngle = 140f;
    [Tooltip("Always notices the player this close, even behind him.")]
    [SerializeField] private float _closeRadius = 2.5f;
    [Tooltip("Layers that block line of sight (walls, props, closed doors).")]
    [SerializeField] private LayerMask _sightBlockers;

    [Header("Hearing")]
    [Tooltip("Minimum received loudness to notice a sound (calm → fully aggressive).")]
    [SerializeField] private float _hearingThreshold = 0.35f;
    [SerializeField] private float _hearingThresholdAggro = 0.1f;
    [SerializeField] private float _noiseMemory = 8f;

    [Header("References (auto if empty)")]
    [SerializeField] private Transform _player;

    private KidnapperActions _actions;
    private bool _sawPlayerLastFrame;

    /// <summary>0 (calm) .. 1 (police almost here).</summary>
    public float Aggression => PoliceArrival.Instance != null ? PoliceArrival.Instance.Progress : 0f;

    public Transform Player => _player;
    public bool SeesPlayer { get; private set; }
    public Vector2 LastSeenPosition { get; private set; }
    public float LastSeenTime { get; private set; } = float.NegativeInfinity;
    public float TimeSinceSeen => Time.time - LastSeenTime;

    /// <summary>Hide spot the player was SEEN entering (null if not).</summary>
    public HideArea SeenHidingIn { get; private set; }

    public bool HasNoise => Time.time - NoiseTime <= _noiseMemory;
    public Vector2 NoisePosition { get; private set; }
    public float NoiseTime { get; private set; } = float.NegativeInfinity;

    public float ViewDistance => Mathf.Lerp(_viewDistance, _viewDistanceAggro, Aggression);

    private void Awake() => _actions = GetComponent<KidnapperActions>();

    private void Update()
    {
        if (_player == null && PlayerInteract.Instance != null) _player = PlayerInteract.Instance.transform;
        if (_player == null) { SeesPlayer = false; return; }

        bool hidden = PlayerHide.Instance != null && PlayerHide.Instance.IsHidden;

        // Watched the player slip into a hide spot.
        if (hidden && _sawPlayerLastFrame && PlayerHide.Instance.CurrentArea != null)
            SeenHidingIn = PlayerHide.Instance.CurrentArea;
        if (SeenHidingIn != null && (SeenHidingIn.IsBroken || !hidden)) SeenHidingIn = null;

        SeesPlayer = !hidden && CanSee(_player.position);
        if (SeesPlayer)
        {
            LastSeenPosition = _player.position;
            LastSeenTime = Time.time;
        }
        _sawPlayerLastFrame = SeesPlayer;
    }

    private bool CanSee(Vector2 target)
    {
        Vector2 eye = transform.position;
        Vector2 to = target - eye;
        float dist = to.magnitude;
        if (dist > ViewDistance) return false;

        if (dist > _closeRadius && _actions != null && _viewAngle < 360f)
        {
            float angle = Vector2.Angle(_actions.Facing, to);
            if (angle > _viewAngle * 0.5f) return false;
        }

        return !Blocked(eye, target);
    }

    private bool Blocked(Vector2 from, Vector2 to)
    {
        var hits = Physics2D.LinecastAll(from, to, _sightBlockers);
        foreach (var h in hits)
        {
            if (h.collider.isTrigger) continue;
            if (h.collider.transform.IsChildOf(transform)) continue;
            if (_player != null && h.collider.transform.IsChildOf(_player)) continue;
            if (h.collider.GetComponentInParent<InteractItem>() != null) continue; // small items don't block sight
            return true;
        }
        return false;
    }

    public void OnSoundHeard(Vector2 origin, float intensity)
    {
        float threshold = Mathf.Lerp(_hearingThreshold, _hearingThresholdAggro, Aggression);
        if (intensity < threshold) return;
        NoisePosition = origin;
        NoiseTime = Time.time;
    }

    // Brain tells the sensor when a memory has been dealt with.
    public void ForgetNoise() => NoiseTime = float.NegativeInfinity;
    public void ForgetHideSpot() => SeenHidingIn = null;
    public void ForgetPlayer() => LastSeenTime = float.NegativeInfinity;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, Application.isPlaying ? ViewDistance : _viewDistance);
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, _closeRadius);
    }
}
