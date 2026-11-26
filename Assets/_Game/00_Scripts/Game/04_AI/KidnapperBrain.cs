using UnityEngine;
using Slafurry.Core.Interface;

/// <summary>
/// Kidnapper BRAIN — decides what to do each frame from what KidnapperSensor knows,
/// and tells KidnapperActions to do it. Priority:
/// chase (sees player) → break the hide spot he saw them enter → search where they were last
/// seen → investigate a noise → sabotage a hide spot / outlet (aggressive only) → wander.
/// Aggression (= police progress) makes him faster, hear better, search longer, recover
/// from hits faster and sabotage more often.
/// </summary>
[RequireComponent(typeof(KidnapperSensor), typeof(KidnapperActions))]
public class KidnapperBrain : MonoBehaviour, IThrowHittable
{
    public enum State { Wander, Investigate, Chase, Search, BreakHideSpot, Sabotage }

    [Header("Speed (player: walk 5, sprint 7.5)")]
    [SerializeField] private float _wanderSpeed = 3f;
    [SerializeField] private float _wanderSpeedAggro = 4.5f;
    [SerializeField] private float _investigateSpeed = 4.5f;
    [SerializeField] private float _investigateSpeedAggro = 6.5f;
    [Tooltip("Faster than the player's sprint, so throwing is the way out.")]
    [SerializeField] private float _chaseSpeed = 7.9f;
    [SerializeField] private float _chaseSpeedAggro = 9f;

    [Header("Hit by a thrown item")]
    [SerializeField] private float _slowDuration = 2.5f;
    [SerializeField] private float _slowDurationAggro = 1.2f;
    [SerializeField, Range(0f, 1f)] private float _slowMultiplier = 0.25f;

    [Header("Attack")]
    [Tooltip("Hits that take the player from full health to dead.")]
    [SerializeField] private int _hitsToKill = 3;
    [Tooltip("Extra wait after the post-attack slow before he can hit again.")]
    [SerializeField] private float _attackRecovery = 0.5f;

    [Header("Search / investigate")]
    [SerializeField] private float _searchTime = 6f;
    [SerializeField] private float _searchTimeAggro = 14f;
    [SerializeField] private float _searchRadius = 5f;
    [SerializeField] private float _lookAroundTime = 2f;
    [SerializeField] private float _lookAroundTimeAggro = 0.8f;

    [Header("Hide spots / sabotage")]
    [SerializeField] private float _breakTime = 1.6f;
    [SerializeField] private float _breakTimeAggro = 0.8f;
    [Tooltip("Aggression from which he starts wrecking hide spots and outlets on his own.")]
    [SerializeField, Range(0f, 1f)] private float _sabotageFromAggression = 0.4f;
    [SerializeField] private float _sabotageCooldown = 40f;
    [SerializeField] private float _sabotageCooldownAggro = 15f;

    [Header("Wake up")]
    [Tooltip("He stays still until the player picks up the phone or this many seconds pass.")]
    [SerializeField] private float _wakeAfterSeconds = 30f;

    [Header("Wander")]
    [SerializeField] private Vector2 _wanderPause = new Vector2(1f, 3f);
    [Tooltip("At full aggression he wanders within this distance of the player (he senses them).")]
    [SerializeField] private float _huntRadiusAggro = 8f;

    private KidnapperSensor _sensor;
    private KidnapperActions _actions;

    private float _nextAttack;
    private bool _reachedLastSeen;
    private Vector2? _wanderTarget;
    private Vector2? _searchTarget;
    private MonoBehaviour _sabotageTarget;
    private float _nextSabotage;

    public State Current { get; private set; } = State.Wander;
    public bool IsAwake { get; private set; }

    /// <summary>While true he stays asleep no matter what (tutorial steps before he appears).</summary>
    public static bool HoldAsleep;

    // Hooks for audio / game feel (no gameplay logic listens to these).
    public event System.Action OnWokeUp;
    public event System.Action OnSpottedPlayer;      // lost → sees the player
    public event System.Action OnAttacked;           // hit the player
    public event System.Action<Vector2> OnHitByItem; // a thrown item hit him
    public event System.Action<Vector2> OnBrokeSomething; // hide spot / outlet destroyed

    private bool _sawPlayer;
    private float _startTime;

    private float A => _sensor.Aggression;
    private float SlowDuration => Mathf.Lerp(_slowDuration, _slowDurationAggro, A);

    private void Awake()
    {
        _sensor = GetComponent<KidnapperSensor>();
        _actions = GetComponent<KidnapperActions>();
    }

    private void Start()
    {
        _startTime = Time.time;
        _nextSabotage = Time.time + _sabotageCooldown;
    }

    /// <summary>Wake him now (tutorial).</summary>
    public void WakeUp()
    {
        if (IsAwake) return;
        IsAwake = true;
        _nextSabotage = Time.time + _sabotageCooldown;
        OnWokeUp?.Invoke();
    }

    /// <summary>Start the "wake after N seconds" countdown again from now.</summary>
    public void RestartWakeTimer() => _startTime = Time.time;

    private bool CheckAwake()
    {
        if (IsAwake) return true;
        if (HoldAsleep) return false;
        bool hasPhone = PhonePresence.Instance != null && PhonePresence.Instance.HasPhone;
        if (!hasPhone && Time.time - _startTime < _wakeAfterSeconds) return false;
        IsAwake = true;
        _nextSabotage = Time.time + _sabotageCooldown;
        OnWokeUp?.Invoke();
        return true;
    }

    private void Update()
    {
        if (GameOutcome.IsFinished)
        {
            _actions.Stop();
            return;
        }
        if (!CheckAwake()) return;

        bool sees = _sensor.SeesPlayer;
        if (sees && !_sawPlayer) OnSpottedPlayer?.Invoke();
        _sawPlayer = sees;

        if (_actions.IsBusy) return;

        if (sees) { Chase(); return; }
        if (_sensor.SeenHidingIn != null) { BreakHideSpot(_sensor.SeenHidingIn); return; }
        if (_sensor.TimeSinceSeen < Mathf.Lerp(_searchTime, _searchTimeAggro, A)) { Search(); return; }
        _reachedLastSeen = false;
        if (_sensor.HasNoise) { Investigate(); return; }
        if (_sabotageTarget != null || PickSabotageTarget()) { Sabotage(); return; }
        Wander();
    }

    // ------------------------------------------------------------------ behaviours
    private void Chase()
    {
        Enter(State.Chase);
        _reachedLastSeen = false;
        Vector2 player = _sensor.Player.position;

        if (_actions.InAttackRange(player) && Time.time >= _nextAttack)
        {
            Attack();
            return;
        }
        _actions.MoveTo(player, Mathf.Lerp(_chaseSpeed, _chaseSpeedAggro, A), 0.6f);
    }

    private void Attack()
    {
        var health = PlayerHealth.Instance;
        if (health == null) return;

        float damage = health.Max / Mathf.Max(1, _hitsToKill) + 0.01f;
        _actions.Strike(health, damage);
        OnAttacked?.Invoke();

        // Like being hit by a throw: he slows down so the player can get away.
        _actions.ApplySlow(SlowDuration, _slowMultiplier);
        _nextAttack = Time.time + SlowDuration + _attackRecovery;
    }

    private void BreakHideSpot(HideArea spot)
    {
        Enter(State.BreakHideSpot);
        if (!_actions.MoveTo(spot.transform.position, Mathf.Lerp(_chaseSpeed, _chaseSpeedAggro, A), 1.2f)) return;

        _actions.FaceTowards(spot.transform.position);
        _actions.DoAfter(Mathf.Lerp(_breakTime, _breakTimeAggro, A), "breaking hide spot", () =>
        {
            spot.Break();
            OnBrokeSomething?.Invoke(spot.transform.position);
            _sensor.ForgetHideSpot();
        });
    }

    private void Search()
    {
        Enter(State.Search);
        if (!_reachedLastSeen)
        {
            if (_actions.MoveTo(_sensor.LastSeenPosition, Mathf.Lerp(_investigateSpeed, _chaseSpeedAggro, A) , 0.5f))
            {
                _reachedLastSeen = true;
                _searchTarget = null;
            }
            return;
        }

        if (_searchTarget == null && GridPathfinder.Instance != null &&
            GridPathfinder.Instance.TryRandomPoint(_sensor.LastSeenPosition, _searchRadius, out var p))
            _searchTarget = p;
        if (_searchTarget == null) return;

        if (_actions.MoveTo(_searchTarget.Value, Mathf.Lerp(_investigateSpeed, _investigateSpeedAggro, A)))
        {
            _searchTarget = null;
            _actions.DoAfter(Mathf.Lerp(_lookAroundTime, _lookAroundTimeAggro, A) * 0.5f, "looking around", null);
        }
    }

    private void Investigate()
    {
        Enter(State.Investigate);
        if (!_actions.MoveTo(_sensor.NoisePosition, Mathf.Lerp(_investigateSpeed, _investigateSpeedAggro, A), 0.6f)) return;

        _actions.DoAfter(Mathf.Lerp(_lookAroundTime, _lookAroundTimeAggro, A), "looking around", _sensor.ForgetNoise);
    }

    private bool PickSabotageTarget()
    {
        if (A < _sabotageFromAggression || Time.time < _nextSabotage) return false;
        _nextSabotage = Time.time + Mathf.Lerp(_sabotageCooldown, _sabotageCooldownAggro, A);

        // Nearest intact hide spot or outlet.
        MonoBehaviour best = null;
        float bestDist = float.MaxValue;
        Vector2 me = _actions.Position;
        foreach (var h in HideArea.All)
            if (h != null && !h.IsBroken) Consider(h, ref best, ref bestDist, me);
        foreach (var o in ChargingOutlet.All)
            if (o != null && !o.IsBroken) Consider(o, ref best, ref bestDist, me);

        _sabotageTarget = best;
        return best != null;
    }

    private static void Consider(MonoBehaviour target, ref MonoBehaviour best, ref float bestDist, Vector2 from)
    {
        float d = Vector2.Distance(from, target.transform.position);
        if (d < bestDist) { bestDist = d; best = target; }
    }

    private void Sabotage()
    {
        Enter(State.Sabotage);
        var target = _sabotageTarget;
        if (target == null || IsBroken(target)) { _sabotageTarget = null; return; }

        if (!_actions.MoveTo(target.transform.position, Mathf.Lerp(_investigateSpeed, _investigateSpeedAggro, A), 1.2f)) return;

        _actions.FaceTowards(target.transform.position);
        _actions.DoAfter(Mathf.Lerp(_breakTime, _breakTimeAggro, A), "sabotaging", () =>
        {
            if (target is HideArea h) h.Break();
            else if (target is ChargingOutlet o) o.Break();
            OnBrokeSomething?.Invoke(target.transform.position);
            _sabotageTarget = null;
        });
    }

    private static bool IsBroken(MonoBehaviour t) =>
        (t is HideArea h && h.IsBroken) || (t is ChargingOutlet o && o.IsBroken);

    private void Wander()
    {
        Enter(State.Wander);
        if (_wanderTarget == null && !PickWanderTarget()) return;

        if (_actions.MoveTo(_wanderTarget.Value, Mathf.Lerp(_wanderSpeed, _wanderSpeedAggro, A)) || !_actions.HasPath)
        {
            _wanderTarget = null;
            _actions.DoAfter(Random.Range(_wanderPause.x, _wanderPause.y) * (1f - 0.6f * A), "idle", null);
        }
    }

    private bool PickWanderTarget()
    {
        var finder = GridPathfinder.Instance;
        if (finder == null) return false;

        // The closer the police, the more he drifts toward where the player actually is.
        Vector2 center = _actions.Position;
        float radius = 0f;
        if (_sensor.Player != null && A > 0.05f)
        {
            center = _sensor.Player.position;
            radius = Mathf.Lerp(30f, _huntRadiusAggro, A);
        }
        if (!finder.TryRandomPoint(center, radius, out var p)) return false;
        _wanderTarget = p;
        return true;
    }

    private void Enter(State s)
    {
        if (Current == s) return;
        Current = s;
        if (s != State.Wander) _wanderTarget = null;
        if (s != State.Search) _searchTarget = null;
    }

    // ------------------------------------------------------------------ IThrowHittable
    public void OnHitByThrow(Vector2 hitPoint, Vector2 direction)
    {
        OnHitByItem?.Invoke(hitPoint);
        if (!IsAwake) { IsAwake = true; OnWokeUp?.Invoke(); }
        _actions.ApplySlow(SlowDuration, _slowMultiplier);
        // Turn toward where it came from — likely spots the thrower.
        if (direction.sqrMagnitude > 0.0001f)
            _actions.FaceTowards(_actions.Position - direction);
        // Interrupt slow jobs (looking around / sabotage) — but not mid-door.
        if (_actions.IsBusy && _actions.BusyWith != "forcing door") _actions.CancelBusy();
    }
}
