using System.Collections.Generic;
using UnityEngine;
using Slafurry.Core.Interface;

public class HideArea : MonoBehaviour, IInteractable
{
    [Header("Hide Area")]
    [SerializeField] private SpriteRenderer _areaSprite;
    [SerializeField] private Color _hiddenColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private string _interactionPrompt = "Hide";

    [Header("State sprites (tints above are used only when a sprite is missing)")]
    [SerializeField] private Sprite _emptySprite;
    [SerializeField] private Sprite _hiddenSprite;
    [SerializeField] private Sprite _brokenSprite;

    [Header("Broken")]
    [SerializeField] private Color _brokenColor = new Color(0.35f, 0.3f, 0.3f, 1f);

    /// <summary>All enabled hide areas (the kidnapper picks sabotage targets from here).</summary>
    public static readonly List<HideArea> All = new();

    private Color _originalColor;
    private bool _subscribed;

    public bool IsBroken { get; private set; }

    public string InteractionPrompt => _interactionPrompt;

    public bool CanInteract => !IsBroken;

    private void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);

        if (_areaSprite == null)
            _areaSprite = GetComponent<SpriteRenderer>();
        if (_areaSprite != null)
            _originalColor = _areaSprite.color;

        if (PlayerHide.Instance != null)
        {
            PlayerHide.Instance.OnHideStateChanged += OnHideStateChanged;
            _subscribed = true;
        }
        RefreshVisual();
    }

    // PlayerHide may not exist yet when this enables: subscribe as soon as it does.
    private void Update()
    {
        if (_subscribed || PlayerHide.Instance == null) return;
        PlayerHide.Instance.OnHideStateChanged += OnHideStateChanged;
        _subscribed = true;
        RefreshVisual();
    }

    private void OnHideStateChanged(bool isHidden) => RefreshVisual();

    private void RefreshVisual()
    {
        if (_areaSprite == null) return;

        if (IsBroken)
        {
            if (_brokenSprite != null) { _areaSprite.sprite = _brokenSprite; _areaSprite.color = _originalColor; }
            else _areaSprite.color = _brokenColor;
            return;
        }

        bool inThis = PlayerHide.Instance != null && PlayerHide.Instance.IsHidden &&
                      PlayerHide.Instance.CurrentArea == this;
        Sprite sprite = inThis ? _hiddenSprite : _emptySprite;
        if (sprite != null)
        {
            _areaSprite.sprite = sprite;
            _areaSprite.color = _originalColor;
        }
        else
        {
            _areaSprite.color = inThis ? _hiddenColor : _originalColor;
        }
    }

    public void Interact()
    {
        var hide = PlayerHide.Instance;
        if (hide == null || IsBroken) return;
        hide.SetHidden(!hide.IsHidden, this);
        RefreshVisual(); // in case OnEnable ran before PlayerHide existed (no event subscription)
    }

    /// <summary>The kidnapper destroyed this spot: kicks the player out; it can't be used again.</summary>
    public void Break()
    {
        if (IsBroken) return;
        IsBroken = true;

        if (PlayerHide.Instance != null && PlayerHide.Instance.CurrentArea == this)
            PlayerHide.Instance.SetHidden(false);

        RefreshVisual();
        foreach (var col in GetComponents<Collider2D>())
            if (!col.isTrigger) col.enabled = false;
    }

    private void OnDisable()
    {
        All.Remove(this);
        if (_subscribed && PlayerHide.Instance != null)
        {
            PlayerHide.Instance.OnHideStateChanged -= OnHideStateChanged;
            _subscribed = false;
        }
    }
}
