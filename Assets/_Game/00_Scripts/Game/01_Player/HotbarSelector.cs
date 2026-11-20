using UnityEngine;
using Slafurry.Systems.InputHub;

/// <summary>Maps Slot_1..Slot_3 input actions to <see cref="PlayerInventory"/> selection.</summary>
public class HotbarSelector : MonoBehaviour
{
    private PlayerInventory _inventory;
    private bool _subscribed;

    private void Awake()
    {
        _inventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        if (!_subscribed && InputHub.Instance != null)
        {
            Controls.OnSlot1Pressed += OnSlot1;
            Controls.OnSlot2Pressed += OnSlot2;
            Controls.OnSlot3Pressed += OnSlot3;
            _subscribed = true;
        }
    }

    private void OnSlot1() => Select(0);
    private void OnSlot2() => Select(1);
    private void OnSlot3() => Select(2);

    private void Select(int slot)
    {
        if (!Controls.IsInputEnabled) return;
        if (_inventory == null) return;
        _inventory.SelectSlot(slot);
    }

    private void OnDisable()
    {
        if (!_subscribed) return;
        Controls.OnSlot1Pressed -= OnSlot1;
        Controls.OnSlot2Pressed -= OnSlot2;
        Controls.OnSlot3Pressed -= OnSlot3;
        _subscribed = false;
    }
}
