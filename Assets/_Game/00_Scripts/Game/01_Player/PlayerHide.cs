using UnityEngine;

public class PlayerHide : MonoBehaviour
{
    public static PlayerHide Instance { get; private set; }

    public bool IsHidden { get; private set; }
    public HideArea CurrentArea { get; private set; }

    public event System.Action<bool> OnHideStateChanged;

    [Header("Player Visuals")]
    [SerializeField] private SpriteRenderer _playerSprite;

    private PlayerMovement _movement;
    private Color _originalColor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _movement = GetComponent<PlayerMovement>();
        if (_playerSprite == null)
            _playerSprite = GetComponent<SpriteRenderer>();
        if (_playerSprite != null)
            _originalColor = _playerSprite.color;
    }

    public void SetHidden(bool hidden) => SetHidden(hidden, null);

    public void SetHidden(bool hidden, HideArea area)
    {
        if (IsHidden == hidden) return;

        IsHidden = hidden;
        CurrentArea = hidden ? area : null;

        if (_movement != null)
            _movement.enabled = !hidden;

        if (_playerSprite != null)
            _playerSprite.enabled = !hidden;

        OnHideStateChanged?.Invoke(IsHidden);
    }

    public void Toggle() => SetHidden(!IsHidden);
}
