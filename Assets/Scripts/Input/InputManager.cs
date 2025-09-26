using System;
using System.Collections.Generic;
using UnityEngine;
using AngryDogs.SaveSystem;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AngryDogs.Input
{
    /// <summary>
    /// Aggregates touch, mouse, and keyboard/controller input with configurable bindings.
    /// Emits smoothed movement/aim events consumed by gameplay coordinators.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class InputManager : MonoBehaviour
    {
        private const string MoveLeft = "MoveLeft";
        private const string MoveRight = "MoveRight";
        private const string MoveUp = "MoveUp";
        private const string MoveDown = "MoveDown";
        private const string FireAction = "Fire";
        private const string AbilityAction = "Ability";

        [Serializable]
        private struct DefaultBinding
        {
            public string actionId;
            public KeyCode key;
        }

        [Header("Bindings")]
        [SerializeField] private DefaultBinding[] defaultBindings =
        {
            new DefaultBinding { actionId = MoveLeft, key = KeyCode.A },
            new DefaultBinding { actionId = MoveRight, key = KeyCode.D },
            new DefaultBinding { actionId = MoveUp, key = KeyCode.W },
            new DefaultBinding { actionId = MoveDown, key = KeyCode.S },
            new DefaultBinding { actionId = FireAction, key = KeyCode.Mouse0 },
            new DefaultBinding { actionId = AbilityAction, key = KeyCode.Space }
        };

        [Header("Smoothing")]
        [SerializeField, Tooltip("Responsiveness of aim smoothing (seconds to reach target).")]
        private float aimSmoothTime = 0.08f;
        [SerializeField, Tooltip("Responsiveness of lateral dodge smoothing.")]
        private float moveSmoothTime = 0.05f;
        [SerializeField, Tooltip("Touch swipe pixels per second required for max dodge speed.")]
        private float swipeSensitivity = 600f;

        [Header("Systems")]
        [SerializeField] private SaveManager saveManager;

        public event Action<Vector2> Move;
        public event Action<Vector2> Aim;
        public event Action FireStarted;
        public event Action FireCanceled;
        public event Action Ability;

        private readonly Dictionary<string, KeyCode> _bindings = new();
        private Vector2 _smoothedMove;
        private Vector2 _moveVelocity;
        private Vector2 _smoothedAim;
        private Vector2 _aimVelocity;
        private bool _isFireHeld;

#if ENABLE_INPUT_SYSTEM
        [Header("Input System (Optional)")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMap = "Gameplay";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string aimActionName = "Aim";
        [SerializeField] private string fireActionName = "Fire";
        [SerializeField] private string abilityActionName = "Ability";

        private InputAction _moveAction;
        private InputAction _aimAction;
        private InputAction _fireAction;
        private InputAction _abilityAction;
#endif

        private void Awake()
        {
            BootstrapBindings();
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            if (inputActions != null)
            {
                var map = inputActions.FindActionMap(actionMap, throwIfNotFound: false);
                if (map != null)
                {
                    _moveAction = map.FindAction(moveActionName, throwIfNotFound: false);
                    _aimAction = map.FindAction(aimActionName, throwIfNotFound: false);
                    _fireAction = map.FindAction(fireActionName, throwIfNotFound: false);
                    _abilityAction = map.FindAction(abilityActionName, throwIfNotFound: false);

                    _moveAction?.Enable();
                    _aimAction?.Enable();
                    _fireAction?.Enable();
                    _abilityAction?.Enable();

                    if (_fireAction != null)
                    {
                        _fireAction.performed += OnFirePerformed;
                        _fireAction.canceled += OnFireCanceled;
                    }

                    if (_abilityAction != null)
                    {
                        _abilityAction.performed += OnAbilityPerformed;
                    }
                }
            }
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            if (_fireAction != null)
            {
                _fireAction.performed -= OnFirePerformed;
                _fireAction.canceled -= OnFireCanceled;
            }

            if (_abilityAction != null)
            {
                _abilityAction.performed -= OnAbilityPerformed;
            }
#endif
        }

        private void Update()
        {
            var move = ReadMove();
            _smoothedMove = Vector2.SmoothDamp(_smoothedMove, move, ref _moveVelocity, moveSmoothTime);
            Move?.Invoke(_smoothedMove);

            var aim = ReadAim();
            _smoothedAim = Vector2.SmoothDamp(_smoothedAim, aim, ref _aimVelocity, aimSmoothTime);
            Aim?.Invoke(_smoothedAim);

            PollFire();
            PollAbility();
        }

        public void Rebind(string actionId, KeyCode key)
        {
            if (string.IsNullOrEmpty(actionId))
            {
                return;
            }

            _bindings[actionId] = key;
            if (saveManager != null)
            {
                saveManager.StoreKeyBinding(actionId, key);
            }
            else
            {
                PlayerPrefs.SetInt(actionId, (int)key);
                PlayerPrefs.Save();
            }
        }

        public KeyCode GetBinding(string actionId)
        {
            return _bindings.TryGetValue(actionId, out var key) ? key : KeyCode.None;
        }

        private void BootstrapBindings()
        {
            _bindings.Clear();
            foreach (var binding in defaultBindings)
            {
                if (string.IsNullOrEmpty(binding.actionId))
                {
                    continue;
                }

                var key = binding.key;
                if (saveManager != null)
                {
                    key = saveManager.LoadKeyBinding(binding.actionId, binding.key);
                }
                else if (PlayerPrefs.HasKey(binding.actionId))
                {
                    key = (KeyCode)PlayerPrefs.GetInt(binding.actionId);
                }

                _bindings[binding.actionId] = key;
            }
        }

        private Vector2 ReadMove()
        {
#if ENABLE_INPUT_SYSTEM
            if (_moveAction != null)
            {
                var value = _moveAction.ReadValue<Vector2>();
                if (value.sqrMagnitude > 0.0001f)
                {
                    return Vector2.ClampMagnitude(value, 1f);
                }
            }
#endif

            if (UnityEngine.Input.touchCount > 0)
            {
                var touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    var normalizedX = Mathf.Clamp(touch.deltaPosition.x / swipeSensitivity, -1f, 1f);
                    return new Vector2(normalizedX, 0f);
                }
            }

            var horizontal = 0f;
            if (IsBindingPressed(MoveLeft))
            {
                horizontal -= 1f;
            }
            if (IsBindingPressed(MoveRight))
            {
                horizontal += 1f;
            }

            var vertical = 0f;
            if (IsBindingPressed(MoveUp))
            {
                vertical += 1f;
            }
            if (IsBindingPressed(MoveDown))
            {
                vertical -= 1f;
            }

            return Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
        }

        private Vector2 ReadAim()
        {
#if ENABLE_INPUT_SYSTEM
            if (_aimAction != null)
            {
                var value = _aimAction.ReadValue<Vector2>();
                if (value.sqrMagnitude > 0.0001f)
                {
                    return Vector2.ClampMagnitude(value, 1f);
                }
            }
#endif

            if (UnityEngine.Input.touchCount > 0)
            {
                var touch = UnityEngine.Input.GetTouch(0);
                var viewport = new Vector2(
                    Mathf.Clamp01(touch.position.x / Screen.width) * 2f - 1f,
                    Mathf.Clamp01(touch.position.y / Screen.height) * 2f - 1f);
                return Vector2.ClampMagnitude(viewport, 1f);
            }

            var pointer = (Vector2)UnityEngine.Input.mousePosition;
            var aim = new Vector2(
                Mathf.Clamp(pointer.x / Screen.width * 2f - 1f, -1f, 1f),
                Mathf.Clamp(pointer.y / Screen.height * 2f - 1f, -1f, 1f));
            return aim;
        }

        private void PollFire()
        {
#if ENABLE_INPUT_SYSTEM
            if (_fireAction != null)
            {
                return; // Events already fired via callbacks.
            }
#endif
            var firePressed = IsBindingDown(FireAction) || UnityEngine.Input.GetMouseButtonDown(0);
            var fireReleased = IsBindingUp(FireAction) || UnityEngine.Input.GetMouseButtonUp(0);

            if (!_isFireHeld && firePressed)
            {
                _isFireHeld = true;
                FireStarted?.Invoke();
            }

            if (_isFireHeld && fireReleased)
            {
                _isFireHeld = false;
                FireCanceled?.Invoke();
            }

            // Quick tap auto-fire on touch screens.
            if (UnityEngine.Input.touchCount == 1)
            {
                var touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended && touch.deltaTime < 0.2f)
                {
                    FireStarted?.Invoke();
                    FireCanceled?.Invoke();
                }
            }
        }

        private void PollAbility()
        {
#if ENABLE_INPUT_SYSTEM
            if (_abilityAction != null)
            {
                return; // Input System events handle ability triggers.
            }
#endif
            if (IsBindingDown(AbilityAction))
            {
                Ability?.Invoke();
            }
        }

        private bool IsBindingPressed(string actionId)
        {
            if (!_bindings.TryGetValue(actionId, out var key))
            {
                return false;
            }

            return UnityEngine.Input.GetKey(key);
        }

        private bool IsBindingDown(string actionId)
        {
            if (!_bindings.TryGetValue(actionId, out var key))
            {
                return false;
            }

            return UnityEngine.Input.GetKeyDown(key);
        }

        private bool IsBindingUp(string actionId)
        {
            if (!_bindings.TryGetValue(actionId, out var key))
            {
                return false;
            }

            return UnityEngine.Input.GetKeyUp(key);
        }

#if ENABLE_INPUT_SYSTEM
        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            if (!_isFireHeld)
            {
                _isFireHeld = true;
                FireStarted?.Invoke();
            }
        }

        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            if (_isFireHeld)
            {
                _isFireHeld = false;
                FireCanceled?.Invoke();
            }
        }

        private void OnAbilityPerformed(InputAction.CallbackContext context)
        {
            Ability?.Invoke();
        }
#endif
    }
}
