using System.Collections;
using UnityEngine;
using AngryDogs.Core;
using AngryDogs.Obstacles;

namespace AngryDogs.Enemies
{
    /// <summary>
    /// Spawns the Cyber-Chihuahua King boss at checkpoints and creates dynamic arenas.
    /// Riley: "Time to face the ultimate challenge - the Cyber-Chihuahua King!"
    /// Nibble: "Bark! (Translation: I'm ready to take on that tiny tyrant!)"
    /// </summary>
    public sealed class BossSpawner : MonoBehaviour
    {
        [Header("Boss Spawning")]
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnCheckpointDistance = 1000f; // Spawn every 1000 units
        [SerializeField] private float bossArenaRadius = 50f;
        [SerializeField] private int maxBosses = 1;

        [Header("Arena Obstacles")]
        [SerializeField] private ObstacleManager obstacleManager;
        [SerializeField] private GameObject[] arenaObstaclePrefabs;
        [SerializeField] private int arenaObstacleCount = 8;
        [SerializeField] private float arenaObstacleSpacing = 10f;

        [Header("Boss Events")]
        [SerializeField] private AudioClip bossSpawnSound;
        [SerializeField] private AudioClip bossDefeatSound;
        [SerializeField] private ParticleSystem bossSpawnEffect;

        private Transform _player;
        private float _lastBossSpawnDistance;
        private BossHound _currentBoss;
        private bool _isBossActive = false;
        private int _bossesDefeated = 0;

        // Events
        public System.Action<BossHound> OnBossSpawned;
        public System.Action<BossHound> OnBossDefeated;
        public System.Action<int> OnBossCheckpointReached;

        private void Start()
        {
            // Find player
            _player = FindObjectOfType<Player.PlayerController>()?.transform;
            if (_player == null)
            {
                Debug.LogError("Riley: No player found! Can't spawn bosses without a target!");
                return;
            }

            // Find obstacle manager
            if (obstacleManager == null)
                obstacleManager = FindObjectOfType<ObstacleManager>();

            Debug.Log("Nibble: *bark* (Translation: Boss spawner ready! The chihuahua king will appear soon!)");
        }

        private void Update()
        {
            if (_player == null || _isBossActive) return;

            // Check if we should spawn a boss
            var distanceTraveled = _player.position.z;
            if (distanceTraveled - _lastBossSpawnDistance >= spawnCheckpointDistance)
            {
                SpawnBoss();
                _lastBossSpawnDistance = distanceTraveled;
            }
        }

        /// <summary>
        /// Spawns the Cyber-Chihuahua King boss at a random spawn point.
        /// Riley: "Here comes the boss! Time to face the Cyber-Chihuahua King!"
        /// </summary>
        public void SpawnBoss()
        {
            if (_isBossActive || bossPrefab == null) return;

            // Choose spawn point
            var spawnPoint = GetRandomSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("Riley: No valid spawn point found for the boss!");
                return;
            }

            // Spawn boss
            var bossObject = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
            _currentBoss = bossObject.GetComponent<BossHound>();
            
            if (_currentBoss == null)
            {
                Debug.LogError("Riley: Boss prefab doesn't have a BossHound component!");
                Destroy(bossObject);
                return;
            }

            // Set up boss events
            _currentBoss.OnBossDefeated += HandleBossDefeated;
            _currentBoss.OnWeakPointDestroyed += HandleWeakPointDestroyed;
            _currentBoss.OnTreatTantrumStarted += HandleTreatTantrumStarted;
            _currentBoss.OnTreatTantrumEnded += HandleTreatTantrumEnded;

            _isBossActive = true;
            _bossesDefeated++;

            // Create dynamic arena
            StartCoroutine(CreateBossArena(spawnPoint.position));

            // Play spawn effects
            PlayBossSpawnEffects(spawnPoint.position);

            OnBossSpawned?.Invoke(_currentBoss);
            OnBossCheckpointReached?.Invoke(_bossesDefeated);

            Debug.Log($"Riley: The Cyber-Chihuahua King has appeared! This is boss fight #{_bossesDefeated}!");
            Debug.Log("Nibble: *bark* (Translation: Time to show that chihuahua who's boss!)");
        }

        /// <summary>
        /// Gets a random spawn point for the boss.
        /// Riley: "Need to find a good spot for the boss to appear!"
        /// </summary>
        private Transform GetRandomSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                // Create a default spawn point in front of the player
                var spawnPoint = new GameObject("BossSpawnPoint").transform;
                spawnPoint.position = _player.position + _player.forward * 30f;
                return spawnPoint;
            }

            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        /// <summary>
        /// Creates a dynamic arena around the boss spawn point.
        /// Riley: "Time to create an arena worthy of this boss fight!"
        /// </summary>
        private IEnumerator CreateBossArena(Vector3 centerPosition)
        {
            if (obstacleManager == null || arenaObstaclePrefabs == null || arenaObstaclePrefabs.Length == 0)
            {
                yield break;
            }

            Debug.Log("Nibble: *bark* (Translation: Creating a boss arena with obstacles!)");

            // Spawn arena obstacles in a circle around the boss
            for (int i = 0; i < arenaObstacleCount; i++)
            {
                var angle = (360f / arenaObstacleCount) * i * Mathf.Deg2Rad;
                var direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                var obstaclePosition = centerPosition + direction * arenaObstacleSpacing;

                // Choose random obstacle prefab
                var obstaclePrefab = arenaObstaclePrefabs[Random.Range(0, arenaObstaclePrefabs.Length)];
                
                // Spawn obstacle
                var obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
                
                // Add to obstacle manager if it has the right component
                var obstacleComponent = obstacle.GetComponent<ObstacleManager.ObstacleDefinition>();
                if (obstacleComponent != null)
                {
                    // In a real game, you'd add this to the obstacle manager's active obstacles
                    Debug.Log($"Riley: Added {obstacleComponent.id} to the boss arena!");
                }

                // Stagger obstacle creation
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("Riley: Boss arena created! The fight is about to begin!");
        }

        /// <summary>
        /// Plays boss spawn effects.
        /// Nibble: "Bark! (Translation: Time for some dramatic effects!)"
        /// </summary>
        private void PlayBossSpawnEffects(Vector3 position)
        {
            // Play spawn sound
            if (bossSpawnSound != null)
            {
                AudioSource.PlayClipAtPoint(bossSpawnSound, position);
            }

            // Play spawn particle effect
            if (bossSpawnEffect != null)
            {
                var effect = Instantiate(bossSpawnEffect, position, Quaternion.identity);
                Destroy(effect.gameObject, 5f);
            }

            // Screen shake or other effects
            StartCoroutine(ScreenShakeEffect());
        }

        /// <summary>
        /// Creates a screen shake effect when the boss spawns.
        /// Riley: "The ground is shaking! The boss is here!"
        /// </summary>
        private IEnumerator ScreenShakeEffect()
        {
            var originalPosition = Camera.main.transform.position;
            var shakeDuration = 2f;
            var shakeIntensity = 0.5f;

            for (float t = 0; t < shakeDuration; t += Time.deltaTime)
            {
                var shakeX = Random.Range(-shakeIntensity, shakeIntensity);
                var shakeY = Random.Range(-shakeIntensity, shakeIntensity);
                Camera.main.transform.position = originalPosition + new Vector3(shakeX, shakeY, 0);
                yield return null;
            }

            Camera.main.transform.position = originalPosition;
        }

        /// <summary>
        /// Handles boss defeat.
        /// Riley: "We did it! The Cyber-Chihuahua King is defeated!"
        /// </summary>
        private void HandleBossDefeated(BossHound boss)
        {
            _isBossActive = false;
            _currentBoss = null;

            // Play defeat effects
            if (bossDefeatSound != null)
            {
                AudioSource.PlayClipAtPoint(bossDefeatSound, boss.transform.position);
            }

            OnBossDefeated?.Invoke(boss);

            Debug.Log("Riley: The Cyber-Chihuahua King is defeated! We can continue our escape!");
            Debug.Log("Nibble: *happy bark* (Translation: Victory! We beat the tiny tyrant!)");

            // Clear arena obstacles after a delay
            StartCoroutine(ClearArenaAfterDelay());
        }

        /// <summary>
        /// Handles weak point destruction.
        /// Riley: "One weak point down! Keep hitting the others!"
        /// </summary>
        private void HandleWeakPointDestroyed(BossHound.WeakPoint weakPoint)
        {
            Debug.Log($"Riley: {weakPoint.name} destroyed! The mech-suit is taking damage!");
        }

        /// <summary>
        /// Handles treat tantrum start.
        /// Riley: "Oh no! The chihuahua is having a treat tantrum!"
        /// </summary>
        private void HandleTreatTantrumStarted()
        {
            Debug.Log("Riley: The Cyber-Chihuahua King is having a treat tantrum! This is going to be messy!");
            Debug.Log("Nibble: *worried bark* (Translation: The chihuahua is really angry now!)");
        }

        /// <summary>
        /// Handles treat tantrum end.
        /// Riley: "The tantrum is over, but the chihuahua is still dangerous!"
        /// </summary>
        private void HandleTreatTantrumEnded()
        {
            Debug.Log("Riley: The treat tantrum is over, but that chihuahua is still dangerous!");
        }

        /// <summary>
        /// Clears arena obstacles after boss defeat.
        /// Nibble: "Bark! (Translation: Clean up the arena after the fight!)"
        /// </summary>
        private IEnumerator ClearArenaAfterDelay()
        {
            yield return new WaitForSeconds(5f);

            // Find and destroy arena obstacles
            var arenaObstacles = FindObjectsOfType<GameObject>();
            foreach (var obstacle in arenaObstacles)
            {
                if (obstacle.name.Contains("ArenaObstacle"))
                {
                    Destroy(obstacle);
                }
            }

            Debug.Log("Riley: Arena cleared! Ready for the next challenge!");
        }

        /// <summary>
        /// Gets the current boss if one is active.
        /// Riley: "Is there a boss currently active?"
        /// </summary>
        public BossHound GetCurrentBoss()
        {
            return _currentBoss;
        }

        /// <summary>
        /// Checks if a boss is currently active.
        /// Nibble: "Bark! (Translation: Is there a boss fight happening?)"
        /// </summary>
        public bool IsBossActive => _isBossActive;

        /// <summary>
        /// Gets the number of bosses defeated.
        /// Riley: "How many bosses have we defeated?"
        /// </summary>
        public int BossesDefeated => _bossesDefeated;

        /// <summary>
        /// Forces a boss spawn (useful for testing).
        /// Riley: "Time to test the boss spawn system!"
        /// </summary>
        [ContextMenu("Force Spawn Boss")]
        public void ForceSpawnBoss()
        {
            if (!_isBossActive)
            {
                SpawnBoss();
            }
            else
            {
                Debug.Log("Riley: A boss is already active! Can't spawn another one!");
            }
        }
    }
}