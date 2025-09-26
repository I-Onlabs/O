using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AngryDogs.Input
{
    /// <summary>
    /// Abstracts player input for keyboard/mouse, controller, and touch.
    /// Provides smoothed aiming/dodge commands via events consumed by gameplay components.
    /// </summary>
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        public event Action<Vector2> MoveInput;
        public event Action<Vector2> AimInput;
        public event Action FirePressed;
        public event Action FireReleased;
        public event Action AbilityPressed;

        [Header("Settings")]
        [SerializeField, Tooltip("Blend factor per second for smoothing aim input.")]
        private float aimSmoothing = 12f;
        [SerializeField, Tooltip("Seconds that qualify as a quick tap for auto-fire on touch.")]
        private float tapThreshold = 0.2f;

        private Vector2 _currentAim;

#if ENABLE_INPUT_SYSTEM
        [SerializeField] private InputActionAsset actions;
        private InputAction _moveAction;
        private InputAction _aimAction;
        private InputAction _fireAction;
        private InputAction _abilityAction;
#endif

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            if (actions != null)
            {
                _moveAction = actions.FindAction("Gameplay/Move");
                _aimAction = actions.FindAction("Gameplay/Aim");
                _fireAction = actions.FindAction("Gameplay/Fire");
                _abilityAction = actions.FindAction("Gameplay/Ability");

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
            MoveInput?.Invoke(move);

            var rawAim = ReadAim();
            _currentAim = Vector2.Lerp(_currentAim, rawAim, 1f - Mathf.Exp(-aimSmoothing * Time.deltaTime));
            AimInput?.Invoke(_currentAim);

#if !ENABLE_INPUT_SYSTEM
            // Legacy polling for fire/ability
            if (UnityEngine.Input.GetButtonDown("Fire1"))
            {
                FirePressed?.Invoke();
            }
            if (UnityEngine.Input.GetButtonUp("Fire1"))
            {
                FireReleased?.Invoke();
            }
            if (UnityEngine.Input.GetButtonDown("Fire2"))
            {
                AbilityPressed?.Invoke();
            }
#endif
        }

        private Vector2 ReadMove()
        {
#if ENABLE_INPUT_SYSTEM
            if (_moveAction != null)
            {
                return _moveAction.ReadValue<Vector2>();
            }
#endif
            // Legacy fallback: horizontal axis = dodge lanes, vertical for speed boosts.
            return new Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical"));
        }

        private Vector2 ReadAim()
        {
#if ENABLE_INPUT_SYSTEM
            if (_aimAction != null)
            {
                return _aimAction.ReadValue<Vector2>();
            }
#endif
            var pointer = (Vector2)UnityEngine.Input.mousePosition;
            if (UnityEngine.Input.touchCount > 0)
            {
                pointer = UnityEngine.Input.GetTouch(0).position;
            }

            var viewportPoint = new Vector2(
                pointer.x / Screen.width * 2f - 1f,
                pointer.y / Screen.height * 2f - 1f);
            return viewportPoint;
        }

#if ENABLE_INPUT_SYSTEM
        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            FirePressed?.Invoke();
        }

        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            FireReleased?.Invoke();
        }

        private void OnAbilityPerformed(InputAction.CallbackContext context)
        {
            AbilityPressed?.Invoke();
        }
#endif

        private void LateUpdate()
        {
            // Quick tap detection for touch screens (auto-fire).
            if (UnityEngine.Input.touchCount == 0)
            {
                return;
            }

            var touch = UnityEngine.Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended && touch.deltaTime <= tapThreshold)
            {
                FirePressed?.Invoke();
                FireReleased?.Invoke();
            }
        }
    }
}
