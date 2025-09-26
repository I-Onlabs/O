using UnityEngine;

namespace AngryDogs.Player
{
    /// <summary>
    /// Slim orchestration layer that wires the player's dedicated coordinators together.
    /// Keeps high-level flow readable while each component follows the single-responsibility principle.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(PlayerShooter))]
    [RequireComponent(typeof(PlayerMovementCoordinator))]
    [RequireComponent(typeof(PlayerShootingCoordinator))]
    [RequireComponent(typeof(PlayerHealthResponder))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Coordinators")]
        [SerializeField] private PlayerMovementCoordinator movementCoordinator;
        [SerializeField] private PlayerShootingCoordinator shootingCoordinator;
        [SerializeField] private NibbleInteractionCoordinator nibbleCoordinator;
        [SerializeField] private PlayerHealthResponder healthResponder;

        private void Reset()
        {
            movementCoordinator = GetComponent<PlayerMovementCoordinator>();
            shootingCoordinator = GetComponent<PlayerShootingCoordinator>();
            healthResponder = GetComponent<PlayerHealthResponder>();

            if (nibbleCoordinator == null)
            {
                nibbleCoordinator = GetComponentInChildren<NibbleInteractionCoordinator>();
            }
        }

        private void Awake()
        {
            if (movementCoordinator == null)
            {
                movementCoordinator = GetComponent<PlayerMovementCoordinator>();
            }

            if (shootingCoordinator == null)
            {
                shootingCoordinator = GetComponent<PlayerShootingCoordinator>();
            }

            if (healthResponder == null)
            {
                healthResponder = GetComponent<PlayerHealthResponder>();
            }

            if (nibbleCoordinator == null)
            {
                nibbleCoordinator = GetComponentInChildren<NibbleInteractionCoordinator>();
            }
        }

        private void OnEnable()
        {
            if (healthResponder != null)
            {
                healthResponder.RileyDamaged += OnRileyDamaged;
                healthResponder.NibbleDamaged += OnNibbleDamaged;
                healthResponder.GameOver += OnGameOver;
            }
        }

        private void OnDisable()
        {
            if (healthResponder != null)
            {
                healthResponder.RileyDamaged -= OnRileyDamaged;
                healthResponder.NibbleDamaged -= OnNibbleDamaged;
                healthResponder.GameOver -= OnGameOver;
            }
        }

        private void OnRileyDamaged(float _)
        {
            nibbleCoordinator?.HandleRileyDamaged();
        }

        private void OnNibbleDamaged(float _)
        {
            shootingCoordinator?.HandleCompanionDamage();
        }

        private void OnGameOver()
        {
            shootingCoordinator?.HandleGameOver();
            nibbleCoordinator?.HandleGameOver();
        }
    }
}
