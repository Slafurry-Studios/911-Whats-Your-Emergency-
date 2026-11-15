using UnityEngine;

public class PhonePresence : MonoBehaviour
{
    [SerializeField] private PhoneData _data;
    [SerializeField] private GameObject _hud;

    public static PhonePresence Instance { get; private set; }

    public bool HasPhone { get; private set; }

    public event System.Action<bool> OnChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() => Refresh(true);

    private void Update() => Refresh(false);

    private void Refresh(bool force)
    {
        bool has = CheckInventory();
        if (!force && has == HasPhone) return;

        HasPhone = has;
        if (_hud != null) _hud.SetActive(has);
        OnChanged?.Invoke(has);
    }

    private bool CheckInventory()
    {
        var phoneItems = _data.PhoneItems;
        if (phoneItems.Count == 0) return false;

        var inventory = PlayerInventory.Instance;
        if (inventory == null) return false;

        foreach (var item in phoneItems)
        {
            if (inventory.HasItem(item)) return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
