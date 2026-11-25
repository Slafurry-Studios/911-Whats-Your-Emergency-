using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Slafurry.UI.HUD.Inventory
{
    /// <summary>
    /// Put on an item hotbar slot. Drag the slot out onto the world to throw that item from the
    /// player toward the release point. Releasing over the HUD or pressing Escape cancels.
    /// </summary>
    public class HotbarDragThrow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private int _slot;
        [SerializeField] private float _ghostSize = 56f;
        [SerializeField] private Color _ghostColor = new Color(1f, 1f, 1f, 0.85f);

        private PlayerThrow _thrower;
        private Canvas _canvas;
        private RectTransform _ghost;
        private Camera _camera;
        private bool _dragging;
        private bool _cancelled;
        private Vector2 _lastWorld;

        private void Awake() => _canvas = GetComponentInParent<Canvas>()?.rootCanvas;

        public void OnBeginDrag(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left) return;
            if (_thrower == null) _thrower = FindObjectOfType<PlayerThrow>();
            if (_thrower == null || !_thrower.CanThrowSlot(_slot)) return;

            _dragging = true;
            _cancelled = false;
            PlayerInventory.Instance?.SelectSlot(_slot);
            CreateGhost(PlayerInventory.Instance != null ? PlayerInventory.Instance.GetSlotIcon(_slot) : null);
            UpdateDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (_dragging && !_cancelled) UpdateDrag(e);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!_dragging) return;
            _dragging = false;
            DestroyGhost();

            if (_cancelled || OverHud(e) || !TryScreenToWorld(e.position, out var target))
            {
                _thrower.HideAim();
                return;
            }
            _thrower.ThrowSlot(_slot, target);
        }

        private void Update()
        {
            if (!_dragging || _cancelled) return;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Cancel();
            else if (!_thrower.CanThrowSlot(_slot)) Cancel(); // e.g. paused, hidden, item gone
        }

        private void Cancel()
        {
            _cancelled = true;
            DestroyGhost();
            _thrower.HideAim();
        }

        private void UpdateDrag(PointerEventData e)
        {
            if (_ghost != null) _ghost.position = e.position;

            // Over the HUD the aim stays where it was (releasing there cancels).
            if (OverHud(e) || !TryScreenToWorld(e.position, out var world)) return;
            _lastWorld = world;
            var icon = PlayerInventory.Instance != null ? PlayerInventory.Instance.GetSlotIcon(_slot) : null;
            _thrower.ShowAim(world, icon);
        }

        private static bool OverHud(PointerEventData e)
        {
            var go = e.pointerCurrentRaycast.gameObject;
            return go != null && go.GetComponentInParent<Canvas>() != null;
        }

        private bool TryScreenToWorld(Vector2 screen, out Vector2 world)
        {
            world = _lastWorld;
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return false;
            var p = new Vector3(screen.x, screen.y, Mathf.Abs(_camera.transform.position.z));
            world = _camera.ScreenToWorldPoint(p);
            return true;
        }

        private void CreateGhost(Sprite icon)
        {
            if (_canvas == null || icon == null) return;
            var go = new GameObject("ThrowGhost", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_canvas.transform, false);
            go.transform.SetAsLastSibling();
            _ghost = (RectTransform)go.transform;
            _ghost.sizeDelta = Vector2.one * _ghostSize;
            var img = go.GetComponent<Image>();
            img.sprite = icon;
            img.preserveAspect = true;
            img.color = _ghostColor;
            img.raycastTarget = false; // let the pointer see what's under it
        }

        private void DestroyGhost()
        {
            if (_ghost != null) Destroy(_ghost.gameObject);
            _ghost = null;
        }

        private void OnDisable()
        {
            if (!_dragging) return;
            _dragging = false;
            DestroyGhost();
            if (_thrower != null) _thrower.HideAim();
        }
    }
}
