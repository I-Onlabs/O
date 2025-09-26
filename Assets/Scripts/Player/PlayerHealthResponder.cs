using System;
using UnityEngine;
using AngryDogs.Systems;
using AngryDogs.Core;

namespace AngryDogs.Player
{
    /// <summary>
    /// Consolidates Riley and Nibble health callbacks and emits strongly typed events for other coordinators.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealthResponder : MonoBehaviour
    {
        [Header("Health References")]
        [SerializeField] private HealthComponent rileyHealth;
        [SerializeField] private HealthComponent nibbleHealth;

        [Header("Cooldowns")]
        [SerializeField, Tooltip("Seconds before Riley can complain about the same bite again.")]
        private float damageCooldown = 0.75f;

        public event Action<float> RileyDamaged;
        public event Action<float> NibbleDamaged;
        public event Action GameOver;

        private float _nextDamageTime;

        private void Awake()
        {
            if (rileyHealth == null)
            {
                rileyHealth = GetComponentInChildren<HealthComponent>();
            }
        }

        private void OnEnable()
        {
            if (rileyHealth != null)
            {
                rileyHealth.Damaged += HandleRileyDamaged;
                rileyHealth.Died += HandleGameOver;
            }

            if (nibbleHealth != null)
            {
                nibbleHealth.Damaged += HandleNibbleDamaged;
                nibbleHealth.Died += HandleGameOver;
            }
        }

        private void OnDisable()
        {
            if (rileyHealth != null)
            {
                rileyHealth.Damaged -= HandleRileyDamaged;
                rileyHealth.Died -= HandleGameOver;
            }

            if (nibbleHealth != null)
            {
                nibbleHealth.Damaged -= HandleNibbleDamaged;
                nibbleHealth.Died -= HandleGameOver;
            }
        }

        private void HandleRileyDamaged(float remaining)
        {
            if (Time.time < _nextDamageTime)
            {
                return;
            }

            _nextDamageTime = Time.time + damageCooldown;
            GameEvents.RaiseRileyHealthChanged(remaining);
            RileyDamaged?.Invoke(remaining);
        }

        private void HandleNibbleDamaged(float remaining)
        {
            GameEvents.RaiseNibbleHealthChanged(remaining);
            NibbleDamaged?.Invoke(remaining);
        }

        private void HandleGameOver()
        {
            GameEvents.RaiseGameOver();
            GameOver?.Invoke();
        }
    }
}
