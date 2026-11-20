using UnityEngine;
using UnityEngine.InputSystem;
using Slafurry.Systems.InputHub;

public class PlayerThrow : MonoBehaviour
{
    [Header("Throw")]
    [SerializeField] private float _throwSpeed = 10f;
    [SerializeField] private float _aimDistance = 1.5f;
    [SerializeField] private Color _aimColor = Color.yellow;

    [Header("Aim Line")]
    [SerializeField] private float _aimLineLength = 4f;
    [SerializeField] private Color _aimLineColor = Color.red;
    [SerializeField] private float _aimLineWidth = 0.05f;
    [SerializeField] private Material _aimLineMaterial; // optional; falls back to Sprites/Default

    private PlayerInventory _inventory;
    private PlayerMovement _movement;
    private GameObject _aimMarker;
    private LineRenderer _aimLine;
    private Vector2 _lastDir = Vector2.right;
    private bool _aiming;
    private bool _subscribed;

    private void Awake()
    {
        _inventory = GetComponent<PlayerInventory>();
        _movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // Remember last non-zero move direction (input zeroes on release).
        if (_movement != null && _movement.MoveDirection.sqrMagnitude >= 0.01f)
            _lastDir = _movement.MoveDirection.normalized;

        if (!_subscribed && InputHub.Instance != null)
        {
            Controls.OnThrowPressed += OnThrowPressed;
            _subscribed = true;
        }

        if (!_aiming || !Controls.IsInputEnabled) return;

        if (Keyboard.current != null && Keyboard.current[Key.Escape].wasPressedThisFrame)
            CancelAim();
    }

    private void OnThrowPressed()
    {
        if (!Controls.IsInputEnabled) return;

        if (!_aiming)
        {
            var item = _inventory.SelectedItem;
            if (item != null && item.IsThrowable)
                StartAim();
            return;
        }

        Throw();
    }

    private void StartAim()
    {
        _aiming = true;
        _aimMarker = new GameObject("AimMarker");
        var sr = _aimMarker.AddComponent<SpriteRenderer>();
        sr.color = _aimColor;
        sr.size = Vector2.one * 0.3f;

        StartAimLine();
        UpdateAimMarker();
    }

    private void StartAimLine()
    {
        var go = new GameObject("AimLine");
        _aimLine = go.AddComponent<LineRenderer>();
        _aimLine.positionCount = 2;
        _aimLine.useWorldSpace = true;
        _aimLine.startWidth = _aimLineWidth;
        _aimLine.endWidth = _aimLineWidth;
        _aimLine.startColor = _aimLineColor;
        _aimLine.endColor = _aimLineColor;
        _aimLine.sortingOrder = 99;
        _aimLine.material = _aimLineMaterial != null
            ? _aimLineMaterial
            : new Material(Shader.Find("Sprites/Default"));
    }

    private void CancelAim()
    {
        _aiming = false;
        StopAimMarker();
        _inventory.ClearSelection();
    }

    private void Throw()
    {
        var item = _inventory.SelectedItem;
        if (item == null)
        {
            CancelAim();
            return;
        }

        if (PlayerStamina.Instance != null)
        {
            if (!PlayerStamina.Instance.TryDrain(item.ThrowEnergy))
            {
                Debug.Log($"[PlayerThrow] Not enough stamina to throw {item.ItemName} (need {item.ThrowEnergy}).");
                CancelAim();
                return;
            }
        }

        Vector2 dir = _lastDir;
        if (dir.sqrMagnitude < 0.01f) dir = Vector2.right;
        dir.Normalize();

        Vector2 spawnPos = transform.position + (Vector3)(dir * _aimDistance);

        // Reuse the collected world object (same size/scale), fallback to temp object.
        var worldItem = InteractItem.FindCollected(item);
        if (worldItem != null)
            ThrownItem.Throw(worldItem, spawnPos, dir, _throwSpeed, gameObject);
        else
            ThrownItem.Spawn(item, spawnPos, dir, _throwSpeed);

        _inventory.RemoveItem(item);
        CancelAim();
    }

    private void UpdateAimMarker()
    {
        if (!_aiming) return;

        Vector2 dir = _lastDir;
        if (dir.sqrMagnitude < 0.01f) dir = Vector2.right;

        Vector2 start = transform.position + (Vector3)(dir * _aimDistance);

        if (_aimMarker != null)
            _aimMarker.transform.position = start;

        if (_aimLine != null)
        {
            _aimLine.SetPosition(0, start);
            _aimLine.SetPosition(1, start + dir * _aimLineLength);
        }
    }

    private void StopAimMarker()
    {
        if (_aimMarker != null)
        {
            Destroy(_aimMarker);
            _aimMarker = null;
        }
        if (_aimLine != null)
        {
            Destroy(_aimLine.gameObject);
            _aimLine = null;
        }
    }

    private void LateUpdate()
    {
        if (_aiming) UpdateAimMarker();
    }

    private void OnDisable()
    {
        if (_subscribed)
        {
            Controls.OnThrowPressed -= OnThrowPressed;
            _subscribed = false;
        }
        if (_aiming) CancelAim();
    }
}
