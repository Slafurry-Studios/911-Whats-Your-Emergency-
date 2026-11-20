using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Slafurry.Systems.InputHub;

/// <summary>
/// Drives the Animator directly via PlayableGraph (no AnimatorController states/transitions needed).
/// Left mirrors walk right via SpriteRenderer.flipX.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private enum Slot { Idle = 0, Right = 1, Up = 2, Down = 3 }

    [Header("Animation Clips")]
    [SerializeField] private AnimationClip _idleClip;
    [SerializeField] private AnimationClip _walkRightClip; // left mirrors this (flipX)
    [SerializeField] private AnimationClip _walkUpClip;
    [SerializeField] private AnimationClip _walkDownClip;

    [Header("Settings")]
    [SerializeField] private float _crossfade = 0.1f;
    [SerializeField] private float _moveThreshold = 0.01f;

    private Animator _anim;
    private SpriteRenderer _sprite;
    private Vector2 _moveInput;
    private bool _subscribed;

    private PlayableGraph _graph;
    private AnimationMixerPlayable _mixer;
    private AnimationClipPlayable[] _playables;
    private AnimationClip[] _clips;
    private float[] _weights;
    private int _current = -1;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        BuildGraph();
    }

    private void BuildGraph()
    {
        _clips = new[] { _idleClip, _walkRightClip, _walkUpClip, _walkDownClip };
        _playables = new AnimationClipPlayable[_clips.Length];
        _weights = new float[_clips.Length];

        _graph = PlayableGraph.Create($"{name}_PlayerAnimator");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        _mixer = AnimationMixerPlayable.Create(_graph, _clips.Length);

        for (int i = 0; i < _clips.Length; i++)
        {
            if (_clips[i] == null) continue;

            _playables[i] = AnimationClipPlayable.Create(_graph, _clips[i]);
            _graph.Connect(_playables[i], 0, _mixer, i);
            _mixer.SetInputWeight(i, 0f);
        }

        var output = AnimationPlayableOutput.Create(_graph, "Animation", _anim);
        output.SetSourcePlayable(_mixer);

        _graph.Play();
    }

    private void OnEnable()
    {
        if (_graph.IsValid()) _graph.Play();
    }

    private void Update()
    {
        if (!_subscribed && InputHub.Instance != null)
        {
            Controls.OnMoveChanged += OnMoveChanged;
            _subscribed = true;
        }
    }

    private void OnMoveChanged(Vector2 input) => _moveInput = input;

    private void LateUpdate()
    {
        if (!_subscribed) return;

        bool moving = _moveInput.sqrMagnitude >= _moveThreshold * _moveThreshold;

        if (!moving)
        {
            SetState(Slot.Idle);
        }
        else if (Mathf.Abs(_moveInput.x) > Mathf.Abs(_moveInput.y))
        {
            SetState(Slot.Right);
            if (_sprite != null) _sprite.flipX = _moveInput.x < 0f;
        }
        else if (_moveInput.y > 0f)
        {
            SetState(Slot.Up);
            if (_sprite != null) _sprite.flipX = false;
        }
        else
        {
            SetState(Slot.Down);
            if (_sprite != null) _sprite.flipX = false;
        }

        UpdateWeightsAndLoop();
    }

    private void SetState(Slot slot)
    {
        int index = (int)slot;
        if (_current == index) return;
        if (_clips[index] == null) return;

        // Restart the clip if it was fully faded out.
        if (_weights[index] <= 0.001f)
            _playables[index].SetTime(0);

        _current = index;
    }

    private void UpdateWeightsAndLoop()
    {
        if (_current < 0) return;

        float step = _crossfade > 0f ? Time.deltaTime / _crossfade : 1f;
        float sum = 0f;

        for (int i = 0; i < _weights.Length; i++)
        {
            if (_clips[i] == null) continue;

            float target = i == _current ? 1f : 0f;
            _weights[i] = Mathf.MoveTowards(_weights[i], target, step);
            sum += _weights[i];

            // Manual looping so it doesn't depend on clip import settings.
            double len = _clips[i].length;
            if (len > 0 && _playables[i].GetTime() >= len)
                _playables[i].SetTime(_playables[i].GetTime() % len);
        }

        if (sum <= 0f) return;

        for (int i = 0; i < _weights.Length; i++)
        {
            if (_clips[i] == null) continue;
            _mixer.SetInputWeight(i, _weights[i] / sum);
        }
    }

    private void OnDisable()
    {
        if (_graph.IsValid()) _graph.Stop();

        if (!_subscribed) return;
        Controls.OnMoveChanged -= OnMoveChanged;
        _subscribed = false;
    }

    private void OnDestroy()
    {
        if (_graph.IsValid()) _graph.Destroy();
    }
}