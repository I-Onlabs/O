using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AngryDogs.Core;
using AngryDogs.Systems;
using AngryDogs.Obstacles;

namespace AngryDogs.Enemies
{
    /// <summary>
    /// Cyber-Chihuahua King boss - a tiny hound in a massive mech-suit with weak points and Treat Tantrum defense.
    /// Riley: "That's the smallest boss I've ever seen! But that mech-suit looks dangerous..."
    /// Nibble: "Bark! (Translation: Even I'm bigger than that chihuahua!)"
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class BossHound : MonoBehaviour
    {
        [System.Serializable]
        public class WeakPoint
        {
            public Transform transform;
            public string name = "Weak Point";
            public float health = 50f;
            public float maxHealth = 50f;
            public bool isDestroyed = false;
            public GameObject visualEffect;
            public AudioClip hitSound;
            public AudioClip destroySound;
        }

        [System.Serializable]
        public class TreatTantrumPhase
        {
            public string name = "Treat Tantrum";
            public float duration = 10f;
            public float damageMultiplier = 2f;
            public float speedMultiplier = 1.5f;
            public GameObject visualEffect;
            public AudioClip tantrumSound;
        }

        [Header("Boss Stats")]
        [SerializeField] private float maxHealth = 500f;
        [SerializeField] private float attackDamage = 25f;
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float attackInterval = 2f;
        [SerializeField] private float moveSpeed = 3f;

        [Header("Weak Points")]
        [SerializeField] private List<WeakPoint> weakPoints = new List<WeakPoint>();
        [SerializeField] private int weakPointsToDestroy = 3;

        [Header("Treat Tantrum")]
        [SerializeField] private TreatTantrumPhase treatTantrumPhase;
        [SerializeField] private float tantrumTriggerHealth = 0.3f; // Trigger at 30% health
        [SerializeField] private float tantrumCooldown = 15f;

        [Header("Targets")]
        [SerializeField] private Transform riley;
        [SerializeField] private Transform nibble;
        [SerializeField] private HealthComponent rileyHealth;
        [SerializeField] private HealthComponent nibbleHealth;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem mechSmoke;
        [SerializeField] private ParticleSystem treatTantrumEffect;
        [SerializeField] private GameObject mechSuit;
        [SerializeField] private GameObject chihuahuaCore;

        [Header("Audio")]
        [SerializeField] private AudioSource bossAudioSource;
        [SerializeField] private AudioClip bossLaugh;
        [SerializeField] private AudioClip weakPointHit;
        [SerializeField] private AudioClip mechDamage;
        [SerializeField] private AudioClip bossDefeat;

        private NavMeshAgent _agent;
        private HealthComponent _healthComponent;
        private Animator _animator;
        
        // Boss state
        private bool _isInTantrum = false;
        private bool _canTantrum = true;
        private float _lastAttackTime;
        private int _destroyedWeakPoints = 0;
        private Vector3 _lastKnownRileyPosition;
        private bool _isDefeated = false;

        // Events
        public System.Action<BossHound> OnBossDefeated;
        public System.Action<WeakPoint> OnWeakPointDestroyed;
        public System.Action OnTreatTantrumStarted;
        public System.Action OnTreatTantrumEnded;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _healthComponent = GetComponent<HealthComponent>();
            _animator = GetComponent<Animator>();

            // Initialize health component
            _healthComponent.Initialize(maxHealth);
            _healthComponent.OnHealthChanged += HandleHealthChanged;
            _healthComponent.OnDeath += HandleBossDeath;

            // Initialize weak points
            InitializeWeakPoints();

            // Set up agent
            _agent.speed = moveSpeed;
            _agent.stoppingDistance = attackRange * 0.8f;

            Debug.Log("Riley: The Cyber-Chihuahua King has appeared! Time to take down this tiny tyrant!");
        }

        private void Start()
        {
            // Find targets if not assigned
            if (riley == null)
                riley = FindObjectOfType<Player.PlayerController>()?.transform;
            if (nibble == null)
                nibble = FindObjectOfType<Player.NibbleCompanionController>()?.transform;

            if (riley != null)
                rileyHealth = riley.GetComponent<HealthComponent>();
            if (nibble != null)
                nibbleHealth = nibble.GetComponent<HealthComponent>();

            // Start boss behavior
            StartCoroutine(BossBehaviorLoop());
        }

        /// <summary>
        /// Initializes weak points with proper health values.
        /// Riley: "Gotta identify the weak spots on this mech-suit!"
        /// </summary>
        private void InitializeWeakPoints()
        {
            foreach (var weakPoint in weakPoints)
            {
                weakPoint.health = weakPoint.maxHealth;
                weakPoint.isDestroyed = false;
                
                if (weakPoint.visualEffect != null)
                    weakPoint.visualEffect.SetActive(true);
            }

            Debug.Log($"Nibble: *bark* (Translation: Found {weakPoints.Count} weak points on the mech-suit!)");
        }

        /// <summary>
        /// Main boss behavior loop that handles movement, attacks, and phase changes.
        /// Riley: "This chihuahua is more strategic than I expected!"
        /// </summary>
        private IEnumerator BossBehaviorLoop()
        {
            while (!_isDefeated)
            {
                if (riley == null)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                // Update last known position
                _lastKnownRileyPosition = riley.position;

                // Check if we can attack
                if (CanAttack())
                {
                    yield return StartCoroutine(PerformAttack());
                }
                else
                {
                    // Move towards Riley
                    MoveTowardsTarget();
                }

                // Check for treat tantrum trigger
                CheckTreatTantrumTrigger();

                yield return new WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// Checks if the boss can attack Riley.
        /// Nibble: "Bark! (Translation: Is the chihuahua close enough to attack?)"
        /// </summary>
        private bool CanAttack()
        {
            if (riley == null) return false;
            
            var distance = Vector3.Distance(transform.position, riley.position);
            var timeSinceLastAttack = Time.time - _lastAttackTime;
            
            return distance <= attackRange && timeSinceLastAttack >= attackInterval;
        }

        /// <summary>
        /// Performs an attack on Riley.
        /// Riley: "That tiny chihuahua hits harder than expected!"
        /// </summary>
        private IEnumerator PerformAttack()
        {
            _lastAttackTime = Time.time;
            
            // Face Riley
            var direction = (riley.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);

            // Play attack animation
            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
            }

            // Play attack sound
            if (bossAudioSource != null && bossLaugh != null)
            {
                bossAudioSource.PlayOneShot(bossLaugh);
            }

            // Wait for attack animation
            yield return new WaitForSeconds(0.5f);

            // Deal damage to Riley
            if (rileyHealth != null)
            {
                var damage = _isInTantrum ? attackDamage * treatTantrumPhase.damageMultiplier : attackDamage;
                rileyHealth.TakeDamage(damage);
                
                Debug.Log($"Riley: Ouch! The Cyber-Chihuahua King dealt {damage} damage!");
            }

            // Try to damage Nibble too (but with less damage)
            if (nibbleHealth != null && Random.Range(0f, 1f) < 0.3f)
            {
                var nibbleDamage = attackDamage * 0.5f;
                nibbleHealth.TakeDamage(nibbleDamage);
                
                Debug.Log("Nibble: *yelp* (Translation: The chihuahua got me too!)");
            }
        }

        /// <summary>
        /// Moves the boss towards Riley.
        /// Riley: "Here it comes! That mech-suit is faster than it looks!"
        /// </summary>
        private void MoveTowardsTarget()
        {
            if (riley == null) return;

            _agent.SetDestination(riley.position);
            
            // Update speed based on tantrum state
            _agent.speed = _isInTantrum ? moveSpeed * treatTantrumPhase.speedMultiplier : moveSpeed;
        }

        /// <summary>
        /// Checks if treat tantrum should be triggered.
        /// Nibble: "Bark! (Translation: The chihuahua looks really angry!)"
        /// </summary>
        private void CheckTreatTantrumTrigger()
        {
            if (_isInTantrum || !_canTantrum) return;

            var healthPercentage = _healthComponent.CurrentHealth / _healthComponent.MaxHealth;
            if (healthPercentage <= tantrumTriggerHealth)
            {
                StartCoroutine(TriggerTreatTantrum());
            }
        }

        /// <summary>
        /// Triggers the treat tantrum phase.
        /// Riley: "Oh no! The chihuahua is having a treat tantrum! This is going to be messy!"
        /// </summary>
        private IEnumerator TriggerTreatTantrum()
        {
            _isInTantrum = true;
            _canTantrum = false;
            
            Debug.Log("Riley: The Cyber-Chihuahua King is having a treat tantrum! Look out!");
            
            // Play tantrum effects
            if (treatTantrumEffect != null)
                treatTantrumEffect.Play();
            
            if (bossAudioSource != null && treatTantrumPhase.tantrumSound != null)
                bossAudioSource.PlayOneShot(treatTantrumPhase.tantrumSound);

            // Activate visual effects
            if (treatTantrumPhase.visualEffect != null)
                treatTantrumPhase.visualEffect.SetActive(true);

            OnTreatTantrumStarted?.Invoke();

            // Tantrum duration
            yield return new WaitForSeconds(treatTantrumPhase.duration);

            // End tantrum
            EndTreatTantrum();
        }

        /// <summary>
        /// Ends the treat tantrum phase.
        /// Nibble: "Bark! (Translation: The tantrum is over, but the chihuahua is still dangerous!)"
        /// </summary>
        private void EndTreatTantrum()
        {
            _isInTantrum = false;
            
            // Stop tantrum effects
            if (treatTantrumEffect != null)
                treatTantrumEffect.Stop();
            
            if (treatTantrumPhase.visualEffect != null)
                treatTantrumPhase.visualEffect.SetActive(false);

            OnTreatTantrumEnded?.Invoke();

            // Start cooldown
            StartCoroutine(TantrumCooldown());
            
            Debug.Log("Riley: The treat tantrum is over, but that chihuahua is still dangerous!");
        }

        /// <summary>
        /// Handles tantrum cooldown period.
        /// Riley: "The chihuahua is recovering from its tantrum..."
        /// </summary>
        private IEnumerator TantrumCooldown()
        {
            yield return new WaitForSeconds(tantrumCooldown);
            _canTantrum = true;
            
            Debug.Log("Nibble: *bark* (Translation: The chihuahua can tantrum again!)");
        }

        /// <summary>
        /// Handles damage to weak points.
        /// Riley: "Hit the weak points! That's the only way to damage this mech-suit!"
        /// </summary>
        public void DamageWeakPoint(WeakPoint weakPoint, float damage)
        {
            if (weakPoint.isDestroyed) return;

            weakPoint.health -= damage;
            
            // Play hit effects
            if (weakPoint.hitSound != null && bossAudioSource != null)
                bossAudioSource.PlayOneShot(weakPoint.hitSound);

            Debug.Log($"Riley: Hit {weakPoint.name} for {damage} damage! ({weakPoint.health:F1}/{weakPoint.maxHealth:F1})");

            if (weakPoint.health <= 0)
            {
                DestroyWeakPoint(weakPoint);
            }
        }

        /// <summary>
        /// Destroys a weak point.
        /// Riley: "One weak point down! Keep hitting the others!"
        /// </summary>
        private void DestroyWeakPoint(WeakPoint weakPoint)
        {
            weakPoint.isDestroyed = true;
            weakPoint.health = 0;
            _destroyedWeakPoints++;

            // Play destroy effects
            if (weakPoint.destroySound != null && bossAudioSource != null)
                bossAudioSource.PlayOneShot(weakPoint.destroySound);

            if (weakPoint.visualEffect != null)
                weakPoint.visualEffect.SetActive(false);

            OnWeakPointDestroyed?.Invoke(weakPoint);

            Debug.Log($"Riley: {weakPoint.name} destroyed! ({_destroyedWeakPoints}/{weakPointsToDestroy} weak points down)");

            // Check if boss should be defeated
            if (_destroyedWeakPoints >= weakPointsToDestroy)
            {
                DefeatBoss();
            }
        }

        /// <summary>
        /// Defeats the boss.
        /// Riley: "We did it! The Cyber-Chihuahua King is defeated!"
        /// Nibble: "Bark! (Translation: Victory! We beat the tiny tyrant!)"
        /// </summary>
        private void DefeatBoss()
        {
            _isDefeated = true;
            
            // Play defeat effects
            if (bossAudioSource != null && bossDefeat != null)
                bossAudioSource.PlayOneShot(bossDefeat);

            // Disable mech suit, show chihuahua core
            if (mechSuit != null)
                mechSuit.SetActive(false);
            if (chihuahuaCore != null)
                chihuahuaCore.SetActive(true);

            // Stop agent
            _agent.enabled = false;

            // Trigger defeat animation
            if (_animator != null)
                _animator.SetTrigger("Defeat");

            OnBossDefeated?.Invoke(this);

            Debug.Log("Riley: The Cyber-Chihuahua King is defeated! The tiny chihuahua emerges from its mech-suit!");
            Debug.Log("Nibble: *happy bark* (Translation: We won! The chihuahua is just a regular dog now!)");

            // Destroy after a delay
            Destroy(gameObject, 5f);
        }

        /// <summary>
        /// Handles health changes for the boss.
        /// Riley: "The mech-suit is taking damage!"
        /// </summary>
        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            // Play damage effects
            if (mechSmoke != null && currentHealth < maxHealth * 0.5f)
                mechSmoke.Play();

            if (bossAudioSource != null && mechDamage != null)
                bossAudioSource.PlayOneShot(mechDamage);
        }

        /// <summary>
        /// Handles boss death.
        /// Riley: "The Cyber-Chihuahua King is down for good!"
        /// </summary>
        private void HandleBossDeath()
        {
            if (!_isDefeated)
            {
                DefeatBoss();
            }
        }

        /// <summary>
        /// Gets the closest weak point to a position.
        /// Riley: "Need to find the closest weak point to target!"
        /// </summary>
        public WeakPoint GetClosestWeakPoint(Vector3 position)
        {
            WeakPoint closest = null;
            float closestDistance = float.MaxValue;

            foreach (var weakPoint in weakPoints)
            {
                if (weakPoint.isDestroyed) continue;

                var distance = Vector3.Distance(position, weakPoint.transform.position);
                if (distance < closestDistance)
                {
                    closest = weakPoint;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        /// <summary>
        /// Gets all active weak points.
        /// Nibble: "Bark! (Translation: Show me all the weak spots!)"
        /// </summary>
        public List<WeakPoint> GetActiveWeakPoints()
        {
            var activeWeakPoints = new List<WeakPoint>();
            foreach (var weakPoint in weakPoints)
            {
                if (!weakPoint.isDestroyed)
                    activeWeakPoints.Add(weakPoint);
            }
            return activeWeakPoints;
        }

        /// <summary>
        /// Checks if the boss is in treat tantrum phase.
        /// Riley: "Is that chihuahua still having a tantrum?"
        /// </summary>
        public bool IsInTantrum => _isInTantrum;

        /// <summary>
        /// Gets the boss's current phase name.
        /// Nibble: "Bark! (Translation: What phase is the chihuahua in?)"
        /// </summary>
        public string CurrentPhase => _isInTantrum ? treatTantrumPhase.name : "Normal";

        private void OnDrawGizmosSelected()
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw weak points
            Gizmos.color = Color.yellow;
            foreach (var weakPoint in weakPoints)
            {
                if (weakPoint.transform != null && !weakPoint.isDestroyed)
                {
                    Gizmos.DrawWireSphere(weakPoint.transform.position, 0.5f);
                }
            }
        }
    }
}