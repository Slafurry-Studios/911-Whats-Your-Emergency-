using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Police progress as sound: dispatcher when the call connects, a quiet radio loop during the
/// call, a radio report at each milestone, and a siren that fades in and grows as they get close.
/// The two loops use their own AudioSources (volume changes over time); one-shots go through
/// the SFX player.
/// </summary>
public class PoliceRadio : MonoBehaviour
{
    [SerializeField] private AudioClip _radioLoop;
    [SerializeField] private AudioClip _sirenLoop;
    [SerializeField] private AudioMixerGroup _output;

    [Header("Levels")]
    [SerializeField, Range(0f, 1f)] private float _radioVolume = 0.12f;
    [SerializeField, Range(0f, 1f)] private float _sirenMaxVolume = 0.8f;
    [Tooltip("Progress where the siren starts to be heard.")]
    [SerializeField, Range(0f, 1f)] private float _sirenFrom = 0.5f;

    [Header("Milestones (progress → report 1..4), then the announcer near the end")]
    [SerializeField] private float[] _reportAt = { 0.2f, 0.4f, 0.6f, 0.8f };
    [SerializeField] private float _finalAnnouncerAt = 0.95f;

    public static event System.Action<int> OnMilestone; // HUD pulse (index, 4 = final)

    private AudioSource _radio;
    private AudioSource _siren;
    private int _nextReport;
    private bool _finalPlayed;
    private bool _subscribed;

    private void Awake()
    {
        _radio = MakeSource("RadioLoop", _radioLoop);
        _siren = MakeSource("SirenLoop", _sirenLoop);
    }

    private AudioSource MakeSource(string name, AudioClip clip)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var s = go.AddComponent<AudioSource>();
        s.clip = clip;
        s.loop = true;
        s.playOnAwake = false;
        s.spatialBlend = 0f;
        s.volume = 0f;
        s.outputAudioMixerGroup = _output;
        return s;
    }

    private void Update()
    {
        var police = PoliceArrival.Instance;
        if (police == null) return;

        if (!_subscribed)
        {
            police.OnCallStarted += OnCallStarted;
            _subscribed = true;
            if (police.CallStarted) OnCallStarted();
        }
        if (!police.CallStarted) return;

        float p = police.Progress;
        while (_nextReport < _reportAt.Length && p >= _reportAt[_nextReport])
        {
            GameAudio.Play2D(GameAudio.Police.Category, GameAudio.Police.Reports[_nextReport]);
            OnMilestone?.Invoke(_nextReport);
            _nextReport++;
        }
        if (!_finalPlayed && p >= _finalAnnouncerAt)
        {
            _finalPlayed = true;
            GameAudio.Play2D(GameAudio.Police.Category, GameAudio.Police.Announcer2);
            OnMilestone?.Invoke(_reportAt.Length);
        }

        // Siren grows from _sirenFrom to arrival.
        float t = Mathf.InverseLerp(_sirenFrom, 1f, p);
        if (t > 0f && !_siren.isPlaying && _siren.clip != null) _siren.Play();
        _siren.volume = Mathf.MoveTowards(_siren.volume, t * t * _sirenMaxVolume, Time.deltaTime * 0.5f);

        // Radio crackle stays under everything; it dies with the battery.
        bool batteryDead = PhoneBattery.Instance != null && PhoneBattery.Instance.Percent <= 0f;
        _radio.volume = Mathf.MoveTowards(_radio.volume, batteryDead ? 0f : _radioVolume, Time.deltaTime);
    }

    private void OnCallStarted()
    {
        GameAudio.Play2D(GameAudio.Police.Category, GameAudio.Police.Announcer1);
        if (_radio.clip != null && !_radio.isPlaying) _radio.Play();
    }

    private void OnDestroy()
    {
        if (_subscribed && PoliceArrival.Instance != null) PoliceArrival.Instance.OnCallStarted -= OnCallStarted;
    }
}
