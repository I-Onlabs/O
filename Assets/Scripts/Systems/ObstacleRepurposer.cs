using UnityEngine;

namespace AngryDogs.Systems
{
    /// <summary>
    /// Converts destroyed obstacles into defensive traps in-place.
    /// Designers can hook into OnRepurposed to spawn VFX.
    /// </summary>
    public sealed class ObstacleRepurposer : MonoBehaviour
    {
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private ObjectPooler pooler;

        private void Awake()
        {
            if (pooler == null)
            {
                pooler = FindObjectOfType<ObjectPooler>();
            }
        }

        public void TryRepurpose(GameObject hitObject, Vector3 position, Vector3 normal)
        {
            if (hitObject == null || ((1 << hitObject.layer) & obstacleMask) == 0)
            {
                return;
            }

            hitObject.SetActive(false);

            if (trapPrefab == null || pooler == null)
            {
                return;
            }

            var trap = pooler.Get(trapPrefab, position, Quaternion.LookRotation(normal));
            trap.transform.localScale = Vector3.one * 1.25f;
        }
    }
}
