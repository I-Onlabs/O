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
    }
}
