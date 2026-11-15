using UnityEngine;
using Slafurry.Systems.InputHub;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerStatsData _data;

    private Vector2 _moveInput;
    private bool _subscribed;
    private bool _isSprinting;

    private void Update()
    {
        if (!_subscribed && InputHub.Instance != null)
        {
            Controls.OnMoveChanged += OnMoveChanged;
            Controls.OnSprintStarted += OnSprintStarted;
            Controls.OnSprintCanceled += OnSprintCanceled;
            _subscribed = true;
        }

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

        Vector3 move = new Vector3(_moveInput.x, _moveInput.y, 0f);
        transform.position += move * (speed * Time.deltaTime);
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
