using UnityEngine;
using AngryDogs.Core;
using AngryDogs.Input;
using AngryDogs.Systems;

namespace AngryDogs.Player
{
    /// <summary>
    /// High-level orchestrator for the player character. Coordinates movement, shooting, and companion behaviours.
    /// </summary>
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(PlayerShooter))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private NibbleCompanionController nibble;
        [SerializeField] private HealthComponent rileyHealth;
        [SerializeField] private HealthComponent nibbleHealth;

        [Header("Settings")]
        [SerializeField] private float damageCooldown = 0.75f;

        private float _nextDamageTime;

        private void Reset()
        {
            movement = GetComponent<PlayerMovementController>();
            shooter = GetComponent<PlayerShooter>();
        }

        private void Awake()
        {
            if (inputHandler == null)
            {
                inputHandler = FindObjectOfType<PlayerInputHandler>();
            }
        }

        private void OnEnable()
        {
            if (inputHandler != null)
            {
                inputHandler.MoveInput += OnMove;
                inputHandler.AimInput += OnAim;
                inputHandler.FirePressed += OnFirePressed;
                inputHandler.FireReleased += OnFireReleased;
                inputHandler.AbilityPressed += OnAbilityPressed;
            }

            if (rileyHealth != null)
            {
                rileyHealth.Damaged += OnRileyDamaged;
                rileyHealth.Died += HandleGameOver;
            }

            if (nibbleHealth != null)
            {
                nibbleHealth.Damaged += OnNibbleDamaged;
                nibbleHealth.Died += HandleGameOver;
            }
        }

        private void OnDisable()
        {
            if (inputHandler != null)
            {
                inputHandler.MoveInput -= OnMove;
                inputHandler.AimInput -= OnAim;
                inputHandler.FirePressed -= OnFirePressed;
                inputHandler.FireReleased -= OnFireReleased;
                inputHandler.AbilityPressed -= OnAbilityPressed;
            }

            if (rileyHealth != null)
            {
                rileyHealth.Damaged -= OnRileyDamaged;
                rileyHealth.Died -= HandleGameOver;
            }

            if (nibbleHealth != null)
            {
                nibbleHealth.Damaged -= OnNibbleDamaged;
                nibbleHealth.Died -= HandleGameOver;
            }
        }

        private void OnMove(Vector2 move)
        {
            movement.ProcessInput(move);
        }

        private void OnAim(Vector2 aim)
        {
            shooter.ProcessAim(aim);
        }

        private void OnFirePressed()
        {
            shooter.BeginFire();
        }

        private void OnFireReleased()
        {
            shooter.EndFire();
        }

        private void OnAbilityPressed()
        {
            nibble?.TriggerAbility();
        }

        private void OnRileyDamaged(float remainingHealth)
        {
            if (Time.time < _nextDamageTime)
            {
                return;
            }

            _nextDamageTime = Time.time + damageCooldown;
            GameEvents.RaiseRileyHealthChanged(remainingHealth);
            nibble?.OnRileyHit();
        }

        private void OnNibbleDamaged(float remainingHealth)
        {
            GameEvents.RaiseNibbleHealthChanged(remainingHealth);
            shooter?.BoostFireRateTemporary();
        }

        private void HandleGameOver()
        {
            GameEvents.RaiseGameOver();
            shooter.EndFire();
            nibble?.OnGameOver();
        }
    }
}
