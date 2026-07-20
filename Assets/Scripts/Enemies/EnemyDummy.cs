using UnityEngine;
using TransformationFPS.Core;
using TransformationFPS.Abilities;

namespace TransformationFPS.Enemies
{
    /// <summary>
    /// Minimal combat target: absorbs damage, flashes on hit, respawns after a delay
    /// so a solo test session has something to keep shooting at. Notifies the player's
    /// TransformationManager on kill so Ultimate energy has a source in the test arena.
    /// </summary>
    public class EnemyDummy : MonoBehaviour, IDamageable
    {
        public float maxHealth = 60f;
        public float respawnDelay = 4f;

        public bool IsDead { get; private set; }

        private float _currentHealth;
        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;
        private Color _baseColor;
        private Vector3 _spawnPosition;

        private void Awake()
        {
            _currentHealth = maxHealth;
            _renderer = GetComponentInChildren<Renderer>();
            _propBlock = new MaterialPropertyBlock();
            _spawnPosition = transform.position;
            if (_renderer != null)
            {
                _baseColor = _renderer.sharedMaterial.color;
            }
        }

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (IsDead || amount <= 0f) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            FlashHit();

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        private void FlashHit()
        {
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_Color", Color.red);
            _renderer.SetPropertyBlock(_propBlock);
            CancelInvoke(nameof(ResetColor));
            Invoke(nameof(ResetColor), 0.1f);
        }

        private void ResetColor()
        {
            if (_renderer == null) return;
            _propBlock.SetColor("_Color", _baseColor);
            _renderer.SetPropertyBlock(_propBlock);
        }

        private void Die()
        {
            IsDead = true;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.TryGetComponent<TransformationManager>(out var manager))
            {
                manager.NotifyKill();
            }

            gameObject.SetActive(false);
            Invoke(nameof(Respawn), respawnDelay);
        }

        private void Respawn()
        {
            transform.position = _spawnPosition;
            _currentHealth = maxHealth;
            IsDead = false;
            ResetColor();
            gameObject.SetActive(true);
        }
    }
}
