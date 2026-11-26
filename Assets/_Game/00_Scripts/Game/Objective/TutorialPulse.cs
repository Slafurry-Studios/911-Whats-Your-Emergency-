using UnityEngine;

/// <summary>
/// Makes something gently grow/shrink to draw the eye (tutorial highlight). Works on world
/// objects and UI. Restores the original scale when removed.
/// </summary>
public class TutorialPulse : MonoBehaviour
{
    [SerializeField] private float _amount = 0.15f;
    [SerializeField] private float _speed = 5f;

    private Vector3 _baseScale;

    private void OnEnable() => _baseScale = transform.localScale;

    private void Update()
    {
        float k = 1f + _amount * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * _speed));
        transform.localScale = _baseScale * k;
    }

    private void OnDisable() => transform.localScale = _baseScale;

    public static void On(Component target)
    {
        if (target != null && target.GetComponent<TutorialPulse>() == null)
            target.gameObject.AddComponent<TutorialPulse>();
    }

    public static void Off(Component target)
    {
        if (target == null) return;
        var p = target.GetComponent<TutorialPulse>();
        if (p != null) Destroy(p);
    }
}
