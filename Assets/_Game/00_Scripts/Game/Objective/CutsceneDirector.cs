using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Slafurry.Systems.Scene;
using Slafurry.Utils.UI;

/// <summary>
/// Runs a text cutscene: the UISubtitle plays its lines (its onAllComplete calls Finish),
/// timed sound cues fire along the way, optional police lights flash, and any key / click
/// skips (after a short delay). Then the next scene loads.
/// </summary>
public class CutsceneDirector : MonoBehaviour
{
    [System.Serializable]
    public struct Cue
    {
        public float time;
        public string category;
        public string effect;
        public bool loop;
    }

    [SerializeField] private SceneReference _next;
    [SerializeField] private UISubtitle _subtitle;
    [SerializeField] private Cue[] _cues;

    [Header("Skip")]
    [SerializeField] private TMP_Text _skipHint;
    [SerializeField] private float _skipDelay = 1f;

    [Header("Police lights (optional)")]
    [SerializeField] private Graphic _lightA;
    [SerializeField] private Graphic _lightB;
    [SerializeField] private float _lightsFrom = -1f; // seconds; < 0 = off
    [SerializeField] private float _lightsPeriod = 0.45f;
    [SerializeField] private float _lightsAlpha = 0.22f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup _fader; // black overlay, faded out at start / in at the end
    [SerializeField] private float _fadeTime = 0.8f;

    private float _start;
    private bool _finished;

    private void Start()
    {
        _start = Time.unscaledTime;
        if (_skipHint != null) _skipHint.alpha = 0f;
        SetLight(_lightA, 0f);
        SetLight(_lightB, 0f);
        foreach (var cue in _cues) StartCoroutine(PlayCue(cue));
        if (_fader != null) StartCoroutine(Fade(1f, 0f));
    }

    private void Update()
    {
        float t = Time.unscaledTime - _start;

        if (_skipHint != null && t > _skipDelay) _skipHint.alpha = Mathf.MoveTowards(_skipHint.alpha, 0.6f, Time.unscaledDeltaTime);
        if (t > _skipDelay && SkipPressed()) Finish();

        if (_lightsFrom >= 0f && t >= _lightsFrom)
        {
            bool a = Mathf.Repeat(t, _lightsPeriod * 2f) < _lightsPeriod;
            SetLight(_lightA, a ? _lightsAlpha : 0f);
            SetLight(_lightB, a ? 0f : _lightsAlpha);
        }
    }

    private static bool SkipPressed()
    {
        var kb = Keyboard.current;
        if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame ||
                           kb.enterKey.wasPressedThisFrame)) return true;
        var mouse = Mouse.current;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
    }

    private static void SetLight(Graphic g, float alpha)
    {
        if (g == null) return;
        var c = g.color; c.a = alpha; g.color = c;
    }

    private IEnumerator PlayCue(Cue cue)
    {
        yield return new WaitForSecondsRealtime(cue.time);
        if (!_finished) GameAudio.Play2D(cue.category, cue.effect, cue.loop);
    }

    /// <summary>Called by the subtitle's onAllComplete, or by skipping.</summary>
    public void Finish()
    {
        if (_finished) return;
        _finished = true;
        StartCoroutine(FinishRoutine());
    }

    private IEnumerator FinishRoutine()
    {
        if (_subtitle != null) _subtitle.Stop();
        if (_fader != null) yield return Fade(0f, 1f);
        foreach (var cue in _cues)
            if (cue.loop) GameAudio.Stop(cue.category, cue.effect, 1f);
        if (_next != null) SceneSystem.Load(_next);
    }

    private IEnumerator Fade(float from, float to)
    {
        for (float t = 0f; t < _fadeTime; t += Time.unscaledDeltaTime)
        {
            _fader.alpha = Mathf.Lerp(from, to, t / _fadeTime);
            yield return null;
        }
        _fader.alpha = to;
    }
}
