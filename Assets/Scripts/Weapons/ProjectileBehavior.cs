using UnityEngine;
using TransformationFPS.Core;

namespace TransformationFPS.Weapons
{
    /// <summary>
    /// Simple straight-line projectile for the Rocket Launcher: advances each frame and, the
    /// moment a raycast along its path of travel hits anything, explodes - dealing splash damage
    /// (with linear falloff by distance) to every IDamageable within splashRadius, including the
    /// player themselves if they're caught in it. That self-damage is intentional, not an
    /// oversight: it's a real risk/reward of firing a rocket at point-blank range, same as in
    /// most shooters with splash weapons.
    /// </summary>
    public class ProjectileBehavior : MonoBehaviour
    {
        public float maxLifetime = 6f;

        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private float _splashRadius;
        private float _spawnTime;

        public void Launch(Vector3 direction, float speed, float damage, float splashRadius)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _splashRadius = splashRadius;
            _spawnTime = Time.time;
        }

        private void Update()
        {
            if (Time.time - _spawnTime > maxLifetime)
            {
                Destroy(gameObject);
                return;
            }

            float step = _speed * Time.deltaTime;
            if (Physics.Raycast(transform.position, _direction, out RaycastHit hit, step))
            {
                Explode(hit.point);
                return;
            }

            transform.position += _direction * step;
        }

        private void Explode(Vector3 point)
        {
            var hits = Physics.OverlapSphere(point, _splashRadius);
            foreach (var col in hits)
            {
                if (!col.TryGetComponent<IDamageable>(out var damageable)) continue;

                float distance = Vector3.Distance(point, col.transform.position);
                float falloff = Mathf.Clamp01(1f - distance / _splashRadius);
                if (falloff <= 0f) continue;

                damageable.TakeDamage(_damage * falloff, point, -_direction);
            }

            Destroy(gameObject);
        }
    }
}
