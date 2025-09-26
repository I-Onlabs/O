using System.Collections;
using UnityEngine;
using AngryDogs.Systems;

namespace AngryDogs.Player
{
    /// <summary>
    /// Controls projectile spawning and obstacle repurposing logic.
    /// </summary>
    public sealed class PlayerShooter : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField] private float fireRate = 8f;
        [SerializeField] private float boostedFireRate = 12f;
        [SerializeField] private float boostedDuration = 3f;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform muzzle;

        [Header("Systems")]
        [SerializeField] private ObjectPooler pooler;
        [SerializeField] private ObstacleRepurposer repurposer;

        private Coroutine _fireRoutine;
        private float _currentFireRate;

        private void Awake()
        {
            _currentFireRate = fireRate;
            if (pooler == null)
            {
                pooler = FindObjectOfType<ObjectPooler>();
            }
        }

        public void ProcessAim(Vector2 aim)
        {
            // Convert normalized viewport direction to world aim direction.
            var worldDir = new Vector3(aim.x, 0f, 1f).normalized;
            transform.forward = Vector3.Lerp(transform.forward, worldDir, 0.2f);
        }

        public void BeginFire()
        {
            if (_fireRoutine != null)
            {
                return;
            }

            _fireRoutine = StartCoroutine(FireContinuously());
        }

        public void EndFire()
        {
            if (_fireRoutine != null)
            {
                StopCoroutine(_fireRoutine);
                _fireRoutine = null;
            }
        }

        public void BoostFireRateTemporary()
        {
            StopCoroutine(nameof(BoostRoutine));
            StartCoroutine(nameof(BoostRoutine));
        }

        private IEnumerator FireContinuously()
        {
            while (true)
            {
                FireProjectile();
                yield return new WaitForSeconds(1f / Mathf.Max(0.01f, _currentFireRate));
            }
        }

        private void FireProjectile()
        {
            if (projectilePrefab == null || muzzle == null)
            {
                Debug.LogWarning("PlayerShooter missing projectile or muzzle reference.");
                return;
            }

            var projectile = pooler.Get(projectilePrefab.gameObject, muzzle.position, muzzle.rotation);
            if (projectile != null && projectile.TryGetComponent(out Projectile projectileComponent))
            {
                projectileComponent.Launch(OnProjectileHit);
            }
        }

        private void OnProjectileHit(RaycastHit hit)
        {
            if (repurposer == null)
            {
                return;
            }

            repurposer.TryRepurpose(hit.collider.gameObject, hit.point, hit.normal);
        }

        private IEnumerator BoostRoutine()
        {
            _currentFireRate = boostedFireRate;
            yield return new WaitForSeconds(boostedDuration);
            _currentFireRate = fireRate;
        }
    }
}
