using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [SerializeField] private List<ItemData> _items = new();

    public event System.Action<ItemData> OnItemAdded;
    public event System.Action OnCleared;

    public IReadOnlyList<ItemData> Items => _items;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool HasItem(ItemData item) => item != null && _items.Contains(item);

    public int GetItemCount(ItemData item)
    {
        if (item == null) return 0;
        int count = 0;
        foreach (var i in _items)
            if (i == item) count++;
        return count;
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;
        _items.Add(item);
        OnItemAdded?.Invoke(item);
    }

    public void RemoveItem(ItemData item)
    {
        if (item == null) return;
        _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
        OnCleared?.Invoke();
    }
}
