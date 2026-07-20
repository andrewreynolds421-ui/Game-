using UnityEngine;
using TransformationFPS.Core;

namespace TransformationFPS.Player
{
    /// <summary>
    /// Minimal hitscan primary weapon: left-click to fire, right-click to aim (FOV zoom).
    /// Stands in for a full weapon-archetype system (auto rifle / hand cannon / etc.)
    /// that would later be swapped in per Destiny-style loadout slots.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        public Camera weaponCamera;

        [Header("Stats")]
        public float damage = 18f;
        public float fireRate = 8f; // rounds per second
        public float range = 120f;
        public int magazineSize = 30;
        public float reloadTime = 1.6f;

        [Header("Aim Down Sights")]
        public float baseFov = 75f;
        public float adsFov = 55f;
        public float adsSpeed = 10f;

        public int CurrentAmmo { get; private set; }
        public bool IsReloading { get; private set; }

        private float _nextFireTime;
        private float _reloadFinishTime;

        private void Awake()
        {
            if (weaponCamera == null) weaponCamera = Camera.main;
            CurrentAmmo = magazineSize;
        }

        private void Update()
        {
            HandleReload();
            HandleAds();
            HandleFire();
        }

        private void HandleFire()
        {
            if (IsReloading) return;

            bool wantsFire = Input.GetButton("Fire1");
            if (wantsFire && Time.time >= _nextFireTime)
            {
                if (CurrentAmmo <= 0)
                {
                    StartReload();
                    return;
                }
                Fire();
                _nextFireTime = Time.time + 1f / fireRate;
            }

            if (Input.GetKeyDown(KeyCode.R) && CurrentAmmo < magazineSize)
            {
                StartReload();
            }
        }

        private void Fire()
        {
            CurrentAmmo--;

            if (weaponCamera == null) return;
            Ray ray = new Ray(weaponCamera.transform.position, weaponCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage, hit.point, hit.normal);
                }
            }
        }

        private void StartReload()
        {
            if (IsReloading || CurrentAmmo == magazineSize) return;
            IsReloading = true;
            _reloadFinishTime = Time.time + reloadTime;
        }

        private void HandleReload()
        {
            if (IsReloading && Time.time >= _reloadFinishTime)
            {
                CurrentAmmo = magazineSize;
                IsReloading = false;
            }
        }

        private void HandleAds()
        {
            if (weaponCamera == null) return;
            bool aiming = Input.GetButton("Fire2");
            float targetFov = aiming ? adsFov : baseFov;
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, targetFov, Time.deltaTime * adsSpeed);
        }
    }
}
