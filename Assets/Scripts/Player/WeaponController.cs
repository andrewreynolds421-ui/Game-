using UnityEngine;
using TransformationFPS.Core;
using TransformationFPS.Weapons;

namespace TransformationFPS.Player
{
    /// <summary>
    /// A three-slot Destiny-style weapon loadout: switch with 1/2/3 or the scroll wheel, each
    /// weapon fires according to its own WeaponDefinition (full-auto, semi-auto, burst, or
    /// charge, hitscan or projectile, single-shot or multi-pellet), and ammo is drawn from a
    /// shared reserve pool per AmmoType - Primary reserve is treated as unlimited, Special and
    /// Heavy are small pools that only deplete (no pickups yet).
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        public Camera weaponCamera;

        [Header("Loadout")]
        public WeaponDefinition[] loadout = new WeaponDefinition[3];

        [Header("Ammo Reserves (Primary is unlimited)")]
        public int specialReserve = 24;
        public int heavyReserve = 12;

        [Header("Aim Down Sights")]
        public float baseFov = 75f;

        public bool IsReloading { get; private set; }
        public int CurrentSlotIndex { get; private set; }
        public float ChargeProgress01 => _isCharging && CurrentWeapon != null
            ? Mathf.Clamp01((Time.time - _chargeStartTime) / Mathf.Max(0.01f, CurrentWeapon.Definition.chargeTime))
            : 0f;

        public WeaponInstance CurrentWeapon =>
            _instances != null && CurrentSlotIndex >= 0 && CurrentSlotIndex < _instances.Length
                ? _instances[CurrentSlotIndex]
                : null;

        private WeaponInstance[] _instances;
        private bool _isAiming;
        private float _nextFireTime;
        private float _reloadFinishTime;
        private int _burstRoundsRemaining;
        private float _nextBurstRoundTime;
        private bool _isCharging;
        private float _chargeStartTime;

        private void Awake()
        {
            if (weaponCamera == null) weaponCamera = Camera.main;
        }

        private void Start()
        {
            _instances = new WeaponInstance[loadout.Length];
            for (int i = 0; i < loadout.Length; i++)
            {
                if (loadout[i] != null)
                {
                    _instances[i] = new WeaponInstance(loadout[i]);
                }
            }
            CurrentSlotIndex = FindFirstEquippedSlot();
        }

        private int FindFirstEquippedSlot()
        {
            for (int i = 0; i < _instances.Length; i++)
            {
                if (_instances[i] != null) return i;
            }
            return -1;
        }

        private void Update()
        {
            HandleSwitchInput();

            var current = CurrentWeapon;
            if (current == null) return;

            HandleAds(current.Definition);
            HandleReload(current);

            if (IsReloading) return;

            switch (current.Definition.fireMode)
            {
                case FireMode.FullAuto:
                    HandleFullAuto(current);
                    break;
                case FireMode.SemiAuto:
                    HandleSemiAuto(current);
                    break;
                case FireMode.Burst:
                    HandleBurst(current);
                    break;
                case FireMode.Charge:
                    HandleCharge(current);
                    break;
            }

            if (Input.GetKeyDown(KeyCode.R) && current.AmmoInMagazine < current.Definition.magazineSize)
            {
                StartReload(current);
            }
        }

        private void HandleSwitchInput()
        {
            if (_instances == null) return;

            if (Input.GetKeyDown(KeyCode.Alpha1)) TrySwitch(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) TrySwitch(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) TrySwitch(2);
            else
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll > 0f) CycleSlot(1);
                else if (scroll < 0f) CycleSlot(-1);
            }
        }

        private void TrySwitch(int index)
        {
            if (index == CurrentSlotIndex) return;
            if (index < 0 || index >= _instances.Length || _instances[index] == null) return;

            CurrentSlotIndex = index;
            CancelTransientFireState();
        }

        private void CycleSlot(int direction)
        {
            if (_instances.Length == 0) return;
            int index = CurrentSlotIndex;
            for (int i = 0; i < _instances.Length; i++)
            {
                index = (index + direction + _instances.Length) % _instances.Length;
                if (_instances[index] != null)
                {
                    TrySwitch(index);
                    return;
                }
            }
        }

        private void CancelTransientFireState()
        {
            IsReloading = false;
            _burstRoundsRemaining = 0;
            _isCharging = false;
            _nextFireTime = 0f;
        }

        private void HandleFullAuto(WeaponInstance current)
        {
            if (Input.GetButton("Fire1") && Time.time >= _nextFireTime)
            {
                if (current.AmmoInMagazine <= 0) { StartReload(current); return; }
                FireShot(current);
                _nextFireTime = Time.time + 1f / current.Definition.fireRate;
            }
        }

        private void HandleSemiAuto(WeaponInstance current)
        {
            if (Input.GetButtonDown("Fire1") && Time.time >= _nextFireTime)
            {
                if (current.AmmoInMagazine <= 0) { StartReload(current); return; }
                FireShot(current);
                _nextFireTime = Time.time + 1f / current.Definition.fireRate;
            }
        }

        private void HandleBurst(WeaponInstance current)
        {
            if (_burstRoundsRemaining > 0)
            {
                if (Time.time >= _nextBurstRoundTime)
                {
                    if (current.AmmoInMagazine <= 0) { _burstRoundsRemaining = 0; StartReload(current); return; }
                    FireShot(current);
                    _burstRoundsRemaining--;
                    _nextBurstRoundTime = Time.time + current.Definition.burstRoundDelay;
                }
                return;
            }

            if (Input.GetButtonDown("Fire1") && Time.time >= _nextFireTime)
            {
                if (current.AmmoInMagazine <= 0) { StartReload(current); return; }
                FireShot(current);
                _burstRoundsRemaining = current.Definition.burstCount - 1;
                _nextBurstRoundTime = Time.time + current.Definition.burstRoundDelay;
                _nextFireTime = Time.time + 1f / current.Definition.fireRate;
            }
        }

        private void HandleCharge(WeaponInstance current)
        {
            if (!_isCharging && Input.GetButton("Fire1") && current.AmmoInMagazine > 0 && Time.time >= _nextFireTime)
            {
                _isCharging = true;
                _chargeStartTime = Time.time;
            }

            if (!_isCharging) return;

            bool fullyCharged = Time.time - _chargeStartTime >= current.Definition.chargeTime;
            if (fullyCharged)
            {
                _isCharging = false;
                FireShot(current);
                _nextFireTime = Time.time + 1f / current.Definition.fireRate;
            }
            else if (!Input.GetButton("Fire1"))
            {
                _isCharging = false; // released early - cancels with no shot fired
            }
        }

        private void FireShot(WeaponInstance current)
        {
            var def = current.Definition;
            current.AmmoInMagazine--;

            if (def.isProjectile)
            {
                FireProjectile(def);
                return;
            }

            float spreadDegrees = def.baseSpreadDegrees * (_isAiming ? def.adsSpreadMultiplier : 1f);
            int pellets = Mathf.Max(1, def.pelletsPerShot);
            for (int i = 0; i < pellets; i++)
            {
                FireHitscanPellet(def, spreadDegrees);
            }
        }

        private void FireHitscanPellet(WeaponDefinition def, float spreadDegrees)
        {
            if (weaponCamera == null) return;

            Vector3 direction = ApplySpread(weaponCamera.transform.forward, spreadDegrees);
            Ray ray = new Ray(weaponCamera.transform.position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, def.range))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(def.damagePerHit, hit.point, hit.normal);
                }
            }
        }

        private static Vector3 ApplySpread(Vector3 forward, float spreadDegrees)
        {
            if (spreadDegrees <= 0f) return forward;
            float yaw = Random.Range(-spreadDegrees, spreadDegrees);
            float pitch = Random.Range(-spreadDegrees, spreadDegrees);
            return Quaternion.Euler(pitch, yaw, 0f) * forward;
        }

        private void FireProjectile(WeaponDefinition def)
        {
            if (weaponCamera == null) return;

            var go = new GameObject($"{def.weaponName}_Projectile");
            go.transform.position = weaponCamera.transform.position + weaponCamera.transform.forward * 0.5f;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = Vector3.one * 0.3f;
            Object.Destroy(visual.GetComponent<Collider>());

            var projectile = go.AddComponent<ProjectileBehavior>();
            projectile.Launch(weaponCamera.transform.forward, def.projectileSpeed, def.splashDamage, def.splashRadius);
        }

        private void StartReload(WeaponInstance current)
        {
            if (IsReloading || current.AmmoInMagazine == current.Definition.magazineSize) return;
            if (current.Definition.ammoType != AmmoType.Primary && GetReserve(current.Definition.ammoType) <= 0) return;

            IsReloading = true;
            _burstRoundsRemaining = 0;
            _isCharging = false;
            _reloadFinishTime = Time.time + current.Definition.reloadTime;
        }

        private void HandleReload(WeaponInstance current)
        {
            if (IsReloading && Time.time >= _reloadFinishTime)
            {
                FinishReload(current);
            }
        }

        private void FinishReload(WeaponInstance current)
        {
            var def = current.Definition;

            if (def.ammoType == AmmoType.Primary)
            {
                current.AmmoInMagazine = def.magazineSize;
            }
            else
            {
                int needed = def.magazineSize - current.AmmoInMagazine;
                int available = GetReserve(def.ammoType);
                int drawn = Mathf.Min(needed, available);
                current.AmmoInMagazine += drawn;
                SetReserve(def.ammoType, available - drawn);
            }

            IsReloading = false;
        }

        public int GetReserve(AmmoType type)
        {
            return type switch
            {
                AmmoType.Special => specialReserve,
                AmmoType.Heavy => heavyReserve,
                _ => int.MaxValue
            };
        }

        private void SetReserve(AmmoType type, int value)
        {
            switch (type)
            {
                case AmmoType.Special: specialReserve = value; break;
                case AmmoType.Heavy: heavyReserve = value; break;
            }
        }

        private void HandleAds(WeaponDefinition def)
        {
            if (weaponCamera == null) return;
            _isAiming = Input.GetButton("Fire2");
            float targetFov = _isAiming ? def.adsFov : baseFov;
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, targetFov, Time.deltaTime * def.adsSpeed);
        }
    }
}
