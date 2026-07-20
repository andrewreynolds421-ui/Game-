using System;
using UnityEngine;
using TransformationFPS.Core;
using TransformationFPS.Abilities;

namespace TransformationFPS.Player
{
    /// <summary>
    /// Destiny-style health model: a regenerating shield layer absorbs damage first,
    /// then a health pool that only heals from abilities/pickups, not passively.
    /// </summary>
    public class PlayerStats : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        public float maxHealth = 100f;
        public float maxShield = 100f;
        public float shieldRegenDelay = 3.5f;
        public float shieldRegenRate = 40f; // per second

        public float CurrentHealth { get; private set; }
        public float CurrentShield { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float, float> OnHealthChanged; // current, max
        public event Action<float, float> OnShieldChanged;  // current, max
        public event Action OnDied;

        private float _lastDamageTime = -999f;
        private TransformationManager _transformationManager;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            CurrentShield = maxShield;
            _transformationManager = GetComponent<TransformationManager>();
        }

        private void Update()
        {
            if (IsDead) return;

            if (CurrentShield < maxShield && Time.time - _lastDamageTime >= shieldRegenDelay)
            {
                CurrentShield = Mathf.Min(maxShield, CurrentShield + shieldRegenRate * Time.deltaTime);
                OnShieldChanged?.Invoke(CurrentShield, maxShield);
            }
        }

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (IsDead || amount <= 0f) return;

            if (_transformationManager != null)
            {
                amount *= 1f - _transformationManager.DamageResistance01();
            }

            _lastDamageTime = Time.time;

            if (CurrentShield > 0f)
            {
                float absorbed = Mathf.Min(CurrentShield, amount);
                CurrentShield -= absorbed;
                amount -= absorbed;
                OnShieldChanged?.Invoke(CurrentShield, maxShield);
            }

            if (amount > 0f)
            {
                CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
                OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            }

            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            IsDead = true;
            OnDied?.Invoke();
        }
    }
}
