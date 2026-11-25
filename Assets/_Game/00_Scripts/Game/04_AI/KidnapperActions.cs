using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kidnapper ACTIONS — how the kidnapper does things. Walks paths from GridPathfinder
/// (forcing closed doors open on the way), gets slowed, hits the player and performs
/// timed jobs (break a hide spot, wreck an outlet). Makes no decisions: KidnapperBrain
/// calls these.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class KidnapperActions : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _repathInterval = 0.3f;
    [Tooltip("Radius used to detect a closed door on the next path segment.")]
    [SerializeField] private float _bodyRadius = 0.35f;

    [Header("Doors")]
    [SerializeField] private float _doorForceTime = 0.9f;

    [Header("Attack")]
    [SerializeField] private float _attackRange = 1.4f;

    private Rigidbody2D _rb;
    private readonly List<Vector2> _path = new();
    private int _index;
    private Vector2 _goal;
    private bool _hasGoal;
    private float _nextRepath;
    private float _speed;

    private float _slowUntil = float.NegativeInfinity;
    private float _slowMultiplier = 1f;

    private float _busyUntil;
    private System.Action _onBusyDone;

    public Vector2 Position => _rb != null ? _rb.position : (Vector2)transform.position;
    public Vector2 Facing { get; private set; } = Vector2.down;
    public Vector2 Velocity { get; private set; }
    public bool IsSlowed => Time.time < _slowUntil;
    public bool IsBusy => _onBusyDone != null;
    public string BusyWith { get; private set; }
    public bool HasPath => _hasGoal && _index < _path.Count;

    /// <summary>Speed multiplier from external sources (e.g. Brain tuning). 1 = normal.</summary>
    public float SpeedScale { get; set; } = 1f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // ------------------------------------------------------------------ movement
    /// <summary>Walk toward goal at speed. Returns true once within arriveDistance.</summary>
    public bool MoveTo(Vector2 goal, float speed, float arriveDistance = 0.3f)
    {
        if (IsBusy) return false;
        if (Vector2.Distance(Position, goal) <= arriveDistance)
        {
            Stop();
            return true;
        }

        _speed = speed;
        if (!_hasGoal || Time.time >= _nextRepath || Vector2.Distance(goal, _goal) > 0.75f)
            Repath(goal);
        return false;
    }

    private void Repath(Vector2 goal)
    {
        _goal = goal;
        _hasGoal = true;
        _nextRepath = Time.time + _repathInterval;
        _index = 0;
        var finder = GridPathfinder.Instance;
        if (finder == null || !finder.FindPath(Position, goal, _path))
            _path.Clear(); // unreachable: stand still rather than walk through walls
    }

    public void Stop()
    {
        _hasGoal = false;
        _path.Clear();
        _index = 0;
        Velocity = Vector2.zero;
    }

    public void FaceTowards(Vector2 point)
    {
        Vector2 d = point - Position;
        if (d.sqrMagnitude > 0.0001f) Facing = d.normalized;
    }

    // ------------------------------------------------------------------ status
    /// <summary>Move at multiplier speed for duration (stacks by taking the longer one).</summary>
    public void ApplySlow(float duration, float multiplier)
    {
        _slowUntil = Mathf.Max(_slowUntil, Time.time + duration);
        _slowMultiplier = Mathf.Clamp01(multiplier);
    }

    // ------------------------------------------------------------------ timed jobs
    /// <summary>Stand still for seconds, then run done (break a spot, force a door, look around).</summary>
    public void DoAfter(float seconds, string what, System.Action done)
    {
        Stop();
        _busyUntil = Time.time + Mathf.Max(0f, seconds);
        _onBusyDone = done ?? (() => { });
        BusyWith = what;
    }

    public void CancelBusy()
    {
        _onBusyDone = null;
        BusyWith = null;
    }

    // ------------------------------------------------------------------ attack
    public bool InAttackRange(Vector2 target) => Vector2.Distance(Position, target) <= _attackRange;

    public void Strike(PlayerHealth target, float damage)
    {
        if (target == null) return;
        FaceTowards(target.transform.position);
        target.Damage(damage);
    }

    // ------------------------------------------------------------------ tick
    private void Update()
    {
        if (_onBusyDone != null && Time.time >= _busyUntil)
        {
            var done = _onBusyDone;
            _onBusyDone = null;
            BusyWith = null;
            done();
        }
    }

    private void FixedUpdate()
    {
        if (IsBusy || !HasPath)
        {
            Velocity = Vector2.zero;
            return;
        }

        Vector2 pos = Position;

        // A closed door on the way: stop and force it open.
        var door = ClosedDoorBetween(pos, _path[_index]);
        if (door != null)
        {
            FaceTowards(door.transform.position);
            DoAfter(_doorForceTime, "forcing door", door.ForceOpen);
            return;
        }

        float step = _speed * SpeedScale * (IsSlowed ? _slowMultiplier : 1f) * Time.fixedDeltaTime;
        while (step > 0f && _index < _path.Count)
        {
            Vector2 to = _path[_index] - pos;
            float d = to.magnitude;
            if (d <= step)
            {
                pos = _path[_index];
                step -= d;
                _index++;
            }
            else
            {
                pos += to / d * step;
                step = 0f;
            }
        }

        Velocity = (pos - Position) / Time.fixedDeltaTime;
        if (Velocity.sqrMagnitude > 0.01f) Facing = Velocity.normalized;
        _rb.MovePosition(pos);
    }

    private Door ClosedDoorBetween(Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.0001f) return null;
        foreach (var hit in Physics2D.CircleCastAll(from, _bodyRadius, dir / dist, dist + _bodyRadius))
        {
            if (hit.collider.isTrigger) continue;
            var door = hit.collider.GetComponentInParent<Door>();
            if (door != null && !door.IsOpen) return door;
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!HasPath) return;
        Gizmos.color = Color.red;
        Vector2 prev = Position;
        for (int i = _index; i < _path.Count; i++)
        {
            Gizmos.DrawLine(prev, _path[i]);
            prev = _path[i];
        }
    }
}
