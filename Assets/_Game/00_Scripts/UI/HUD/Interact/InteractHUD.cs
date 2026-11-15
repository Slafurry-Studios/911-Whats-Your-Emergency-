using UnityEngine;
using TMPro;
using Slafurry.Core.Interface;

namespace Slafurry.UI.HUD.Interact
{
    [RequireComponent(typeof(TMP_Text))]
    public class InteractHUD : MonoBehaviour
    {
        private TMP_Text _text;
        private IInteractable _current;
        private bool _subscribed;

        private void Awake() => _text = GetComponent<TMP_Text>();

        private void Update()
        {
            if (!_subscribed && PlayerInteract.Instance != null)
            {
                PlayerInteract.Instance.OnTargetChanged += OnTargetChanged;
                OnTargetChanged(PlayerInteract.Instance.CurrentTarget);
                _subscribed = true;
            }
        }

        private void OnTargetChanged(IInteractable target) => _current = target;

        private void Refresh()
        {
            var interact = PlayerInteract.Instance;
            bool show = _current != null && interact != null && interact.CanInteract && _current.CanInteract;
            _text.enabled = show;
            if (show) _text.text = $"[E] {_current.InteractionPrompt}";
        }

        private void LateUpdate() => Refresh();

        private void OnDisable()
        {
            if (!_subscribed) return;
            if (PlayerInteract.Instance != null)
                PlayerInteract.Instance.OnTargetChanged -= OnTargetChanged;
            _subscribed = false;
        }
    }
}
