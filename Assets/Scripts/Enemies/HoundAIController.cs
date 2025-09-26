using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using AngryDogs.Core;
using AngryDogs.Systems;

namespace AngryDogs.Enemies
{
    /// <summary>
    /// Optimized hound AI controller with lightweight state machine.
    /// Supports fear debuff and pooled activation.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class HoundAIController : MonoBehaviour
    {
        private enum HoundState
        {
            Chasing,
            Attacking,
            Feared
        }

        [Header("Stats")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackInterval = 1.5f;
        [SerializeField] private float damage = 10f;

        [Header("Targets")]
        [SerializeField] private Transform riley;
        [SerializeField] private HealthComponent rileyHealth;
        [SerializeField] private HealthComponent nibbleHealth;

        [Header("Feedback")]
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem fearVfx;

        private static int _activeCount;

        private NavMeshAgent _agent;
        private HoundState _state;
        private float _lastAttackTime;
        private Coroutine _fearRoutine;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            _activeCount++;
            GameEvents.RaiseHoundPackCountChanged(_activeCount);
            SetState(HoundState.Chasing);
        }

        private void OnDisable()
        {
            _activeCount = Mathf.Max(0, _activeCount - 1);
            GameEvents.RaiseHoundPackCountChanged(_activeCount);

            if (_fearRoutine != null)
            {
                StopCoroutine(_fearRoutine);
                _fearRoutine = null;
            }
        }

        private void Update()
        {
            switch (_state)
            {
                case HoundState.Chasing:
                    UpdateChasing();
                    break;
                case HoundState.Attacking:
                    UpdateAttacking();
                    break;
            }
        }

        private void UpdateChasing()
        {
            if (riley == null)
            {
                return;
            }

            _agent.SetDestination(riley.position);
            if (Vector3.SqrMagnitude(riley.position - transform.position) <= attackRange * attackRange)
            {
                SetState(HoundState.Attacking);
            }
        }

        private void UpdateAttacking()
        {
            if (Time.time - _lastAttackTime < attackInterval)
            {
                return;
            }

            _lastAttackTime = Time.time;
            if (rileyHealth != null)
            {
                rileyHealth.ApplyDamage(damage);
            }

            if (nibbleHealth != null)
            {
                nibbleHealth.ApplyDamage(damage * 0.5f);
            }

            animator?.SetTrigger("Bite");
        }

        public void ApplyFear(float duration)
        {
            if (_fearRoutine != null)
            {
                StopCoroutine(_fearRoutine);
            }

            _fearRoutine = StartCoroutine(FearRoutine(duration));
        }

        private IEnumerator FearRoutine(float duration)
        {
            SetState(HoundState.Feared);
            fearVfx?.Play();
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _agent.Move(-transform.forward * _agent.speed * 0.5f * Time.deltaTime);
                yield return null;
            }

            fearVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            SetState(HoundState.Chasing);
        }

        private void SetState(HoundState newState)
        {
            _state = newState;
            switch (_state)
            {
                case HoundState.Chasing:
                    _agent.isStopped = false;
                    animator?.SetBool("IsRunning", true);
                    break;
                case HoundState.Attacking:
                    _agent.isStopped = true;
                    animator?.SetBool("IsRunning", false);
                    break;
                case HoundState.Feared:
                    _agent.isStopped = false;
                    animator?.SetBool("IsRunning", false);
                    break;
            }
        }
    }
}
