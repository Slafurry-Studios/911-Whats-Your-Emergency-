using UnityEngine;
using Slafurry.Systems.InputHub;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerStatsData _data;

    // Gap kept between the player collider and walls so casts don't start overlapping.
    private const float Skin = 0.01f;

    private Vector2 _moveInput;
    private Vector2 _velocity;
    private bool _subscribed;
    private bool _isSprinting;

    private Rigidbody2D _rb;
    private ContactFilter2D _filter;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];

    public Vector2 MoveDirection => _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb != null) _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        _filter = new ContactFilter2D { useTriggers = false };
        _filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
    }

    private void Update()
    {
        if (!_subscribed && InputHub.Instance != null)
        {
            Controls.OnMoveChanged += OnMoveChanged;
            Controls.OnSprintStarted += OnSprintStarted;
            Controls.OnSprintCanceled += OnSprintCanceled;
            _subscribed = true;
        }

        _velocity = Vector2.zero;
        if (!_subscribed || !Controls.IsInputEnabled) return;

        float speed = _data.MoveSpeed;
        if (_isSprinting)
        {
            speed *= _data.SprintSpeedMultiplier;

            var stamina = PlayerStamina.Instance;
            if (stamina != null && stamina.Current > 0f)
            {
                stamina.TryDrain(_data.StaminaDrainPerSecond * Time.deltaTime);
            }
            else
            {
                _isSprinting = false;
            }
        }

        _velocity = _moveInput * speed;

        // No Rigidbody2D: old behaviour (no collision).
        if (_rb == null)
            transform.position += (Vector3)(_velocity * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (_rb == null || _velocity == Vector2.zero) return;

        // Kinematic body: cast each axis separately so the player slides along walls.
        Vector2 delta = _velocity * Time.fixedDeltaTime;
        Vector2 step = AllowedMove(new Vector2(delta.x, 0f)) + AllowedMove(new Vector2(0f, delta.y));
        if (step != Vector2.zero)
            _rb.MovePosition(_rb.position + step);
    }

    private Vector2 AllowedMove(Vector2 delta)
    {
        float distance = delta.magnitude;
        if (distance <= 0f) return Vector2.zero;

        Vector2 dir = delta / distance;
        int count = _rb.Cast(dir, _filter, _hits, distance + Skin);
        for (int i = 0; i < count; i++)
        {
            // Already overlapping at the start (spawned touching a prop): don't let that lock
            // movement in every direction — only real hits ahead count.
            if (_hits[i].distance <= 0f) continue;
            distance = Mathf.Min(distance, _hits[i].distance - Skin);
        }

        return dir * Mathf.Max(0f, distance);
    }

    private void OnMoveChanged(Vector2 input) => _moveInput = input;

    private void OnSprintStarted() => _isSprinting = true;

    private void OnSprintCanceled() => _isSprinting = false;

    private void OnDisable()
    {
        if (!_subscribed) return;
        if (InputHub.Instance != null)
        {
            Controls.OnMoveChanged -= OnMoveChanged;
            Controls.OnSprintStarted -= OnSprintStarted;
            Controls.OnSprintCanceled -= OnSprintCanceled;
        }
        _subscribed = false;
    }
}
