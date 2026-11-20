using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    /// <summary>Max number of non-essential (throwable) items the player can carry.</summary>
    public const int MaxOtherItems = 3;

    [SerializeField] private List<ItemData> _otherItems = new();
    [SerializeField] private List<ItemData> _essentialItems = new();

    private int _selectedSlot = -1; // -1 = none, 0..MaxOtherItems-1

    public event System.Action<ItemData> OnItemAdded;
    public event System.Action<ItemData> OnItemRemoved;
    public event System.Action OnInventoryChanged;
    public event System.Action<int> OnSelectionChanged;

    public IReadOnlyList<ItemData> OtherItems => _otherItems;
    public IReadOnlyList<ItemData> EssentialItems => _essentialItems;
    public int SelectedSlot => _selectedSlot;
    public ItemData SelectedItem =>
        _selectedSlot >= 0 && _selectedSlot < _otherItems.Count ? _otherItems[_selectedSlot] : null;
    public int OtherItemCount => _otherItems.Count;
    public bool CanAddOtherItem => _otherItems.Count < MaxOtherItems;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool HasItem(ItemData item) => item != null && (_otherItems.Contains(item) || _essentialItems.Contains(item));

    public int GetItemCount(ItemData item)
    {
        if (item == null) return 0;
        int count = 0;
        foreach (var i in _otherItems)
            if (i == item) count++;
        foreach (var i in _essentialItems)
            if (i == item) count++;
        return count;
    }

    public bool AddItem(ItemData item)
    {
        if (item == null) return false;

        if (item.Type == ItemType.Essential)
        {
            if (!_essentialItems.Contains(item))
                _essentialItems.Add(item);
        }
        else
        {
            if (_otherItems.Count >= MaxOtherItems) return false;
            _otherItems.Add(item);
        }

        OnInventoryChanged?.Invoke();
        OnItemAdded?.Invoke(item);
        return true;
    }

    public bool RemoveItem(ItemData item)
    {
        if (item == null) return false;

        bool removed = item.Type == ItemType.Essential ? _essentialItems.Remove(item) : _otherItems.Remove(item);
        if (!removed) return false;

        if (item.Type != ItemType.Essential)
        {
            if (_selectedSlot >= _otherItems.Count)
                _selectedSlot = _otherItems.Count > 0 ? _otherItems.Count - 1 : -1;
            OnSelectionChanged?.Invoke(_selectedSlot);
        }

        OnInventoryChanged?.Invoke();
        OnItemRemoved?.Invoke(item);
        return true;
    }

    public void SelectSlot(int slot)
    {
        if (slot < -1 || slot >= _otherItems.Count) return;
        if (_selectedSlot == slot) return;
        _selectedSlot = slot;
        OnSelectionChanged?.Invoke(slot);
    }

    public void ClearSelection() => SelectSlot(-1);

    public void Clear()
    {
        _otherItems.Clear();
        _essentialItems.Clear();
        _selectedSlot = -1;
        OnInventoryChanged?.Invoke();
        OnSelectionChanged?.Invoke(-1);
    }
}
