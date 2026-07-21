using UnityEngine;

namespace TransformationFPS.Weapons
{
    public enum WeaponArchetype
    {
        AutoRifle,
        HandCannon,
        PulseRifle,
        Shotgun,
        SniperRifle,
        RocketLauncher,
        FusionRifle
    }

    public enum FireMode
    {
        FullAuto,
        SemiAuto,
        Burst,
        Charge
    }

    /// <summary>Destiny-style ammo economy: Primary is unlimited/always refills, Special and Heavy
    /// draw from small shared reserve pools tracked on WeaponController.</summary>
    public enum AmmoType
    {
        Primary,
        Special,
        Heavy
    }

    /// <summary>
    /// Data-driven definition for one weapon. Designers create these as ScriptableObject assets
    /// (Create > TransformationFPS > Weapon) in a real content pipeline; for now SampleWeaponLibrary
    /// builds a starter set in code, matching the pattern used for TransformationForm/AbilityDefinition.
    /// </summary>
    [CreateAssetMenu(menuName = "TransformationFPS/Weapon", fileName = "NewWeapon")]
    public class WeaponDefinition : ScriptableObject
    {
        public string weaponName = "New Weapon";
        public WeaponArchetype archetype;
        public AmmoType ammoType;
        public FireMode fireMode;
        [TextArea] public string description;

        [Header("Core Stats")]
        [Tooltip("Damage dealt by a single raycast/projectile hit - for multi-pellet or multi-bolt weapons, this is per-pellet/per-bolt.")]
        public float damagePerHit = 20f;
        [Tooltip("Shots per second for FullAuto; trigger-pulls per second (cooldown between bursts/charges) for SemiAuto/Burst/Charge.")]
        public float fireRate = 6f;
        public int magazineSize = 30;
        public float reloadTime = 1.8f;
        public float range = 120f;

        [Header("Accuracy")]
        public float baseSpreadDegrees = 1.5f;
        [Range(0f, 1f)] public float adsSpreadMultiplier = 0.3f;

        [Header("Multi-Projectile (Shotgun pellets / Fusion Rifle bolts)")]
        public int pelletsPerShot = 1;

        [Header("Burst (Pulse Rifle)")]
        public int burstCount = 1;
        public float burstRoundDelay = 0.06f;

        [Header("Charge (Fusion Rifle)")]
        [Tooltip("Must hold the trigger this long before it fires; releasing early cancels with no shot.")]
        public float chargeTime = 0.9f;

        [Header("Projectile (Rocket Launcher)")]
        public bool isProjectile;
        public float projectileSpeed = 45f;
        public float splashRadius = 5f;
        public float splashDamage = 90f;

        [Header("Aim Down Sights")]
        public float adsFov = 55f;
        public float adsSpeed = 10f;
    }
}
