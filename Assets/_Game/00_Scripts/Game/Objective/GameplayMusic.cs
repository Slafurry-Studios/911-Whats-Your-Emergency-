using UnityEngine;

/// <summary>
/// Picks the gameplay music from the situation:
/// Explore (calm) → Suspense (911 called, or he is searching / investigating) → Danger (chase).
/// Danger is held for a few seconds after he loses sight so the music doesn't flicker.
/// </summary>
public class GameplayMusic : MonoBehaviour
{
    [SerializeField] private KidnapperBrain _kidnapper;
    [SerializeField] private float _dangerHold = 6f;
    [SerializeField] private float _fadeToDanger = 0.6f;
    [SerializeField] private float _fadeCalm = 2f;

    private string _current;
    private float _lastChase = float.NegativeInfinity;

    private void Start() => Play(GameAudio.Music.Explore, _fadeCalm);

    private void Update()
    {
        if (GameOutcome.IsFinished) return;
        if (_kidnapper == null) _kidnapper = FindObjectOfType<KidnapperBrain>();

        bool chasing = _kidnapper != null && _kidnapper.Current == KidnapperBrain.State.Chase;
        if (chasing) _lastChase = Time.time;

        string want;
        if (Time.time - _lastChase < _dangerHold) want = GameAudio.Music.Danger;
        else if (Suspicious()) want = GameAudio.Music.Suspense;
        else want = GameAudio.Music.Explore;

        Play(want, want == GameAudio.Music.Danger ? _fadeToDanger : _fadeCalm);
    }

    private bool Suspicious()
    {
        if (PoliceArrival.Instance != null && PoliceArrival.Instance.CallStarted) return true;
        if (_kidnapper == null || !_kidnapper.IsAwake) return false;
        var s = _kidnapper.Current;
        return s == KidnapperBrain.State.Search || s == KidnapperBrain.State.Investigate ||
               s == KidnapperBrain.State.BreakHideSpot;
    }

    private void Play(string track, float fade)
    {
        if (_current == track) return;
        _current = track;
        GameAudio.PlayMusic(track, fade);
    }
}
