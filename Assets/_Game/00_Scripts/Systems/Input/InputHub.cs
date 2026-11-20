using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Slafurry.Core.Abstract;
using Slafurry.Systems.Scene;

namespace Slafurry.Systems.InputHub
{
    public static class Controls
    {
        public static event Action OnJumpPressed
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnJumpPressed += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnJumpPressed -= value;
            }
        }
        public static event Action<Vector2> OnMoveChanged
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnMoveChanged += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnMoveChanged -= value;
            }
        }
        public static event Action OnCrouchStarted
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnCrouchStarted += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnCrouchStarted -= value;
            }
        }
        public static event Action OnCrouchCanceled
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnCrouchCanceled += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnCrouchCanceled -= value;
            }
        }
        public static event Action OnInteractPressed
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnInteractPressed += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnInteractPressed -= value;
            }
        }
        public static event Action OnInteractReleased
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnInteractReleased += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnInteractReleased -= value;
            }
        }

        public static event Action OnSprintStarted
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnSprintStarted += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnSprintStarted -= value;
            }
        }
        public static event Action OnSprintCanceled
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnSprintCanceled += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnSprintCanceled -= value;
            }
        }

        public static event Action OnFlashlightPressed
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnFlashlightPressed += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnFlashlightPressed -= value;
            }
        }

        public static event Action OnVolIncPressed
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnVolIncPressed += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnVolIncPressed -= value;
            }
        }

        public static event Action OnVolDecPressed
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnVolDecPressed += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnVolDecPressed -= value;
            }
        }

        public static event Action OnTabPressed
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnTabPressed += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnTabPressed -= value;
            }
        }

        public static event Action OnSlot1Pressed
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnSlot1Pressed += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnSlot1Pressed -= value;
            }
        }

        public static event Action OnSlot2Pressed
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnSlot2Pressed += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnSlot2Pressed -= value;
            }
        }

        public static event Action OnSlot3Pressed
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnSlot3Pressed += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnSlot3Pressed -= value;
            }
        }

        public static event Action OnThrowPressed
        {
            add
            {
                if (InputHub.Instance != null) InputHub.Instance.OnThrowPressed += value;
            }
            remove
            {
                if (InputHub.Instance != null) InputHub.Instance.OnThrowPressed -= value;
            }
        }

        public static bool IsInputEnabled => InputHub.Instance != null && InputHub.Instance.IsInputEnabled;

        public static void EnableInput()
        {
            if (InputHub.Instance != null) InputHub.Instance.EnableInput();
        }
        public static void DisableInput()
        {
            if (InputHub.Instance != null) InputHub.Instance.DisableInput();
        }
        public static void SetInputEnabled(bool enabled)
        {
            if (InputHub.Instance != null) InputHub.Instance.SetInputEnabled(enabled);
        }
    }

    public class InputHub : GameSystem<InputHub>
    {
        [SerializeField] private InputActionAsset inputActions;

        public event Action OnJumpPressed;
        public event Action<Vector2> OnMoveChanged;
        public event Action OnCrouchStarted;
        public event Action OnCrouchCanceled;
        public event Action OnInteractPressed;
        public event Action OnInteractReleased;

        public event Action OnSprintStarted;
        public event Action OnSprintCanceled;
        public event Action OnFlashlightPressed;
        public event Action OnVolIncPressed;
        public event Action OnVolDecPressed;
        public event Action OnTabPressed;
        public event Action OnSlot1Pressed;
        public event Action OnSlot2Pressed;
        public event Action OnSlot3Pressed;
        public event Action OnThrowPressed;

        private InputAction _jumpAction;
        private InputAction _moveAction;
        private InputAction _crouchAction;
        private InputAction _interactAction;
        private InputAction _sprintAction;
        private InputAction _flashlightAction;
        private InputAction _volIncAction;
        private InputAction _volDecAction;
        private InputAction _tabAction;
        private InputAction _slot1Action;
        private InputAction _slot2Action;
        private InputAction _slot3Action;
        private InputAction _throwAction;
        private InputActionMap _gameplayMap;

        public bool IsInputEnabled { get; private set; } = true;

        public override IEnumerator Initialize() { yield return null; }
        public override void PostInitialize()
        {
            SceneLoader.Instance.OnSceneLoadCompleted += _ => EnableInput();
        }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            _gameplayMap = inputActions.FindActionMap("Gameplay");
            _jumpAction = _gameplayMap.FindAction("Jump");
            _moveAction = _gameplayMap.FindAction("Move");
            _crouchAction = _gameplayMap.FindAction("Crouch");
            _interactAction = _gameplayMap.FindAction("Interact");
            _sprintAction = _gameplayMap.FindAction("Sprint");

            _jumpAction.performed += ctx => OnJumpPressed?.Invoke();
            _moveAction.performed += ctx => OnMoveChanged?.Invoke(ctx.ReadValue<Vector2>());
            _moveAction.canceled += ctx => OnMoveChanged?.Invoke(Vector2.zero);
            _crouchAction.started += ctx => OnCrouchStarted?.Invoke();
            _crouchAction.canceled += ctx => OnCrouchCanceled?.Invoke();
            _interactAction.performed += ctx => OnInteractPressed?.Invoke();
            _interactAction.canceled += ctx => OnInteractReleased?.Invoke();
            if (_sprintAction != null)
            {
                _sprintAction.started += ctx => OnSprintStarted?.Invoke();
                _sprintAction.canceled += ctx => OnSprintCanceled?.Invoke();
            }

            _flashlightAction = _gameplayMap.FindAction("Flashlight");
            _volIncAction = _gameplayMap.FindAction("VolInc");
            _volDecAction = _gameplayMap.FindAction("VolDec");
            _tabAction = _gameplayMap.FindAction("Tab");

            if (_flashlightAction != null)
                _flashlightAction.performed += ctx => OnFlashlightPressed?.Invoke();
            if (_volIncAction != null)
                _volIncAction.performed += ctx => OnVolIncPressed?.Invoke();
            if (_volDecAction != null)
                _volDecAction.performed += ctx => OnVolDecPressed?.Invoke();
            if (_tabAction != null)
                _tabAction.performed += ctx => OnTabPressed?.Invoke();

            _slot1Action = _gameplayMap.FindAction("Slot_1");
            _slot2Action = _gameplayMap.FindAction("Slot_2");
            _slot3Action = _gameplayMap.FindAction("Slot_3");
            _throwAction = _gameplayMap.FindAction("Throw");

            if (_slot1Action != null)
                _slot1Action.performed += ctx => OnSlot1Pressed?.Invoke();
            if (_slot2Action != null)
                _slot2Action.performed += ctx => OnSlot2Pressed?.Invoke();
            if (_slot3Action != null)
                _slot3Action.performed += ctx => OnSlot3Pressed?.Invoke();
            if (_throwAction != null)
                _throwAction.performed += ctx => OnThrowPressed?.Invoke();

            _gameplayMap.Enable();
            IsInputEnabled = true;
        }

        public void EnableInput()
        {
            if (IsInputEnabled) return;

            _gameplayMap.Enable();
            IsInputEnabled = true;
        }

        public void DisableInput()
        {
            if (!IsInputEnabled) return;

            if (_moveAction.IsPressed())
                OnMoveChanged?.Invoke(Vector2.zero);

            if (_crouchAction.IsPressed())
                OnCrouchCanceled?.Invoke();

            _gameplayMap.Disable();
            IsInputEnabled = false;
        }

        public void SetInputEnabled(bool enabled)
        {
            if (enabled) EnableInput();
            else DisableInput();
        }
    }
}
