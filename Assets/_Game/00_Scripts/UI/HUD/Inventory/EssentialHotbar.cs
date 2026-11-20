using UnityEngine;
using UnityEngine.UI;

namespace Slafurry.UI.HUD.Inventory
{
    public class EssentialHotbar : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ItemData _phoneData;
        [SerializeField] private ItemData _cableData;

        [Header("Slot Images")]
        [SerializeField] private Image _phoneSlot;
        [SerializeField] private Image _cableSlot;

        // Instance yang sedang di-subscribe (untuk unsubscribe yang aman).
        private PlayerInventory _subscribedTo;

        private void OnEnable()
        {
            TrySubscribe();
            Rebuild();
        }

        private void Update()
        {
            var current = PlayerInventory.Instance;

            // Belum subscribe, atau Instance berganti (respawn / scene reload).
            if (current != null && current != _subscribedTo)
            {
                Unsubscribe();
                TrySubscribe();
                Rebuild();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void TrySubscribe()
        {
            var inv = PlayerInventory.Instance;
            if (inv == null || _subscribedTo == inv) return;

            inv.OnInventoryChanged += OnInventoryChanged;
            _subscribedTo = inv;
        }

        private void Unsubscribe()
        {
            if (_subscribedTo != null)
                _subscribedTo.OnInventoryChanged -= OnInventoryChanged;
            _subscribedTo = null;
        }

        private void OnInventoryChanged() => Rebuild();

        private void Rebuild()
        {
            var inv = PlayerInventory.Instance;

            SetSlot(_phoneSlot, _phoneData, inv);
            SetSlot(_cableSlot, _cableData, inv);
        }

        private static void SetSlot(Image slot, ItemData data, PlayerInventory inv)
        {
            if (slot == null) return;

            bool has = data != null && inv != null && inv.HasItem(data);
            slot.sprite = has ? data.Icon : null;
            slot.enabled = has;
        }
    }
}