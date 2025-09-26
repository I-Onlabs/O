using UnityEngine;
using AngryDogs.Input;

namespace AngryDogs.Player
{
    /// <summary>
    /// Isolates aiming and firing responsibilities from the high-level player controller.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerShootingCoordinator : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private PlayerShooter shooter;

        private void Reset()
        {
            shooter = GetComponent<PlayerShooter>();
            inputManager = FindObjectOfType<InputManager>();
        }

        private void Awake()
        {
            if (shooter == null)
            {
                shooter = GetComponent<PlayerShooter>();
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
                inputManager.Aim += HandleAim;
                inputManager.FireStarted += HandleFireStarted;
                inputManager.FireCanceled += HandleFireCanceled;
            }
        }

        private void OnDisable()
        {
            if (inputManager != null)
            {
                inputManager.Aim -= HandleAim;
                inputManager.FireStarted -= HandleFireStarted;
                inputManager.FireCanceled -= HandleFireCanceled;
            }
        }

        public void HandleGameOver()
        {
            shooter?.EndFire();
        }

        public void HandleCompanionDamage()
        {
            shooter?.BoostFireRateTemporary();
        }

        private void HandleAim(Vector2 aim)
        {
            shooter?.ProcessAim(aim);
        }

        private void HandleFireStarted()
        {
            shooter?.BeginFire();
        }

        private void HandleFireCanceled()
        {
            shooter?.EndFire();
        }
    }
}
