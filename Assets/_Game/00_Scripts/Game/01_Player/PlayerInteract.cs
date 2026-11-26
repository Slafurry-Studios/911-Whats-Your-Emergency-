using UnityEngine;
using Slafurry.Core.Interface;
using Slafurry.Systems.InputHub;

public class PlayerInteract : BaseTrigger
{
    public static PlayerInteract Instance { get; private set; }

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 1.5f;
    [SerializeField] private LayerMask interactLayer;

    public IInteractable CurrentTarget { get; private set; }

    public bool CanInteract => CanTrigger();

    public event System.Action<IInteractable> OnTargetChanged;

    private bool _subscribed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (!_subscribed && InputHub.Instance != null)
        {
            Controls.OnInteractPressed += OnInteractPressed;
            _subscribed = true;
        }

        UpdateTarget();
    }

    // Measure from the body collider (the feet), which is what touches doors and items —
    // the transform is the sprite centre, well above the feet in this top-down view.
    private Vector2 Origin
    {
        get
        {
            if (_body == null) _body = GetComponent<Collider2D>();
            return _body != null ? (Vector2)_body.bounds.center : (Vector2)transform.position;
        }
    }

    private Collider2D _body;

    private void UpdateTarget()
    {
        Vector2 origin = Origin;
        var hits = Physics2D.OverlapCircleAll(origin, interactRadius, interactLayer);

        IInteractable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            var interactable = hit.gameObject.GetComponent<IInteractable>();
            if (interactable == null || !interactable.CanInteract) continue;

            float distance = Vector2.Distance(origin, hit.ClosestPoint(origin));
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = interactable;
            }
        }

        if (nearest != CurrentTarget)
        {
            CurrentTarget = nearest;
            OnTargetChanged?.Invoke(CurrentTarget);
        }
    }

    private void OnInteractPressed()
    {
        if (!Controls.IsInputEnabled) return;
        if (!CanTrigger()) return;
        // Left click is also Interact: ignore clicks on the HUD (e.g. dragging a hotbar slot).
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame &&
            UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
        if (CurrentTarget == null || !CurrentTarget.CanInteract) return;

        CurrentTarget.Interact();
        AddTriggerCount();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Origin, interactRadius);
    }

    private void OnDisable()
    {
        if (!_subscribed) return;
        if (InputHub.Instance != null)
            Controls.OnInteractPressed -= OnInteractPressed;
        _subscribed = false;
    }
}
