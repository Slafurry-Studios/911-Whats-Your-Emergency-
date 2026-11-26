using UnityEngine;

/// <summary>
/// Kidnapper sounds: footsteps by distance walked (faster when chasing), forcing doors,
/// smashing hide spots / outlets. Positional, so the player hears where he is.
/// </summary>
[RequireComponent(typeof(KidnapperActions))]
public class KidnapperAudio : MonoBehaviour
{
    [Tooltip("World units walked per footstep sound.")]
    [SerializeField] private float _stepDistance = 1.3f;

    private KidnapperActions _actions;
    private KidnapperBrain _brain;
    private Vector2 _lastPos;
    private float _walked;

    private void Awake()
    {
        _actions = GetComponent<KidnapperActions>();
        _brain = GetComponent<KidnapperBrain>();
    }

    private void OnEnable()
    {
        _lastPos = transform.position;
        _actions.OnForcingDoor += OnForcingDoor;
        if (_brain != null) _brain.OnBrokeSomething += OnBroke;
    }

    private void OnDisable()
    {
        _actions.OnForcingDoor -= OnForcingDoor;
        if (_brain != null) _brain.OnBrokeSomething -= OnBroke;
    }

    private void Update()
    {
        Vector2 pos = transform.position;
        _walked += Vector2.Distance(pos, _lastPos);
        _lastPos = pos;
        if (_walked < _stepDistance) return;
        _walked = 0f;
        GameAudio.Play3D(GameAudio.Kidnapper.Category, GameAudio.Kidnapper.Footstep, pos);
    }

    private void OnForcingDoor(Door door) =>
        GameAudio.Play3D(GameAudio.Kidnapper.Category, GameAudio.Kidnapper.DoorForce, door.transform.position);

    private void OnBroke(Vector2 at) =>
        GameAudio.Play3D(GameAudio.Kidnapper.Category, GameAudio.Kidnapper.Break, at);
}
