using System;
using UnityEngine;
using TransformationFPS.Core;
using TransformationFPS.Player;

namespace TransformationFPS.Abilities
{
    /// <summary>
    /// Drives the Destiny-style ability loop for whatever TransformationForm is equipped:
    /// melee (F), morph bolt (G), adaptation (Q), ultimate transformation (X).
    /// Ultimate energy fills from dealing damage / kills, mirroring supers charging from combat.
    /// </summary>
    [RequireComponent(typeof(FirstPersonController))]
    public class TransformationManager : MonoBehaviour
    {
        [Header("Equipped Form")]
        public TransformationForm equippedForm;

        [Header("Input")]
        public KeyCode meleeKey = KeyCode.F;
        public KeyCode morphBoltKey = KeyCode.G;
        public KeyCode adaptationKey = KeyCode.Q;
        public KeyCode ultimateKey = KeyCode.X;

        [Header("Ultimate Energy")]
        [Range(0f, 100f)] public float ultimateEnergy = 0f;
        public float energyPerMeleeHit = 8f;
        public float energyPerMorphBoltHit = 12f;
        public float energyPerKill = 25f;

        public bool IsTransformed { get; private set; }
        public float MeleeCooldownRemaining => Mathf.Max(0f, _meleeReadyTime - Time.time);
        public float MorphBoltCooldownRemaining => Mathf.Max(0f, _morphBoltReadyTime - Time.time);
        public float AdaptationCooldownRemaining => Mathf.Max(0f, _adaptationReadyTime - Time.time);

        public event Action<AbilitySlot> OnAbilityUsed;
        public event Action OnUltimateReady;
        public event Action OnTransformStart;
        public event Action OnTransformEnd;

        private float _meleeReadyTime;
        private float _morphBoltReadyTime;
        private float _adaptationReadyTime;
        private float _transformEndTime;
        private bool _wasUltimateReady;

        private FirstPersonController _movement;
        private PlayerStats _stats;
        private Vector3 _baseScale;
        private Renderer[] _bodyRenderers;
        private MaterialPropertyBlock _propBlock;

        private const float BasePassiveDamageResist = 0f;

        private void Awake()
        {
            _movement = GetComponent<FirstPersonController>();
            _stats = GetComponent<PlayerStats>();
            _baseScale = transform.localScale;
            _bodyRenderers = GetComponentsInChildren<Renderer>();
            _propBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            ApplyPassiveModifiers();
        }

        private void Update()
        {
            if (equippedForm == null) return;

            if (IsTransformed && Time.time >= _transformEndTime)
            {
                EndTransformation();
            }

            HandleAbilityInput();
            CheckUltimateReady();
        }

        private void HandleAbilityInput()
        {
            if (Input.GetKeyDown(meleeKey) && MeleeCooldownRemaining <= 0f && equippedForm.meleeAbility != null)
            {
                UseMelee();
            }
            if (Input.GetKeyDown(morphBoltKey) && MorphBoltCooldownRemaining <= 0f && equippedForm.morphBoltAbility != null)
            {
                UseMorphBolt();
            }
            if (Input.GetKeyDown(adaptationKey) && AdaptationCooldownRemaining <= 0f && equippedForm.adaptationAbility != null)
            {
                UseAdaptation();
            }
            if (Input.GetKeyDown(ultimateKey) && ultimateEnergy >= (equippedForm.ultimateAbility != null ? equippedForm.ultimateAbility.energyCost : 100f))
            {
                UseUltimate();
            }
        }

        private void UseMelee()
        {
            var ability = equippedForm.meleeAbility;
            _meleeReadyTime = Time.time + ability.cooldownSeconds;

            if (Camera.main != null && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 2.5f))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(ability.damage * CurrentDamageMultiplier(), hit.point, hit.normal);
                    AddUltimateEnergy(energyPerMeleeHit);
                }
            }

            OnAbilityUsed?.Invoke(AbilitySlot.Melee);
        }

        private void UseMorphBolt()
        {
            var ability = equippedForm.morphBoltAbility;
            _morphBoltReadyTime = Time.time + ability.cooldownSeconds;

            if (Camera.main != null && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 60f))
            {
                var hits = Physics.OverlapSphere(hit.point, Mathf.Max(0.1f, ability.radius));
                foreach (var col in hits)
                {
                    if (col.TryGetComponent<IDamageable>(out var damageable))
                    {
                        damageable.TakeDamage(ability.damage * CurrentDamageMultiplier(), hit.point, hit.normal);
                        AddUltimateEnergy(energyPerMorphBoltHit);
                    }
                }
            }

            OnAbilityUsed?.Invoke(AbilitySlot.MorphBolt);
        }

        private void UseAdaptation()
        {
            var ability = equippedForm.adaptationAbility;
            _adaptationReadyTime = Time.time + ability.cooldownSeconds;
            // Adaptation is a utility slot (e.g. burrow / phase step / regen field);
            // concrete effects are intentionally left to per-form subclasses of this manager
            // or a future StatusEffect system - this MVP just tracks the cooldown and event.
            OnAbilityUsed?.Invoke(AbilitySlot.Adaptation);
        }

        private void UseUltimate()
        {
            var ability = equippedForm.ultimateAbility;
            ultimateEnergy = 0f;
            BeginTransformation(ability.durationSeconds > 0f ? ability.durationSeconds : 15f);
            OnAbilityUsed?.Invoke(AbilitySlot.Ultimate);
        }

        private void BeginTransformation(float duration)
        {
            IsTransformed = true;
            _transformEndTime = Time.time + duration;

            transform.localScale = _baseScale * equippedForm.ultimateBodyScale;
            _movement.jumpMultiplier = equippedForm.passiveJumpMultiplier;
            TintBody(equippedForm.ultimateTintColor);

            OnTransformStart?.Invoke();
        }

        private void EndTransformation()
        {
            IsTransformed = false;
            transform.localScale = _baseScale;
            TintBody(Color.white);
            ApplyPassiveModifiers();

            OnTransformEnd?.Invoke();
        }

        private void ApplyPassiveModifiers()
        {
            if (equippedForm == null || _movement == null) return;
            _movement.speedMultiplier = equippedForm.passiveSpeedMultiplier;
            _movement.jumpMultiplier = equippedForm.passiveJumpMultiplier;
        }

        private void TintBody(Color color)
        {
            foreach (var renderer in _bodyRenderers)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_Color", color);
                renderer.SetPropertyBlock(_propBlock);
            }
        }

        private float CurrentDamageMultiplier()
        {
            return IsTransformed ? equippedForm.ultimateDamageMultiplier : 1f;
        }

        public float DamageResistance01()
        {
            if (equippedForm == null) return BasePassiveDamageResist;
            return Mathf.Clamp01(equippedForm.passiveDamageResist01);
        }

        public void AddUltimateEnergy(float amount)
        {
            ultimateEnergy = Mathf.Clamp(ultimateEnergy + amount, 0f, 100f);
        }

        public void NotifyKill()
        {
            AddUltimateEnergy(energyPerKill);
        }

        private void CheckUltimateReady()
        {
            float requiredEnergy = equippedForm.ultimateAbility != null ? equippedForm.ultimateAbility.energyCost : 100f;
            bool ready = ultimateEnergy >= requiredEnergy;
            if (ready && !_wasUltimateReady)
            {
                OnUltimateReady?.Invoke();
            }
            _wasUltimateReady = ready;
        }
    }
}
