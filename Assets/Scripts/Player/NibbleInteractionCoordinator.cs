using UnityEngine;
using AngryDogs.Input;

namespace AngryDogs.Player
{
    /// <summary>
    /// Responsible solely for coordinating Nibble's interactions and ability triggers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NibbleInteractionCoordinator : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private NibbleCompanionController nibble;

        private void Reset()
        {
            if (nibble == null)
            {
                nibble = GetComponentInChildren<NibbleCompanionController>();
            }

            inputManager = FindObjectOfType<InputManager>();
        }

        private void Awake()
        {
            if (nibble == null)
            {
                nibble = GetComponentInChildren<NibbleCompanionController>();
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
                inputManager.Ability += HandleAbility;
            }
        }

        private void OnDisable()
        {
            if (inputManager != null)
            {
                inputManager.Ability -= HandleAbility;
            }
        }

        public void HandleRileyDamaged()
        {
            nibble?.OnRileyHit();
        }

        public void HandleGameOver()
        {
            nibble?.OnGameOver();
        }

        private void HandleAbility()
        {
            nibble?.TriggerAbility();
        }
    }
}
