using UnityEngine;
using Slafurry.Core.Interface;

public class HideArea : MonoBehaviour, IInteractable
{
    [Header("Hide Area")]
    [SerializeField] private SpriteRenderer _areaSprite;
    [SerializeField] private Color _hiddenColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private string _interactionPrompt = "Hide";

    private Color _originalColor;
    private bool _subscribed;

    public string InteractionPrompt => _interactionPrompt;

    private void OnEnable()
    {
        if (_areaSprite == null)
            _areaSprite = GetComponent<SpriteRenderer>();
        if (_areaSprite != null)
            _originalColor = _areaSprite.color;

        if (PlayerHide.Instance != null)
        {
            PlayerHide.Instance.OnHideStateChanged += OnHideStateChanged;
            _subscribed = true;
            OnHideStateChanged(PlayerHide.Instance.IsHidden);
        }
    }

    private void OnHideStateChanged(bool isHidden)
    {
        if (_areaSprite != null)
            _areaSprite.color = isHidden ? _hiddenColor : _originalColor;
    }

    public void Interact()
    {
        PlayerHide.Instance?.Toggle();
    }

    private void OnDisable()
    {
        if (_subscribed && PlayerHide.Instance != null)
        {
            PlayerHide.Instance.OnHideStateChanged -= OnHideStateChanged;
            _subscribed = false;
        }
    }
}
