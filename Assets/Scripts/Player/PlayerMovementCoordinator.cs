using UnityEngine;
using AngryDogs.Input;

namespace AngryDogs.Player
{
    /// <summary>
    /// Bridges raw player input to the movement controller. Keeps lane dodging logic isolated.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerMovementCoordinator : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private PlayerMovementController movement;

        private void Reset()
        {
            movement = GetComponent<PlayerMovementController>();
            inputManager = FindObjectOfType<InputManager>();
        }

        private void Awake()
        {
            if (movement == null)
            {
                movement = GetComponent<PlayerMovementController>();
            }

            if (inputManager == null)
            {
                inputManager = FindObjectOfType<InputManager>();
            }
        }

        private void OnEnable()
        {
            if (inputManager != null)
            {
                inputManager.Move += HandleMove;
            }
        }

        private void OnDisable()
        {
            if (inputManager != null)
            {
                inputManager.Move -= HandleMove;
            }
        }

        private void HandleMove(Vector2 move)
        {
            movement?.ProcessInput(move);
        }
    }
}
