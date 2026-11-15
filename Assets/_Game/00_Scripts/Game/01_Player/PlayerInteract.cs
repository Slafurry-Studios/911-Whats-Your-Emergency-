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

    private void UpdateTarget()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);

        IInteractable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            var interactable = hit.gameObject.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
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
        if (CurrentTarget == null) return;

        CurrentTarget.Interact();
        AddTriggerCount();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }

    private void OnDisable()
    {
        if (!_subscribed) return;
        if (InputHub.Instance != null)
            Controls.OnInteractPressed -= OnInteractPressed;
        _subscribed = false;
    }
}
