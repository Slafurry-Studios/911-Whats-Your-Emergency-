using UnityEngine;
using UnityEngine.UI;
using Slafurry.Systems.InputHub;

namespace Slafurry.UI.HUD.Inventory
{
    public class ItemHotbar : MonoBehaviour
    {
        [Header("Slot Images")]
        [SerializeField] private Image _slot1;
        [SerializeField] private Image _slot2;
        [SerializeField] private Image _slot3;

        [Header("Selection Highlights")]
        [SerializeField] private Image _select1;
        [SerializeField] private Image _select2;
        [SerializeField] private Image _select3;
        [SerializeField] private Color _selectedColor = new Color(1f, 0.9f, 0.2f, 0.35f);

        private readonly Image[] _slots = new Image[3];
        private readonly Image[] _highlights = new Image[3];

        private bool _inventorySubscribed;
        private bool _inputSubscribed;

        private void Awake()
        {
            _slots[0] = _slot1;
            _slots[1] = _slot2;
            _slots[2] = _slot3;
            _highlights[0] = _select1;
            _highlights[1] = _select2;
            _highlights[2] = _select3;
        }

        private void OnEnable()
        {
            TrySubscribeInventory();
            TrySubscribeInput();
            Rebuild();
            ApplySelection(PlayerInventory.Instance != null ? PlayerInventory.Instance.SelectedSlot : -1);
        }

        private void Update()
        {
            // Retry subscription for singletons that may not exist yet at OnEnable.
            if (!_inventorySubscribed && PlayerInventory.Instance != null)
            {
                TrySubscribeInventory();
                Rebuild();
                ApplySelection(PlayerInventory.Instance.SelectedSlot);
            }

            if (!_inputSubscribed)
                TrySubscribeInput();
        }

        private void OnDisable()
        {
            if (_inventorySubscribed && PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.OnInventoryChanged -= OnInventoryChanged;
                PlayerInventory.Instance.OnSelectionChanged -= OnSelectionChanged;
            }
            _inventorySubscribed = false;

            if (_inputSubscribed)
            {
                Controls.OnSlot1Pressed -= OnSlot1;
                Controls.OnSlot2Pressed -= OnSlot2;
                Controls.OnSlot3Pressed -= OnSlot3;
            }
            _inputSubscribed = false;
        }

        // ---------- Subscriptions ----------

        private void TrySubscribeInventory()
        {
            if (_inventorySubscribed || PlayerInventory.Instance == null) return;

            PlayerInventory.Instance.OnInventoryChanged += OnInventoryChanged;
            PlayerInventory.Instance.OnSelectionChanged += OnSelectionChanged;
            _inventorySubscribed = true;
        }

        private void TrySubscribeInput()
        {
            if (_inputSubscribed || InputHub.Instance == null) return;

            Controls.OnSlot1Pressed += OnSlot1;
            Controls.OnSlot2Pressed += OnSlot2;
            Controls.OnSlot3Pressed += OnSlot3;
            _inputSubscribed = true;
        }

        // ---------- Input ----------

        private void OnSlot1() => Select(0);
        private void OnSlot2() => Select(1);
        private void OnSlot3() => Select(2);

        private void Select(int slot)
        {
            if (!Controls.IsInputEnabled) return;
            if (PlayerInventory.Instance == null) return;
            PlayerInventory.Instance.SelectSlot(slot);
        }

        // ---------- UI ----------

        private void OnInventoryChanged() => Rebuild();

        private void OnSelectionChanged(int slot) => ApplySelection(slot);

        private void ApplySelection(int slot)
        {
            for (int i = 0; i < 3; i++)
            {
                var h = _highlights[i];
                if (h == null) continue;

                // Keep Image enabled; only tint vertex color (clear when unselected).
                h.color = i == slot ? _selectedColor : Color.clear;
            }
        }

        private void Rebuild()
        {
            var items = PlayerInventory.Instance?.OtherItems;

            for (int i = 0; i < 3; i++)
            {
                if (_slots[i] == null) continue;

                bool has = items != null && i < items.Count;
                _slots[i].sprite = has ? items[i].Icon : null;
                _slots[i].enabled = has;
            }
        }
    }
}