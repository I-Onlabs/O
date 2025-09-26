using System.Collections;
using UnityEngine;
using AngryDogs.Enemies;
using AngryDogs.Systems;

namespace AngryDogs.Player
{
    /// <summary>
    /// Controls Nibble's fetch loops and support abilities.
    /// </summary>
    public sealed class NibbleCompanionController : MonoBehaviour
    {
        [SerializeField] private float fetchCooldown = 6f;
        [SerializeField] private float abilityRadius = 6f;
        [SerializeField] private LayerMask houndMask;
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem abilityVfx;

        private Coroutine _fetchRoutine;

        private void OnEnable()
        {
            _fetchRoutine = StartCoroutine(FetchLoop());
        }

        private void OnDisable()
        {
            if (_fetchRoutine != null)
            {
                StopCoroutine(_fetchRoutine);
                _fetchRoutine = null;
            }
        }

        private IEnumerator FetchLoop()
        {
            var wait = new WaitForSeconds(fetchCooldown);
            while (true)
            {
                yield return wait;
                TriggerAbility();
            }
        }

        public void TriggerAbility()
        {
            animator?.SetTrigger("Bark");
            abilityVfx?.Play();

            var hits = Physics.OverlapSphere(transform.position, abilityRadius, houndMask);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out HoundAIController hound))
                {
                    hound.ApplyFear(2f);
                }
            }
        }

        public void OnRileyHit()
        {
            animator?.SetTrigger("Worried");
        }

        public void OnGameOver()
        {
            if (abilityVfx != null)
            {
                abilityVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
