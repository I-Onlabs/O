using UnityEngine;

namespace AngryDogs.Obstacles
{
    /// <summary>
    /// Handles sticky pellet behavior that makes hounds slip comically.
    /// Riley: "These pellets are like digital banana peels for cybernetic hounds!"
    /// </summary>
    public class StickyPelletComponent : MonoBehaviour
    {
        [SerializeField] private float slipForce = 5f;
        [SerializeField] private float slipDuration = 2f;
        [SerializeField] private AudioClip slipSound;
        
        private float _duration;
        private bool _isActive = true;

        public void Initialize(float duration)
        {
            _duration = duration;
            _isActive = true;
            
            // Start the countdown
            Invoke(nameof(Deactivate), duration);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;

            // Check if it's a hound
            var houndAI = other.GetComponent<Enemies.HoundAIController>();
            if (houndAI != null)
            {
                MakeHoundSlip(other);
            }
        }

        private void MakeHoundSlip(Collider hound)
        {
            // Riley: "Haha! Look at that hound slip and slide!"
            Debug.Log("Riley: Hound hit a sticky pellet! Time for some comedic slipping!");
            
            // Apply slip effect to the hound
            var rigidbody = hound.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                // Apply random slip force
                var slipDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    0,
                    Random.Range(-1f, 1f)
                ).normalized;
                
                rigidbody.AddForce(slipDirection * slipForce, ForceMode.Impulse);
                
                // Add some rotation for comedic effect
                rigidbody.AddTorque(Vector3.up * Random.Range(-180f, 180f), ForceMode.Impulse);
            }

            // Play slip sound
            if (slipSound != null)
            {
                AudioSource.PlayClipAtPoint(slipSound, transform.position);
            }

            // Trigger hound slip animation/state
            var houndAI = hound.GetComponent<Enemies.HoundAIController>();
            if (houndAI != null)
            {
                // In a real game, you'd trigger a slip animation state here
                Debug.Log("Nibble: *happy bark* (Translation: Bad hound is slipping!)");
            }

            // Deactivate this pellet after use
            _isActive = false;
            gameObject.SetActive(false);
        }

        private void Deactivate()
        {
            _isActive = false;
            Destroy(gameObject);
        }
    }
}