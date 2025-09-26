using System.Collections.Generic;
using UnityEngine;

namespace AngryDogs.Systems
{
    /// <summary>
    /// Generic object pooler for reuse of projectiles, obstacles, hounds, etc.
    /// Avoids runtime allocations on mobile.
    /// </summary>
    public sealed class ObjectPooler : MonoBehaviour
    {
        [System.Serializable]
        private class Pool
        {
            public GameObject prefab;
            public int preload = 8;
        }

        [SerializeField] private List<Pool> pools = new();

        private readonly Dictionary<GameObject, Queue<GameObject>> _available = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

        private void Awake()
        {
            foreach (var pool in pools)
            {
                WarmPool(pool);
            }
        }

        private void WarmPool(Pool pool)
        {
            if (pool.prefab == null)
            {
                return;
            }

            if (!_available.ContainsKey(pool.prefab))
            {
                _available[pool.prefab] = new Queue<GameObject>();
            }

            for (var i = 0; i < pool.preload; i++)
            {
                var instance = CreateInstance(pool.prefab);
                Return(pool.prefab, instance);
            }
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogWarning("ObjectPooler.Get called with null prefab");
                return null;
            }

            if (!_available.TryGetValue(prefab, out var queue) || queue.Count == 0)
            {
                var instance = CreateInstance(prefab);
                return Activate(instance, position, rotation);
            }

            var pooled = queue.Dequeue();
            return Activate(pooled, position, rotation);
        }

        public void Return(GameObject prefab, GameObject instance)
        {
            if (prefab == null || instance == null)
            {
                return;
            }

            if (!_available.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _available[prefab] = queue;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
            queue.Enqueue(instance);
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            var instance = Instantiate(prefab, transform);
            _instanceToPrefab[instance] = prefab;
            return instance;
        }

        private GameObject Activate(GameObject instance, Vector3 position, Quaternion rotation)
        {
            instance.transform.SetParent(null);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }

        public void Return(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (_instanceToPrefab.TryGetValue(instance, out var prefab))
            {
                Return(prefab, instance);
            }
            else
            {
                Destroy(instance);
            }
        }
    }
}
