using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Slafurry.Systems.Pause;
using Slafurry.Utils.GameFeel;

/// <summary>
/// Hooks game events to the existing game-feel effects:
/// player hit → red flash + camera shake + short hit stop + player blinks while invulnerable;
/// item hits the kidnapper → pop + small shake + micro hit stop; door forced / spot smashed → shake.
/// Also drives danger: vignette tightens and reddens as the kidnapper gets close, and the
/// "gut feeling" warning shows when he is near but out of sight.
/// </summary>
public class GameFeelDirector : MonoBehaviour
{
    [Header("Effects (existing components)")]
    [SerializeField] private CameraShake _shake;
    [SerializeField] private ScreenFlash _flash;
    [SerializeField] private Blink _playerBlink;
    [SerializeField] private WorldScalePop _kidnapperPop;
    [SerializeField] private Slafurry.UI.HUD.GutFeelingHUD _gutFeeling;

    [Header("Refs")]
    [SerializeField] private KidnapperBrain _kidnapper;
    [SerializeField] private Volume _volume;

    [Header("Danger")]
    [SerializeField] private float _dangerRadius = 9f;
    [SerializeField] private float _gutFeelingRadius = 7f;
    [SerializeField] private float _gutFeelingCooldown = 18f;
    [SerializeField] private float _extraVignette = 0.25f;
    [SerializeField] private Color _dangerColor = new Color(0.35f, 0f, 0f);

    private Vignette _vignette;
    private float _baseVignette;
    private Color _baseVignetteColor;
    private float _danger;
    private float _nextGutFeeling;
    private float _lastHealth = -1f;
    private bool _hitStopping;
    private bool _subscribed;

    private void Start()
    {
        if (_volume != null && _volume.profile.TryGet(out _vignette)) // .profile = runtime copy
        {
            _baseVignette = _vignette.intensity.value;
            _baseVignetteColor = _vignette.color.value;
        }
        _nextGutFeeling = Time.time + 10f;
    }

    private void Update()
    {
        Subscribe();
        UpdateDanger();
    }

    private void Subscribe()
    {
        if (_subscribed) return;
        if (_kidnapper == null) _kidnapper = FindObjectOfType<KidnapperBrain>();
        if (_kidnapper == null || PlayerHealth.Instance == null) return;

        _kidnapper.OnHitByItem += OnKidnapperHit;
        _kidnapper.OnBrokeSomething += OnBroke;
        var actions = _kidnapper.GetComponent<KidnapperActions>();
        if (actions != null) actions.OnForcingDoor += OnDoorForced;
        PlayerHealth.Instance.OnChanged += OnHealthChanged;
        _lastHealth = PlayerHealth.Instance.Current;
        _subscribed = true;
    }

    // ------------------------------------------------------------------ events
    private void OnHealthChanged(float current, float max)
    {
        bool hurt = _lastHealth >= 0f && current < _lastHealth;
        _lastHealth = current;
        if (!hurt) return;

        if (_flash != null) _flash.PlayEffect();
        if (_shake != null) _shake.Shake(2.5f, 2f, 0.35f);
        HitStop(0.12f);
        if (_playerBlink != null) StartCoroutine(BlinkFor(1.6f));
    }

    private void OnKidnapperHit(Vector2 at)
    {
        if (_kidnapperPop != null) _kidnapperPop.PlayEffect();
        if (_shake != null) _shake.Shake(0.8f, 2f, 0.15f);
        HitStop(0.05f);
    }

    private void OnDoorForced(Door door)
    {
        if (_shake != null && IsNear(door.transform.position, 12f)) _shake.Shake(1.2f, 1.5f, 0.3f);
    }

    private void OnBroke(Vector2 at)
    {
        if (_shake != null && IsNear(at, 12f)) _shake.Shake(1f, 2f, 0.25f);
    }

    private static bool IsNear(Vector2 at, float radius) =>
        PlayerInteract.Instance != null && Vector2.Distance(PlayerInteract.Instance.transform.position, at) <= radius;

    // ------------------------------------------------------------------ danger
    private void UpdateDanger()
    {
        float target = 0f;
        var player = PlayerInteract.Instance;
        if (_kidnapper != null && _kidnapper.IsAwake && player != null && !GameOutcome.IsFinished)
        {
            float d = Vector2.Distance(_kidnapper.transform.position, player.transform.position);
            target = Mathf.Clamp01(1f - d / _dangerRadius);
            if (_kidnapper.Current == KidnapperBrain.State.Chase) target = Mathf.Max(target, 0.6f);

            bool seen = _kidnapper.Current == KidnapperBrain.State.Chase;
            if (!seen && d <= _gutFeelingRadius && Time.time >= _nextGutFeeling && _gutFeeling != null)
            {
                _gutFeeling.Show();
                _nextGutFeeling = Time.time + _gutFeelingCooldown;
            }
        }

        _danger = Mathf.MoveTowards(_danger, target, Time.deltaTime * 1.5f);
        if (_vignette != null)
        {
            _vignette.intensity.Override(_baseVignette + _extraVignette * _danger);
            _vignette.color.Override(Color.Lerp(_baseVignetteColor, _dangerColor, _danger));
        }
    }

    // ------------------------------------------------------------------ helpers
    private IEnumerator BlinkFor(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end)
        {
            _playerBlink.PlayEffect(new Color(1f, 0.35f, 0.35f, 0.6f), 0.08f);
            yield return new WaitForSecondsRealtime(0.16f);
        }
    }

    /// <summary>Freeze the game for a moment (skipped while paused).</summary>
    private void HitStop(float seconds)
    {
        if (_hitStopping || Time.timeScale <= 0f) return;
        if (PauseSystem.Instance != null && PauseSystem.Instance.IsPaused) return;
        StartCoroutine(HitStopRoutine(seconds));
    }

    private IEnumerator HitStopRoutine(float seconds)
    {
        _hitStopping = true;
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(seconds);
        // Always back to normal speed: restoring "the previous value" could resurrect another
        // effect's slow-motion (e.g. the tutorial's) after it already ended.
        if (PauseSystem.Instance == null || !PauseSystem.Instance.IsPaused) Time.timeScale = 1f;
        _hitStopping = false;
    }

    private void OnDestroy()
    {
        if (!_subscribed) return;
        if (_kidnapper != null)
        {
            _kidnapper.OnHitByItem -= OnKidnapperHit;
            _kidnapper.OnBrokeSomething -= OnBroke;
            var actions = _kidnapper.GetComponent<KidnapperActions>();
            if (actions != null) actions.OnForcingDoor -= OnDoorForced;
        }
        if (PlayerHealth.Instance != null) PlayerHealth.Instance.OnChanged -= OnHealthChanged;
    }
}
