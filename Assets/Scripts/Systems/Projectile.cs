using System;
using UnityEngine;

namespace AngryDogs.Systems
{
    /// <summary>
    /// Simple pooled projectile that raycasts forward and notifies callback on impact.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 40f;
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private LayerMask hitMask;
        [SerializeField] private ObjectPooler pooler;

        private Action<RaycastHit> _onHit;
        private Rigidbody _rigidbody;
        private float _lifetime;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (pooler == null)
            {
                pooler = FindObjectOfType<ObjectPooler>();
            }
        }

        public void Launch(Action<RaycastHit> onHit)
        {
            _onHit = onHit;
            _lifetime = 0f;
            gameObject.SetActive(true);
            _rigidbody.velocity = transform.forward * speed;
        }

        private void Update()
        {
            _lifetime += Time.deltaTime;
            if (_lifetime >= maxLifetime)
            {
                ReturnToPool();
                return;
            }

            if (Physics.Raycast(transform.position, transform.forward, out var hit, speed * Time.deltaTime, hitMask))
            {
                _onHit?.Invoke(hit);
                ReturnToPool();
            }
        }

        private void OnDisable()
        {
            _rigidbody.velocity = Vector3.zero;
            _onHit = null;
        }

        private void ReturnToPool()
        {
            if (pooler != null)
            {
                pooler.Return(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
