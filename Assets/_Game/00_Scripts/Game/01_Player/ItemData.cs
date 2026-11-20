using UnityEngine;

public enum ItemType
{
    Normal,
    Throwable,
    Essential,
    Static
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string _itemName = "Item";
    [SerializeField] private Sprite _icon;
    [SerializeField] private bool _stackable = true;
    [SerializeField] private ItemType _type = ItemType.Normal;
    [SerializeField] private float _throwEnergy = 1f;

    public string ItemName => _itemName;
    public Sprite Icon => _icon;
    public bool Stackable => _stackable;
    public ItemType Type => _type;
    public bool IsEssential => _type == ItemType.Essential;
    public bool IsThrowable => _type == ItemType.Throwable;
    /// <summary>Energy cost to throw this item.</summary>
    public float ThrowEnergy => _throwEnergy;
}
