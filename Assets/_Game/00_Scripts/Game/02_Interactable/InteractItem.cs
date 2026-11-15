using UnityEngine;
using Slafurry.Core.Interface;

public class InteractItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private ItemData _item;
    [SerializeField] private string _interactionPrompt = "Take Item";

    private bool _collected;

    public string InteractionPrompt => _interactionPrompt;

    public bool CanInteract => !_collected;

    public void Interact()
    {
        if (_collected) return;
        if (_item == null) return;

        _collected = true;
        PlayerInventory.Instance?.AddItem(_item);
        gameObject.SetActive(false);
    }
}
