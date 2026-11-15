using UnityEngine;
using Slafurry.Core.Interface;

public class InteractItemDebug : MonoBehaviour, IInteractable
{
    [Header("Debug")]
    [SerializeField] private string itemName = "Debug Item";
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private string _interactionPrompt = "Debug Interact";

    private SpriteRenderer _renderer;
    private Color _originalColor;
    private bool _isHighlighted;

    public string InteractionPrompt => _interactionPrompt;

    private void Awake() => _renderer = GetComponent<SpriteRenderer>();

    private void OnEnable()
    {
        if (_renderer != null)
            _originalColor = _renderer.color;
    }

    public void Interact()
    {
        Debug.Log($"[InteractItemDebug] Interacted with '{itemName}' at {gameObject.name}");
        _isHighlighted = !_isHighlighted;
        if (_renderer != null)
            _renderer.color = _isHighlighted ? highlightColor : _originalColor;
    }
}
