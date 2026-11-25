using UnityEngine;

/// <summary>
/// Kidnapper look: plays the sprite-sheet frames for idle / walk down / up / side from
/// KidnapperActions' velocity, and flickers while slowed. Pure presentation.
/// </summary>
public class KidnapperVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private KidnapperActions _actions;

    [Header("Frames")]
    [SerializeField] private Sprite[] _idle;
    [SerializeField] private Sprite[] _walkDown;
    [SerializeField] private Sprite[] _walkUp;
    [Tooltip("Side walk frames, drawn facing right (mirrored for left).")]
    [SerializeField] private Sprite[] _walkSide;
    [SerializeField] private float _framesPerSecond = 8f;

    [Header("Slowed")]
    [SerializeField] private Color _slowedTint = new Color(0.55f, 0.6f, 1f, 1f);
    [SerializeField] private float _flickerRate = 10f;

    private float _time;

    private void Awake()
    {
        if (_renderer == null) _renderer = GetComponentInChildren<SpriteRenderer>();
        if (_actions == null) _actions = GetComponentInParent<KidnapperActions>();
    }

    private void LateUpdate()
    {
        if (_renderer == null || _actions == null) return;

        Vector2 v = _actions.Velocity;
        bool moving = v.sqrMagnitude > 0.05f;
        Vector2 face = moving ? v : _actions.Facing;

        Sprite[] frames;
        bool flip = false;
        if (!moving) frames = _idle;
        else if (Mathf.Abs(face.x) > Mathf.Abs(face.y)) { frames = _walkSide; flip = face.x < 0f; }
        else frames = face.y > 0f ? _walkUp : _walkDown;
        if (frames == null || frames.Length == 0) frames = _idle;

        _time += Time.deltaTime * (moving ? Mathf.Clamp(v.magnitude / 4f, 0.75f, 2f) : 1f);
        if (frames != null && frames.Length > 0)
            _renderer.sprite = frames[(int)(_time * _framesPerSecond) % frames.Length];
        _renderer.flipX = flip;

        _renderer.color = _actions.IsSlowed && Mathf.Repeat(Time.time * _flickerRate, 1f) < 0.5f
            ? _slowedTint
            : Color.white;
    }
}
