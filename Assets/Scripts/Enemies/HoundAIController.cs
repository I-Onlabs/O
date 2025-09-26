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
        [SerializeField, Tooltip("Seconds between NavMesh repaths. Higher values save CPU at the cost of responsiveness.")]
        private float chaseRepathInterval = 0.2f;
        [SerializeField, Tooltip("How far Riley must move before a new path is requested.")]
        private float repathMovementThreshold = 0.75f;

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
        private float _nextRepathTime;
        private Vector3 _lastKnownRileyPosition;
        private float _attackRangeSqr;
        private float _repathMovementThresholdSqr;
        private int _isRunningHash;
        private int _biteHash;
        private int _updateOffset;
        private float _distanceCheckTimer;
        private const float DistanceCheckInterval = 0.1f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _attackRangeSqr = attackRange * attackRange;
            _repathMovementThresholdSqr = repathMovementThreshold * repathMovementThreshold;
            _isRunningHash = Animator.StringToHash("IsRunning");
            _biteHash = Animator.StringToHash("Bite");

            // Mobile-friendly agent defaults: turn off expensive rotation update if animator handles it.
            _agent.updateRotation = false;
            _agent.autoBraking = false;
            _distanceCheckTimer = DistanceCheckInterval;
        }

        private void OnEnable()
        {
            _activeCount++;
            GameEvents.RaiseHoundPackCountChanged(_activeCount);
            SetState(HoundState.Chasing);
            _nextRepathTime = Time.time;
            CacheRileyPosition();
            _updateOffset = _activeCount % 3;
            _distanceCheckTimer = 0f;
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

            // Reset lightweight state to play nice with pooling.
            _lastAttackTime = 0f;
            _nextRepathTime = 0f;
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }
        }

        private void Update()
        {
            // Stagger chasing logic across frames so 20+ hounds don't all hammer the CPU at once.
            if (_state == HoundState.Chasing)
            {
                if ((Time.frameCount + _updateOffset) % 3 == 0)
                {
                    UpdateChasing();
                }
            }
            else if (_state == HoundState.Attacking)
            {
                UpdateAttacking();
            }
        }

        private void LateUpdate()
        {
            if (_state == HoundState.Feared)
            {
                return; // Fear routine manually steers movement.
            }

            var velocity = _agent.velocity;
            velocity.y = 0f;
            if (velocity.sqrMagnitude > 0.001f)
            {
                var targetRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        private void UpdateChasing()
        {
            if (riley == null)
            {
                return;
            }

            if (ShouldRequestNewPath())
            {
                _agent.SetDestination(riley.position);
                CacheRileyPosition();
                _nextRepathTime = Time.time + chaseRepathInterval;
            }

            _distanceCheckTimer -= Time.deltaTime;
            if (_distanceCheckTimer <= 0f)
            {
                _distanceCheckTimer = DistanceCheckInterval;
            }
            else
            {
                return;
            }

            if (Vector3.SqrMagnitude(riley.position - transform.position) <= _attackRangeSqr)
            {
                SetState(HoundState.Attacking);
            }
        }

        private void UpdateAttacking()
        {
            if (riley == null)
            {
                SetState(HoundState.Chasing);
                return;
            }

            if (Vector3.SqrMagnitude(riley.position - transform.position) > _attackRangeSqr * 1.2f)
            {
                SetState(HoundState.Chasing);
                return;
            }

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

            animator?.SetTrigger(_biteHash);
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
                    if (animator != null)
                    {
                        animator.SetBool(_isRunningHash, true);
                    }
                    break;
                case HoundState.Attacking:
                    _agent.isStopped = true;
                    if (animator != null)
                    {
                        animator.SetBool(_isRunningHash, false);
                    }
                    break;
                case HoundState.Feared:
                    _agent.isStopped = false;
                    if (animator != null)
                    {
                        animator.SetBool(_isRunningHash, false);
                    }
                    break;
            }
        }

        private bool ShouldRequestNewPath()
        {
            if (Time.time < _nextRepathTime)
            {
                return false;
            }

            if (Vector3.SqrMagnitude(riley.position - _lastKnownRileyPosition) < _repathMovementThresholdSqr)
            {
                return false;
            }

            return true;
        }

        private void CacheRileyPosition()
        {
            _lastKnownRileyPosition = riley != null ? riley.position : Vector3.zero;
        }

        /// <summary>
        /// Allows pooled instances to get refreshed targets without extra FindObjectOfType calls.
        /// </summary>
        public void ConfigureTargets(Transform rileyTransform, HealthComponent rileyHp, HealthComponent nibbleHp)
        {
            riley = rileyTransform;
            rileyHealth = rileyHp;
            nibbleHealth = nibbleHp;
            CacheRileyPosition();
        }
    }
}
