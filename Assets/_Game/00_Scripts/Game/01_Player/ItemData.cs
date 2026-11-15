using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string _itemName = "Item";
    [SerializeField] private Sprite _icon;
    [SerializeField] private bool _stackable = true;

    public string ItemName => _itemName;
    public Sprite Icon => _icon;
    public bool Stackable => _stackable;
}
