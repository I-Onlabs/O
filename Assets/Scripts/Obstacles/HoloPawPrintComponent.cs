using UnityEngine;

namespace AngryDogs.Obstacles
{
    /// <summary>
    /// Handles holographic paw print behavior that misguides hounds away from Nibble.
    /// Nibble: "Bark! (Translation: Look, it's me but not me!)"
    /// </summary>
    public class HoloPawPrintComponent : MonoBehaviour
    {
        [SerializeField] private float attractionRadius = 3f;
        [SerializeField] private float attractionStrength = 2f;
        [SerializeField] private AudioClip holoSound;
        
        private float _duration;
        private int _printIndex;
        private bool _isActive = true;

        public void Initialize(float duration, int printIndex)
        {
            _duration = duration;
            _printIndex = printIndex;
            _isActive = true;
            
            // Start the countdown
            Invoke(nameof(Deactivate), duration);
        }

        private void Update()
        {
            if (!_isActive) return;

            // Attract nearby hounds to this decoy position
            AttractNearbyHounds();
        }

        private void AttractNearbyHounds()
        {
            // Find all hounds within attraction radius
            var colliders = Physics.OverlapSphere(transform.position, attractionRadius);
            
            foreach (var collider in colliders)
            {
                var houndAI = collider.GetComponent<Enemies.HoundAIController>();
                if (houndAI != null)
                {
                    // Apply attraction force to misguide the hound
                    var direction = (transform.position - collider.transform.position).normalized;
                    var rigidbody = collider.GetComponent<Rigidbody>();
                    
                    if (rigidbody != null)
                    {
                        rigidbody.AddForce(direction * attractionStrength * Time.deltaTime, ForceMode.Force);
                    }

                    // Riley: "Haha! The hounds are chasing holographic Nibble!"
                    if (_printIndex == 1) // Only log for the first print to avoid spam
                    {
                        Debug.Log("Riley: Hounds are being misled by holographic paw prints!");
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;

            // When a hound reaches this decoy, play a confused sound
            var houndAI = other.GetComponent<Enemies.HoundAIController>();
            if (houndAI != null)
            {
                Debug.Log("Nibble: *confused bark* (Translation: Why is the hound chasing nothing?)");
                
                if (holoSound != null)
                {
                    AudioSource.PlayClipAtPoint(holoSound, transform.position);
                }

                // Trigger hound confusion state
                // In a real game, you'd trigger a confusion animation/state here
            }
        }

        private void Deactivate()
        {
            _isActive = false;
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw attraction radius in editor
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, attractionRadius);
        }
    }
}