using System.Collections.Generic;
using UnityEngine;

namespace AngryDogs.Systems
{
    /// <summary>
    /// Spawns obstacles ahead of the player using weighted lanes and difficulty curves.
    /// </summary>
    public sealed class ObstacleSpawner : MonoBehaviour
    {
        [System.Serializable]
        private class SpawnEntry
        {
            public GameObject prefab;
            [Range(0f, 1f)] public float weight = 1f;
        }

        [SerializeField] private Transform player;
        [SerializeField] private float spawnDistance = 40f;
        [SerializeField] private float despawnDistance = 10f;
        [SerializeField] private int laneCount = 3;
        [SerializeField] private float laneWidth = 3f;
        [SerializeField] private List<SpawnEntry> spawnTable = new();
        [SerializeField] private AnimationCurve spawnRateOverTime = AnimationCurve.Linear(0f, 1.5f, 600f, 0.5f);
        [SerializeField] private ObjectPooler pooler;

        private readonly List<GameObject> _activeObstacles = new();
        private float _lastSpawnZ;
        private float _timeAlive;

        private void Update()
        {
            if (player == null || spawnTable.Count == 0)
            {
                return;
            }

            _timeAlive += Time.deltaTime;
            var targetSpawnInterval = spawnRateOverTime.Evaluate(_timeAlive);

            if (player.position.z - _lastSpawnZ >= targetSpawnInterval)
            {
                SpawnObstacle();
            }

            DespawnBehindPlayer();
        }

        private void SpawnObstacle()
        {
            var prefab = PickWeightedPrefab();
            if (prefab == null)
            {
                return;
            }

            var laneIndex = Random.Range(0, Mathf.Max(1, laneCount));
            var centeredIndex = laneIndex - (laneCount - 1) * 0.5f;
            var spawnPos = player.position + player.forward * spawnDistance + player.right * (centeredIndex * laneWidth);
            var obstacle = pooler.Get(prefab, spawnPos, Quaternion.identity);
            _activeObstacles.Add(obstacle);
            _lastSpawnZ = player.position.z;
        }

        private void DespawnBehindPlayer()
        {
            for (var i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = _activeObstacles[i];
                if (player.position.z - obstacle.transform.position.z > despawnDistance)
                {
                    pooler.Return(obstacle);
                    _activeObstacles.RemoveAt(i);
                }
            }
        }

        private GameObject PickWeightedPrefab()
        {
            var totalWeight = 0f;
            foreach (var entry in spawnTable)
            {
                totalWeight += entry.weight;
            }

            var randomValue = Random.Range(0f, totalWeight);
            foreach (var entry in spawnTable)
            {
                if (randomValue <= entry.weight)
                {
                    return entry.prefab;
                }

                randomValue -= entry.weight;
            }

            return null;
        }
    }
}
