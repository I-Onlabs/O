using System.Collections;
using UnityEngine;

namespace AngryDogs.Obstacles
{
    /// <summary>
    /// Handles goo trap behavior - can be repurposed into defense for Nibble.
    /// Riley: "These goo traps are like digital slime for cybernetic hounds!"
    /// Nibble: "Bark! (Translation: Gooey mess that can protect me!)"
    /// </summary>
    public class GooTrapComponent : MonoBehaviour
    {
        [Header("Goo Trap Settings")]
        [SerializeField] private float gooStunDuration = 2f;
        [SerializeField] private float gooSlowMultiplier = 0.3f;
        [SerializeField] private float gooShieldDuration = 8f;
        [SerializeField] private float gooShieldRadius = 2f;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem gooEffect;
        [SerializeField] private ParticleSystem shieldEffect;
        [SerializeField] private AudioClip gooSplashSound;
        [SerializeField] private AudioClip shieldActivateSound;
        
        private float _duration;
        private int _index;
        private bool _isRepurposed = false;
        private bool _isShieldActive = false;
        private AudioSource _audioSource;
        private Collider _collider;
        
        // Events
        public System.Action<GooTrapComponent> OnGooTrapActivated;
        public System.Action<GooTrapComponent> OnGooTrapRepurposed;
        public System.Action<GooTrapComponent> OnGooShieldActivated;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            _collider = GetComponent<Collider>();
        }

        /// <summary>
        /// Initializes the goo trap with duration and index.
        /// Riley: "Time to set up this goo trap!"
        /// </summary>
        public void Initialize(float duration, int index)
        {
            _duration = duration;
            _index = index;
            
            // Start the goo trap lifecycle
            StartCoroutine(GooTrapLifecycle());
            
            Debug.Log($"Nibble: *bark* (Translation: Goo trap {index} activated!)");
        }

        /// <summary>
        /// Main lifecycle of the goo trap.
        /// Riley: "This goo trap has a lifecycle - from obstacle to defense!"
        /// </summary>
        private IEnumerator GooTrapLifecycle()
        {
            // Phase 1: Active goo trap (obstacle)
            yield return StartCoroutine(ActiveGooTrapPhase());
            
            // Phase 2: Repurposed into defense
            if (!_isRepurposed)
            {
                RepurposeIntoDefense();
            }
            
            // Phase 3: Shield defense
            if (_isRepurposed)
            {
                yield return StartCoroutine(ShieldDefensePhase());
            }
        }

        /// <summary>
        /// Active goo trap phase - slows and stuns hounds.
        /// Nibble: "Bark! (Translation: The goo trap is active and dangerous!)"
        /// </summary>
        private IEnumerator ActiveGooTrapPhase()
        {
            OnGooTrapActivated?.Invoke(this);
            
            // Play goo effect
            if (gooEffect != null)
                gooEffect.Play();
            
            if (gooSplashSound != null && _audioSource != null)
                _audioSource.PlayOneShot(gooSplashSound);
            
            var startTime = Time.time;
            while (Time.time - startTime < _duration && !_isRepurposed)
            {
                // Check for hounds in the goo trap
                CheckForHoundsInGoo();
                yield return new WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// Checks for hounds in the goo trap and applies effects.
        /// Riley: "Gotta check if any hounds stepped in the goo!"
        /// </summary>
        private void CheckForHoundsInGoo()
        {
            var colliders = Physics.OverlapSphere(transform.position, _collider.radius);
            
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    // Apply goo effects to hounds
                    ApplyGooEffectsToHound(collider.gameObject);
                }
            }
        }

        /// <summary>
        /// Applies goo effects to a hound (slow and stun).
        /// Nibble: "Bark! (Translation: The hound is stuck in goo!)"
        /// </summary>
        private void ApplyGooEffectsToHound(GameObject hound)
        {
            // Get hound components
            var houndAI = hound.GetComponent<Enemies.HoundAIController>();
            var houndRigidbody = hound.GetComponent<Rigidbody>();
            
            if (houndAI != null)
            {
                // Slow down the hound
                houndAI.ApplySpeedModifier(gooSlowMultiplier, gooStunDuration);
                Debug.Log("Riley: The hound is slowed by the goo trap!");
            }
            
            if (houndRigidbody != null)
            {
                // Apply goo physics (sticky effect)
                houndRigidbody.drag = 5f; // Increase drag to simulate stickiness
                StartCoroutine(ResetHoundPhysics(houndRigidbody, gooStunDuration));
            }
        }

        /// <summary>
        /// Resets hound physics after goo effect wears off.
        /// Riley: "The hound is breaking free from the goo!"
        /// </summary>
        private IEnumerator ResetHoundPhysics(Rigidbody houndRigidbody, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (houndRigidbody != null)
            {
                houndRigidbody.drag = 1f; // Reset to normal drag
                Debug.Log("Nibble: *bark* (Translation: The hound broke free from the goo!)");
            }
        }

        /// <summary>
        /// Repurposes the goo trap into a defensive shield.
        /// Riley: "Time to turn this goo trap into a shield for Nibble!"
        /// </summary>
        public void RepurposeIntoDefense()
        {
            if (_isRepurposed) return;
            
            _isRepurposed = true;
            OnGooTrapRepurposed?.Invoke(this);
            
            // Change visual appearance
            if (gooEffect != null)
                gooEffect.Stop();
            
            if (shieldEffect != null)
                shieldEffect.Play();
            
            if (shieldActivateSound != null && _audioSource != null)
                _audioSource.PlayOneShot(shieldActivateSound);
            
            Debug.Log("Riley: Goo trap repurposed into defensive shield!");
            Debug.Log("Nibble: *happy bark* (Translation: I have a goo shield now!)");
        }

        /// <summary>
        /// Shield defense phase - protects Nibble from hounds.
        /// Nibble: "Bark! (Translation: The goo shield is protecting me!)"
        /// </summary>
        private IEnumerator ShieldDefensePhase()
        {
            OnGooShieldActivated?.Invoke(this);
            _isShieldActive = true;
            
            var startTime = Time.time;
            while (Time.time - startTime < gooShieldDuration)
            {
                // Check for hounds near Nibble and protect her
                ProtectNibbleFromHounds();
                yield return new WaitForSeconds(0.1f);
            }
            
            // Shield expires
            _isShieldActive = false;
            if (shieldEffect != null)
                shieldEffect.Stop();
            
            Debug.Log("Riley: The goo shield has expired!");
            Debug.Log("Nibble: *sad bark* (Translation: My goo shield is gone!)");
        }

        /// <summary>
        /// Protects Nibble from hounds using the goo shield.
        /// Riley: "The goo shield is protecting Nibble from the hounds!"
        /// </summary>
        private void ProtectNibbleFromHounds()
        {
            // Find Nibble
            var nibble = FindObjectOfType<Player.NibbleCompanionController>();
            if (nibble == null) return;
            
            var nibblePosition = nibble.transform.position;
            var distanceToNibble = Vector3.Distance(transform.position, nibblePosition);
            
            // If shield is close enough to Nibble, protect her
            if (distanceToNibble <= gooShieldRadius)
            {
                // Find hounds near Nibble
                var colliders = Physics.OverlapSphere(nibblePosition, gooShieldRadius);
                
                foreach (var collider in colliders)
                {
                    if (collider.CompareTag("Enemy"))
                    {
                        // Push hounds away from Nibble
                        PushHoundAwayFromNibble(collider.gameObject, nibblePosition);
                    }
                }
            }
        }

        /// <summary>
        /// Pushes a hound away from Nibble using the goo shield.
        /// Nibble: "Bark! (Translation: The goo shield is pushing the hound away!)"
        /// </summary>
        private void PushHoundAwayFromNibble(GameObject hound, Vector3 nibblePosition)
        {
            var houndRigidbody = hound.GetComponent<Rigidbody>();
            if (houndRigidbody == null) return;
            
            var direction = (hound.transform.position - nibblePosition).normalized;
            var pushForce = 10f;
            
            houndRigidbody.AddForce(direction * pushForce, ForceMode.Impulse);
            
            // Apply goo effects to the pushed hound
            ApplyGooEffectsToHound(hound);
            
            Debug.Log("Riley: The goo shield pushed a hound away from Nibble!");
        }

        /// <summary>
        /// Checks if the goo trap is currently repurposed as a shield.
        /// Riley: "Is this goo trap currently protecting Nibble?"
        /// </summary>
        public bool IsRepurposedAsShield => _isRepurposed && _isShieldActive;

        /// <summary>
        /// Gets the current state of the goo trap.
        /// Nibble: "Bark! (Translation: What state is the goo trap in?)"
        /// </summary>
        public string GetCurrentState()
        {
            if (_isShieldActive) return "Shield Active";
            if (_isRepurposed) return "Repurposed";
            return "Active Goo Trap";
        }

        private void OnDrawGizmosSelected()
        {
            // Draw goo trap radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _collider != null ? _collider.radius : 1f);
            
            // Draw shield radius if repurposed
            if (_isRepurposed)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, gooShieldRadius);
            }
        }
    }
}