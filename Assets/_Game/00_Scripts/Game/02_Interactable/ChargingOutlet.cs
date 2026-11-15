using UnityEngine;
using Slafurry.Core.Interface;

public class ChargingOutlet : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData _chargingCable;
    [SerializeField] private string _interactionPrompt = "Charge Phone";

    public string InteractionPrompt => _interactionPrompt;

    public bool CanInteract
    {
        get
        {
            if (PhonePresence.Instance != null && !PhonePresence.Instance.HasPhone) return false;
            if (PhoneCharging.Instance != null && PhoneCharging.Instance.IsCharging) return false;
            if (_chargingCable == null) return false;

            var inventory = PlayerInventory.Instance;
            return inventory != null && inventory.HasItem(_chargingCable);
        }
    }

    public void Interact()
    {
        if (!CanInteract) return;

        PhoneCharging.Instance?.Begin();
    }
}
