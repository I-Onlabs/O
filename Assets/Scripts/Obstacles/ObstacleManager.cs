using System;
using System.Collections.Generic;
using UnityEngine;
using AngryDogs.Systems;

namespace AngryDogs.Obstacles
{
    /// <summary>
    /// Handles obstacle spawning, pooling, and goofy repurposing into traps for Riley and Nibble.
    /// </summary>
    public sealed class ObstacleManager : MonoBehaviour
    {
        [Serializable]
        public class ObstacleDefinition
        {
            public string id = "SlobberCannon";
            public GameObject obstaclePrefab;
            public GameObject repurposedPrefab;
            [Tooltip("Relative chance for this obstacle to spawn compared to others.")]
            public float weight = 1f;
            [Tooltip("Scale multiplier applied to the repurposed defence.")]
            public Vector3 repurposedScale = Vector3.one;
            [Tooltip("Optional SFX when the obstacle is repurposed.")]
            public AudioClip repurposedClip;
            [Tooltip("Optional spawn SFX because neon cannons deserve fanfare.")]
            public AudioClip spawnClip;
            [Tooltip("Distance from Riley after which this obstacle despawns.")]
            public float despawnDistance = 20f;
            
            // New obstacle-specific properties
            [Tooltip("Special behavior type for unique obstacles.")]
            public ObstacleType obstacleType = ObstacleType.Standard;
            [Tooltip("Duration for temporary effects (e.g., sticky pellets, decoy trails).")]
            public float effectDuration = 5f;
            [Tooltip("Effect radius for area-of-effect obstacles.")]
            public float effectRadius = 3f;
        }

        public enum ObstacleType
        {
            Standard,
            KibbleVendingBot,    // Spawns sticky pellets that make hounds slip
            HoloPawPrint,        // Creates decoy trails to misguide hounds
            SlimeSlide,          // Existing repurposing target
            CyberChihuahua,      // Boss obstacle
            NeonSlobberCannon    // New: Spawns goo traps that can be repurposed into defense
        }

        private struct ActiveObstacle
        {
            public ObstacleDefinition Definition;
            public GameObject Instance;
            public bool Repurposed;
        }

        [Header("Player Tracking")]
        [SerializeField] private Transform player;
        [SerializeField] private float forwardSpawnOffset = 30f;
        [SerializeField] private float laneWidth = 3f;
        [SerializeField] private int laneCount = 3;

        [Header("Spawning")]
        [SerializeField] private AnimationCurve spawnIntervalCurve = AnimationCurve.Linear(0f, 3f, 240f, 1f);
        [SerializeField, Tooltip("Hard limit for active obstacles to avoid GC spikes.")]
        private int maxActiveObstacles = 48;
        [SerializeField] private List<ObstacleDefinition> obstacleDefinitions = new();

        [Header("Systems")]
        [SerializeField] private ObjectPooler pooler;
        [SerializeField] private AudioSource sfxSource;

        public event Action<ObstacleDefinition, GameObject> ObstacleSpawned;
        public event Action<ObstacleDefinition, GameObject> ObstacleRepurposed;

        private readonly List<ActiveObstacle> _activeObstacles = new(64);
        private readonly Dictionary<GameObject, int> _lookup = new();
        private readonly WeightedObstaclePicker _picker = new();

        private float _elapsed;
        private float _spawnTimer;
        private System.Random _random;
        private bool _warnedAboutSaturation;

        public int ActiveCount => _activeObstacles.Count;

        public void SetRandomSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        public void ConfigureObstacles(IEnumerable<ObstacleDefinition> definitions)
        {
            obstacleDefinitions = definitions != null
                ? new List<ObstacleDefinition>(definitions)
                : new List<ObstacleDefinition>();
            _picker.Configure(obstacleDefinitions);
            _warnedAboutSaturation = false;
        }

        private void Awake()
        {
            _random = new System.Random(1337);
            _picker.Configure(obstacleDefinitions);

            if (pooler == null)
            {
                pooler = FindObjectOfType<ObjectPooler>();
            }
        }

        private void Update()
        {
            if (player == null || pooler == null || _picker.Count == 0)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            _spawnTimer -= Time.deltaTime;

            if (_spawnTimer <= 0f && _activeObstacles.Count < maxActiveObstacles)
            {
                SpawnObstacle();
                var interval = Mathf.Clamp(spawnIntervalCurve.Evaluate(_elapsed), 0.35f, 6f);
                _spawnTimer = interval;
            }
            else if (!_warnedAboutSaturation && _activeObstacles.Count >= maxActiveObstacles)
            {
                Debug.LogWarning("ObstacleManager: Active obstacle cap reached. Consider raising maxActiveObstacles or tuning spawn curve to avoid draw call spikes.");
                _warnedAboutSaturation = true;
            }

            CullObstacles();
        }

        public void ResetManager()
        {
            for (var i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                var entry = _activeObstacles[i];
                pooler.Return(entry.Instance);
            }

            _activeObstacles.Clear();
            _lookup.Clear();
            _elapsed = 0f;
            _spawnTimer = 0f;
            _warnedAboutSaturation = false;
        }

        public void NotifyObstacleDestroyed(GameObject obstacle, Vector3 hitPoint, Vector3 normal)
        {
            if (obstacle == null || !_lookup.TryGetValue(obstacle, out var index))
            {
                return;
            }

            var entry = _activeObstacles[index];
            if (entry.Repurposed)
            {
                return; // Already turned into a slime slide.
            }

            pooler.Return(entry.Instance);
            RemoveAt(index);

            if (entry.Definition.repurposedPrefab == null)
            {
                return;
            }

            if (normal == Vector3.zero)
            {
                normal = Vector3.up;
            }

            var trap = pooler.Get(entry.Definition.repurposedPrefab, hitPoint, Quaternion.LookRotation(normal, Vector3.up));
            if (trap == null)
            {
                return;
            }
            trap.transform.localScale = entry.Definition.repurposedScale;
            PlayClip(entry.Definition.repurposedClip);

            var repurposed = new ActiveObstacle
            {
                Definition = entry.Definition,
                Instance = trap,
                Repurposed = true
            };

            _lookup[trap] = _activeObstacles.Count;
            _activeObstacles.Add(repurposed);
            ObstacleRepurposed?.Invoke(entry.Definition, trap);
        }

        private void SpawnObstacle()
        {
            var sample = (float)_random.NextDouble();
            var definition = _picker.Pick(sample);
            if (definition == null)
            {
                return;
            }

            var laneIndex = _random.Next(Mathf.Max(1, laneCount));
            var centered = laneIndex - (laneCount - 1) * 0.5f;
            var offset = player.right * (centered * laneWidth);
            var position = player.position + player.forward * forwardSpawnOffset + offset;

            var obstacle = pooler.Get(definition.obstaclePrefab, position, Quaternion.identity);
            var entry = new ActiveObstacle
            {
                Definition = definition,
                Instance = obstacle,
                Repurposed = false
            };

            _lookup[obstacle] = _activeObstacles.Count;
            _activeObstacles.Add(entry);
            PlayClip(definition.spawnClip);
            ObstacleSpawned?.Invoke(definition, obstacle);
        }

        private void CullObstacles()
        {
            for (var i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                var entry = _activeObstacles[i];
                if (entry.Instance == null)
                {
                    RemoveAt(i);
                    continue;
                }

                var distance = player.position.z - entry.Instance.transform.position.z;
                var despawnThreshold = entry.Definition.despawnDistance;
                if (entry.Repurposed)
                {
                    despawnThreshold *= 1.5f; // Let traps linger slightly longer.
                }

                if (distance > despawnThreshold)
                {
                    pooler.Return(entry.Instance);
                    RemoveAt(i);
                }
            }
        }

        private void RemoveAt(int index)
        {
            var lastIndex = _activeObstacles.Count - 1;
            var removed = _activeObstacles[index];
            _lookup.Remove(removed.Instance);

            if (index != lastIndex)
            {
                var swap = _activeObstacles[lastIndex];
                _activeObstacles[index] = swap;
                _lookup[swap.Instance] = index;
            }

            _activeObstacles.RemoveAt(lastIndex);
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Handles special obstacle behaviors when they're destroyed or activated.
        /// Riley: "Time to turn these obstacles into something useful for once!"
        /// </summary>
        public void HandleObstacleInteraction(GameObject obstacle, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (!_lookup.TryGetValue(obstacle, out var index))
            {
                return;
            }

            var entry = _activeObstacles[index];
            var definition = entry.Definition;

            switch (definition.obstacleType)
            {
                case ObstacleType.KibbleVendingBot:
                    ActivateKibbleVendingBot(obstacle, hitPoint, definition);
                    break;
                case ObstacleType.HoloPawPrint:
                    ActivateHoloPawPrint(obstacle, hitPoint, definition);
                    break;
                case ObstacleType.NeonSlobberCannon:
                    ActivateNeonSlobberCannon(obstacle, hitPoint, definition);
                    break;
                case ObstacleType.Standard:
                default:
                    // Standard repurposing behavior
                    NotifyObstacleDestroyed(obstacle, hitPoint, hitNormal);
                    break;
            }
        }

        /// <summary>
        /// Activates Kibble Vending Bot - spawns sticky pellets that make hounds slip comically.
        /// Nibble: "Bark! (Translation: Sticky treats for the bad hounds!)"
        /// </summary>
        private void ActivateKibbleVendingBot(GameObject obstacle, Vector3 hitPoint, ObstacleDefinition definition)
        {
            Debug.Log("Riley: Kibble vending bot activated! Time to make these hounds slip and slide!");

            // Spawn sticky pellets in a radius
            var pelletCount = 8;
            var angleStep = 360f / pelletCount;

            for (int i = 0; i < pelletCount; i++)
            {
                var angle = i * angleStep * Mathf.Deg2Rad;
                var direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                var pelletPosition = hitPoint + direction * definition.effectRadius * 0.5f;

                // Create sticky pellet effect
                StartCoroutine(CreateStickyPelletEffect(pelletPosition, definition.effectDuration));
            }

            // Play humorous sound effect
            PlayClip(definition.repurposedClip);
            
            // Remove the original obstacle
            if (_lookup.TryGetValue(obstacle, out var index))
            {
                pooler.Return(obstacle);
                RemoveAt(index);
            }

            // Trigger event for UI feedback
            ObstacleRepurposed?.Invoke(definition, obstacle);
        }

        /// <summary>
        /// Activates Holo-Paw Print - creates decoy trails to misguide hounds away from Nibble.
        /// Riley: "Holographic paw prints? Now that's some next-level misdirection!"
        /// </summary>
        private void ActivateHoloPawPrint(GameObject obstacle, Vector3 hitPoint, ObstacleDefinition definition)
        {
            Debug.Log("Nibble: *confused bark* (Translation: Why are there more of me?)");

            // Create a trail of holographic paw prints leading away from Nibble
            var trailLength = 5;
            var trailDirection = (hitPoint - player.position).normalized;
            var pawPrintSpacing = 2f;

            for (int i = 1; i <= trailLength; i++)
            {
                var pawPrintPosition = hitPoint + trailDirection * (i * pawPrintSpacing);
                StartCoroutine(CreateHoloPawPrintEffect(pawPrintPosition, definition.effectDuration, i));
            }

            // Play holographic sound effect
            PlayClip(definition.repurposedClip);

            // Remove the original obstacle
            if (_lookup.TryGetValue(obstacle, out var index))
            {
                pooler.Return(obstacle);
                RemoveAt(index);
            }

            // Trigger event for UI feedback
            ObstacleRepurposed?.Invoke(definition, obstacle);
        }

        /// <summary>
        /// Creates a sticky pellet effect that makes hounds slip.
        /// Riley: "These pellets are like digital banana peels for cybernetic hounds!"
        /// </summary>
        private IEnumerator CreateStickyPelletEffect(Vector3 position, float duration)
        {
            // Create a simple sticky pellet visual effect
            var pellet = new GameObject("StickyPellet");
            pellet.transform.position = position;
            
            // Add a simple visual indicator (in a real game, this would be a particle effect)
            var renderer = pellet.AddComponent<MeshRenderer>();
            var collider = pellet.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.isTrigger = true;

            // Add a component to handle hound slipping
            var stickyPellet = pellet.AddComponent<StickyPelletComponent>();
            stickyPellet.Initialize(duration);

            yield return new WaitForSeconds(duration);

            // Clean up
            if (pellet != null)
            {
                Destroy(pellet);
            }
        }

        /// <summary>
        /// Creates a holographic paw print effect to misguide hounds.
        /// Nibble: "Bark! (Translation: Look, it's me but not me!)"
        /// </summary>
        private IEnumerator CreateHoloPawPrintEffect(Vector3 position, float duration, int index)
        {
            // Create a holographic paw print visual effect
            var pawPrint = new GameObject($"HoloPawPrint_{index}");
            pawPrint.transform.position = position;
            
            // Add a simple visual indicator (in a real game, this would be a holographic effect)
            var renderer = pawPrint.AddComponent<MeshRenderer>();
            var collider = pawPrint.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            // Add a component to handle hound misdirection
            var holoPawPrint = pawPrint.AddComponent<HoloPawPrintComponent>();
            holoPawPrint.Initialize(duration, index);

            // Fade out over time
            var startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                var alpha = 1f - ((Time.time - startTime) / duration);
                // In a real game, you'd fade the material alpha here
                yield return null;
            }

            // Clean up
            if (pawPrint != null)
            {
                Destroy(pawPrint);
            }
        }

        /// <summary>
        /// Activates Neon Slobber Cannon - spawns goo traps that can be repurposed into defense.
        /// Riley: "Neon slobber cannons? These hounds are getting creative with their attacks!"
        /// Nibble: "Bark! (Translation: Gooey mess incoming!)"
        /// </summary>
        private void ActivateNeonSlobberCannon(GameObject obstacle, Vector3 hitPoint, ObstacleDefinition definition)
        {
            Debug.Log("Riley: Neon slobber cannon activated! Time to deal with this gooey mess!");
            Debug.Log("Nibble: *bark* (Translation: Gooey mess incoming!)");

            // Spawn goo traps in a pattern around the cannon
            var gooTrapCount = 6;
            var angleStep = 360f / gooTrapCount;
            var gooRadius = definition.effectRadius;

            for (int i = 0; i < gooTrapCount; i++)
            {
                var angle = i * angleStep * Mathf.Deg2Rad;
                var direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                var gooTrapPosition = hitPoint + direction * gooRadius * 0.7f;

                // Create goo trap effect
                StartCoroutine(CreateGooTrapEffect(gooTrapPosition, definition.effectDuration, i));
            }

            // Play slobber cannon sound effect
            PlayClip(definition.repurposedClip);
            
            // Remove the original obstacle
            if (_lookup.TryGetValue(obstacle, out var index))
            {
                pooler.Return(obstacle);
                RemoveAt(index);
            }

            // Trigger event for UI feedback
            ObstacleRepurposed?.Invoke(definition, obstacle);
        }

        /// <summary>
        /// Creates a goo trap effect that can be repurposed into defense.
        /// Riley: "These goo traps are like digital slime for cybernetic hounds!"
        /// </summary>
        private IEnumerator CreateGooTrapEffect(Vector3 position, float duration, int index)
        {
            // Create a goo trap visual effect
            var gooTrap = new GameObject($"GooTrap_{index}");
            gooTrap.transform.position = position;
            
            // Add a simple visual indicator (in a real game, this would be a neon goo effect)
            var renderer = gooTrap.AddComponent<MeshRenderer>();
            var collider = gooTrap.AddComponent<SphereCollider>();
            collider.radius = 1f;
            collider.isTrigger = true;

            // Add a component to handle goo trap behavior
            var gooTrapComponent = gooTrap.AddComponent<GooTrapComponent>();
            gooTrapComponent.Initialize(duration, index);

            yield return new WaitForSeconds(duration);

            // Clean up
            if (gooTrap != null)
            {
                Destroy(gooTrap);
            }
        }

        /// <summary>
        /// Gets all active obstacles of a specific type for gameplay mechanics.
        /// Riley: "Need to know what obstacles are causing trouble out there!"
        /// </summary>
        public List<GameObject> GetActiveObstaclesOfType(ObstacleType type)
        {
            var result = new List<GameObject>();
            
            foreach (var entry in _activeObstacles)
            {
                if (entry.Definition.obstacleType == type && entry.Instance != null)
                {
                    result.Add(entry.Instance);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if any obstacles are affecting a specific position (for hound AI).
        /// Nibble: "Bark! (Translation: Are there any sticky spots around here?)"
        /// </summary>
        public bool IsPositionAffectedByObstacle(Vector3 position, ObstacleType type, float radius = 1f)
        {
            foreach (var entry in _activeObstacles)
            {
                if (entry.Definition.obstacleType == type && entry.Instance != null)
                {
                    var distance = Vector3.Distance(position, entry.Instance.transform.position);
                    if (distance <= radius)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
