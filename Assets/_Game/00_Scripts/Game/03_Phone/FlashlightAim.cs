using UnityEngine;

/// <summary>
/// Turns the phone flashlight cone (a Light2D on this GameObject, cone along local +Y)
/// toward the player's last move direction.
/// </summary>
public class FlashlightAim : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private float _turnSpeed = 720f; // degrees per second

    private float _targetAngle = 180f; // start facing down

    private void Awake()
    {
        if (_movement == null) _movement = GetComponentInParent<PlayerMovement>();
        transform.localRotation = Quaternion.Euler(0f, 0f, _targetAngle);
    }

    private void LateUpdate()
    {
        if (_movement == null) return;

        Vector2 dir = _movement.MoveDirection;
        if (dir.sqrMagnitude > 0.01f)
            _targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        float current = transform.localEulerAngles.z;
        float next = Mathf.MoveTowardsAngle(current, _targetAngle, _turnSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(0f, 0f, next);
    }
}
