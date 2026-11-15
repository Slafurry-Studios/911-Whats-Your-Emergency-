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

    private void ApplyState()
    {
        if (_renderer != null)
            _renderer.sprite = _isOpen ? _openSprite : _closedSprite;

        if (_physicsCollider != null)
            _physicsCollider.enabled = !_isOpen;
    }
}
