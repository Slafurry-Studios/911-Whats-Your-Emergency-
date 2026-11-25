using UnityEngine;
using Slafurry.Systems.InputHub;

/// <summary>
/// Throwing. The hotbar drag (HotbarDragThrow) drives it: while dragging a slot it shows an aim
/// line from the player to the cursor; on release the item flies from the player toward that
/// point and lands there (unless it hits a wall or the kidnapper first).
/// </summary>
public class PlayerThrow : MonoBehaviour
{
    [Header("Throw")]
    [Tooltip("Furthest an item can be thrown (world units).")]
    [SerializeField] private float _maxRange = 12f;
    [SerializeField] private float _minRange = 1.5f;
    [Tooltip("Spawn distance in front of the player.")]
    [SerializeField] private float _aimDistance = 0.8f;

    [Header("Aim Line")]
    [SerializeField] private Color _aimLineColor = new Color(1f, 0.85f, 0.3f, 0.8f);
    [SerializeField] private float _aimLineWidth = 0.06f;
    [SerializeField] private Material _aimLineMaterial; // optional; falls back to Sprites/Default
    [SerializeField] private Color _aimMarkerColor = new Color(1f, 1f, 1f, 0.55f);

    private const float MarkerWorldSize = 1.0864f; // one level tile

    private PlayerInventory _inventory;
    private LineRenderer _aimLine;
    private SpriteRenderer _aimMarker;
    private bool _aiming;

    /// <summary>True while an aim is shown (a hotbar slot is being dragged).</summary>
    public bool IsAiming => _aiming;

    public bool CanThrow =>
        Controls.IsInputEnabled &&
        !GameOutcome.IsFinished &&
        (PlayerHide.Instance == null || !PlayerHide.Instance.IsHidden);

    private void Awake() => _inventory = GetComponent<PlayerInventory>();

    public bool CanThrowSlot(int slot)
    {
        if (!CanThrow || _inventory == null) return false;
        if (slot < 0 || slot >= _inventory.OtherItems.Count) return false;
        var item = _inventory.OtherItems[slot];
        return item != null && item.IsThrowable;
    }

    // ------------------------------------------------------------------ aim preview
    /// <summary>Show the aim from the player toward target (clamped to range).</summary>
    public void ShowAim(Vector2 target, Sprite icon)
    {
        if (_aimLine == null) CreateAimVisuals();
        _aiming = true;

        Vector2 from = transform.position;
        Vector2 landing = ClampTarget(from, target);
        Vector2 dir = (landing - from).normalized;

        _aimLine.gameObject.SetActive(true);
        _aimLine.SetPosition(0, from + dir * _aimDistance);
        _aimLine.SetPosition(1, landing);

        _aimMarker.gameObject.SetActive(icon != null);
        _aimMarker.sprite = icon;
        _aimMarker.transform.position = landing;
        if (icon != null) // about one level tile, like items on the floor
            _aimMarker.transform.localScale = Vector3.one * (MarkerWorldSize / Mathf.Max(0.01f, icon.bounds.size.x));
    }

    public void HideAim()
    {
        _aiming = false;
        if (_aimLine != null) _aimLine.gameObject.SetActive(false);
        if (_aimMarker != null) _aimMarker.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------------ throw
    /// <summary>Throw the item in slot from the player toward target. Returns false if it can't.</summary>
    public bool ThrowSlot(int slot, Vector2 target)
    {
        HideAim();
        if (!CanThrowSlot(slot)) return false;

        var item = _inventory.OtherItems[slot];
        if (PlayerStamina.Instance != null && !PlayerStamina.Instance.TryDrain(item.ThrowEnergy))
        {
            Debug.Log($"[PlayerThrow] Not enough stamina to throw {item.ItemName} (need {item.ThrowEnergy}).");
            return false;
        }

        Vector2 from = transform.position;
        Vector2 landing = ClampTarget(from, target);
        Vector2 dir = (landing - from).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;

        Vector2 spawn = from + dir * _aimDistance;
        float travel = Mathf.Max(0.1f, Vector2.Distance(spawn, landing));
        float speed = travel * ThrownItem.Drag; // linear drag → stops ~speed/drag later

        // The exact object picked into this slot (keeps its sprite); else any collected one.
        var worldItem = _inventory.GetSlotWorldItem(slot);
        if (worldItem == null || worldItem.gameObject.activeSelf) worldItem = InteractItem.FindCollected(item);

        if (worldItem != null) ThrownItem.Throw(worldItem, spawn, dir, speed, gameObject);
        else ThrownItem.Spawn(item, spawn, dir, speed);

        _inventory.RemoveSlot(slot);
        return true;
    }

    private Vector2 ClampTarget(Vector2 from, Vector2 target)
    {
        Vector2 to = target - from;
        float d = to.magnitude;
        if (d < 0.0001f) return from + Vector2.right * _minRange;
        return from + to / d * Mathf.Clamp(d, _minRange, _maxRange);
    }

    private void CreateAimVisuals()
    {
        var lineGo = new GameObject("AimLine");
        _aimLine = lineGo.AddComponent<LineRenderer>();
        _aimLine.positionCount = 2;
        _aimLine.useWorldSpace = true;
        _aimLine.startWidth = _aimLineWidth;
        _aimLine.endWidth = _aimLineWidth * 0.5f;
        _aimLine.startColor = _aimLineColor;
        _aimLine.endColor = new Color(_aimLineColor.r, _aimLineColor.g, _aimLineColor.b, 0.2f);
        _aimLine.sortingOrder = 99;
        _aimLine.material = _aimLineMaterial != null
            ? _aimLineMaterial
            : new Material(Shader.Find("Sprites/Default"));

        var markerGo = new GameObject("AimMarker");
        _aimMarker = markerGo.AddComponent<SpriteRenderer>();
        _aimMarker.color = _aimMarkerColor;
        _aimMarker.sortingOrder = 100;

        HideAim();
    }

    private void OnDisable() => HideAim();

    private void OnDestroy()
    {
        if (_aimLine != null) Destroy(_aimLine.gameObject);
        if (_aimMarker != null) Destroy(_aimMarker.gameObject);
    }
}
