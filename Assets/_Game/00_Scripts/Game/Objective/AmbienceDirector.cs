using UnityEngine;

/// <summary>
/// Night ambience for the house: quiet looping night + crickets, and now and then a one-shot
/// (owl, creaking wood/door, room tone) somewhere around the player.
/// </summary>
public class AmbienceDirector : MonoBehaviour
{
    [SerializeField] private Vector2 _interval = new Vector2(9f, 20f);
    [SerializeField] private Vector2 _distance = new Vector2(4f, 10f);

    private static readonly string[] OneShots =
    {
        GameAudio.Ambience.Owl, GameAudio.Ambience.WoodCreak, GameAudio.Ambience.DoorCreak, GameAudio.Ambience.Room,
    };

    private float _next;

    private void Start()
    {
        GameAudio.Play2D(GameAudio.Ambience.Category, GameAudio.Ambience.Night, loop: true, fadeIn: 3f);
        GameAudio.Play2D(GameAudio.Ambience.Category, GameAudio.Ambience.Crickets, loop: true, fadeIn: 4f);
        _next = Time.time + Random.Range(_interval.x, _interval.y) * 0.5f;
    }

    private void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + Random.Range(_interval.x, _interval.y);

        Vector2 center = PlayerInteract.Instance != null ? (Vector2)PlayerInteract.Instance.transform.position : Vector2.zero;
        Vector2 at = center + Random.insideUnitCircle.normalized * Random.Range(_distance.x, _distance.y);
        GameAudio.Play3D(GameAudio.Ambience.Category, OneShots[Random.Range(0, OneShots.Length)], at);
    }

    private void OnDestroy()
    {
        GameAudio.Stop(GameAudio.Ambience.Category, GameAudio.Ambience.Night, 1f);
        GameAudio.Stop(GameAudio.Ambience.Category, GameAudio.Ambience.Crickets, 1f);
    }
}
