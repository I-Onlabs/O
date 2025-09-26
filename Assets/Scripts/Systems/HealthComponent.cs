using System;
using UnityEngine;

namespace AngryDogs.Systems
{
    /// <summary>
    /// Minimal health container with events for damage and death notifications.
    /// </summary>
    public sealed class HealthComponent : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool clampNegative = true;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }

        public event Action<float> Damaged;
        public event Action<float> Healed;
        public event Action Died;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void ApplyDamage(float amount)
        {
            if (amount <= 0f || CurrentHealth <= 0f)
            {
                return;
            }

            CurrentHealth -= amount;
            if (clampNegative)
            {
                CurrentHealth = Mathf.Max(CurrentHealth, 0f);
            }

            Damaged?.Invoke(CurrentHealth);

            if (CurrentHealth <= 0f)
            {
                Died?.Invoke();
            }
        }

        public void Restore(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
            Healed?.Invoke(CurrentHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
        }
    }
}
