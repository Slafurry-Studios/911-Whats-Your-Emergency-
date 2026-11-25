using System.Collections.Generic;
using UnityEngine;
using Slafurry.Core.Interface;

public class InteractItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData _item;
    [SerializeField] private string _interactionPrompt = "Take Item";

    private static readonly List<InteractItem> Instances = new();

    private bool _collected;

    public string InteractionPrompt => _interactionPrompt;

    public bool CanInteract => !_collected;

    public ItemData Item => _item;

    private void Awake() => Instances.Add(this);

    private void OnDestroy() => Instances.Remove(this);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Instances.Clear();

    /// <summary>
    /// Collected (inactive) world object for this item — reused when thrown
    /// so size/scale/sprite match and it can be taken again after landing.
    /// </summary>
    public static InteractItem FindCollected(ItemData item)
    {
        if (item == null) return null;
        for (int i = 0; i < Instances.Count; i++)
        {
            var it = Instances[i];
            if (it != null && it._collected && it._item == item && !it.gameObject.activeSelf)
                return it;
        }
        return null;
    }

    /// <summary>Thrown object came to rest — pickable again.</summary>
    public void OnLanded() => _collected = false;

    public void Interact()
    {
        if (_collected) return;
        if (_item == null) return;
        if (PlayerInventory.Instance == null) return;

        if (!PlayerInventory.Instance.AddItem(_item, this)) return; // inventory full for non-essential items

        _collected = true;
        gameObject.SetActive(false);
    }
}
