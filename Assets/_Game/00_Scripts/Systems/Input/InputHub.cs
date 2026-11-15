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

        private InputAction _jumpAction;
        private InputAction _moveAction;
        private InputAction _crouchAction;
        private InputAction _interactAction;
        private InputAction _sprintAction;
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
