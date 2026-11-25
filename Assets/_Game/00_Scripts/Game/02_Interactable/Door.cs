using UnityEngine;
using Slafurry.Core.Interface;

public class Door : BaseTrigger, IInteractable
{
    [Header("Door")]
    [SerializeField] private bool _startOpen = false;
    [SerializeField] private Sprite _openSprite;
    [SerializeField] private Sprite _closedSprite;
    [SerializeField] private Collider2D _physicsCollider;
    [SerializeField] private string _promptOpen = "Close Door";
    [SerializeField] private string _promptClosed = "Open Door";

    private SpriteRenderer _renderer;
    private bool _isOpen;

    public string InteractionPrompt => _isOpen ? _promptOpen : _promptClosed;

    public bool IsOpen => _isOpen;

    public bool CanInteract => CanTrigger();

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _isOpen = _startOpen;
        ApplyState();
    }

    public void Interact()
    {
        if (!CanTrigger()) return;

        _isOpen = !_isOpen;
        ApplyState();
        AddTriggerCount();
    }

    /// <summary>Opens the door regardless of the trigger limit (the kidnapper forcing it).</summary>
    public void ForceOpen()
    {
        if (_isOpen) return;
        _isOpen = true;
        ApplyState();
    }

    private void ApplyState()
    {
        if (_renderer != null)
            _renderer.sprite = _isOpen ? _openSprite : _closedSprite;

        if (_physicsCollider != null)
            _physicsCollider.enabled = !_isOpen;
    }
}
