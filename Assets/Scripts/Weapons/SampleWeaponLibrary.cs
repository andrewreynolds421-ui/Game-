using UnityEngine;

namespace TransformationFPS.Weapons
{
    /// <summary>
    /// Builds a starter set of seven Destiny-archetype weapons entirely in code via
    /// ScriptableObject.CreateInstance, matching the pattern SampleFormLibrary uses for
    /// TransformationForms - no hand-authored .asset files required. In a full content
    /// pipeline these would instead be assets created via the Editor's
    /// Create > TransformationFPS > Weapon menu (the [CreateAssetMenu] attribute is already there).
    /// </summary>
    public static class SampleWeaponLibrary
    {
        public static WeaponDefinition CreateAutoRifle()
        {
            var def = ScriptableObject.CreateInstance<WeaponDefinition>();
            def.weaponName = "Rapid-7 Auto Rifle";
            def.archetype = WeaponArchetype.AutoRifle;
            def.ammoType = AmmoType.Primary;
            def.fireMode = FireMode.FullAuto;
            def.description = "Full-auto primary. Moderate damage, high fire rate, unlimited reserve ammo.";
            def.damagePerHit = 16f;
            def.fireRate = 8.5f;
            def.magazineSize = 33;
            def.reloadTime = 1.9f;
            def.range = 90f;
            def.baseSpreadDegrees = 1.8f;
            def.adsSpreadMultiplier = 0.35f;
            def.adsFov = 60f;
            return def;
        }

        public static WeaponDefinition CreateHandCannon()
        {
            var def = ScriptableObject.CreateInstance<WeaponDefinition>();
            def.weaponName = "Last Word Hand Cannon";
            def.archetype = WeaponArchetype.HandCannon;
            def.ammoType = AmmoType.Primary;
            def.fireMode = FireMode.SemiAuto;
            def.description = "Semi-auto primary. Slow but hard-hitting, click-to-fire precision.";
            def.damagePerHit = 38f;
            def.fireRate = 3.3f;
            def.magazineSize = 8;
            def.reloadTime = 1.6f;
            def.range = 100f;
            def.baseSpreadDegrees = 0.6f;
            def.adsSpreadMultiplier = 0.2f;
            def.adsFov = 55f;
            return def;
        }

        public static WeaponDefinition CreatePulseRifle()
        {
            var def = ScriptableObject.CreateInstance<WeaponDefinition>();
            def.weaponName = "Trinary Pulse Rifle";
            def.archetype = WeaponArchetype.PulseRifle;
            def.ammoType = AmmoType.Primary;
            def.fireMode = FireMode.Burst;
            def.description = "Primary. Fires 3-round bursts; each burst deals heavy damage if it all connects.";
            def.damagePerHit = 14f;
            def.fireRate = 2.6f; // bursts per second
            def.burstCount = 3;
            def.burstRoundDelay = 0.06f;
            def.magazineSize = 24;
            def.reloadTime = 2.0f;
            def.range = 95f;
            def.baseSpreadDegrees = 1.0f;
            def.adsSpreadMultiplier = 0.3f;
            def.adsFov = 58f;
            return def;
        }

        public static WeaponDefinition CreateShotgun()
        {
            var def = ScriptableObject.CreateInstance<WeaponDefinition>();
            def.weaponName = "Close Call Shotgun";
            def.archetype = WeaponArchetype.Shotgun;
            def.ammoType = AmmoType.Special;
            def.fireMode = FireMode.SemiAuto;
            def.description = "Special. Devastating up close, fires a spread of pellets, useless at range.";
            def.damagePerHit = 22f; // per pellet
            def.pelletsPerShot = 6;
            def.fireRate = 1.1f;
            def.magazineSize = 5;
            def.reloadTime = 2.4f;
            def.range = 18f;
            def.baseSpreadDegrees = 7f;
            def.adsSpreadMultiplier = 0.6f;
            def.adsFov = 65f;
            return def;
        }

        public static WeaponDefinition CreateSniperRifle()
        {
            var def = ScriptableObject.CreateInstance<WeaponDefinition>();
            def.weaponName = "Farsight Sniper Rifle";
            def.archetype = WeaponArchetype.SniperRifle;
            def.ammoType = AmmoType.Special;
            def.fireMode = FireMode.SemiAuto;
            def.description = "Special. Very high single-shot damage, slow fire rate, tight zoom.";
            def.damagePerHit = 95f;
            def.fireRate = 0.9f;
            def.magazineSize = 4;
            def.reloadTime = 2.6f;
            def.range = 250f;
            def.baseSpreadDegrees = 0.15f;
            def.adsSpreadMultiplier = 0.05f;
            def.adsFov = 25f;
            def.adsSpeed = 6f;
            return def;
        }

        public static WeaponDefinition CreateRocketLauncher()
        {
            var def = ScriptableObject.CreateInstance<WeaponDefinition>();
            def.weaponName = "Last Stand Rocket Launcher";
            def.archetype = WeaponArchetype.RocketLauncher;
            def.ammoType = AmmoType.Heavy;
            def.fireMode = FireMode.SemiAuto;
            def.description = "Heavy. Slow projectile, large splash damage. Watch your distance - it can hurt you too.";
            def.damagePerHit = 0f; // damage comes entirely from splash on impact
            def.fireRate = 0.6f;
            def.magazineSize = 1;
            def.reloadTime = 3.2f;
            def.range = 400f;
            def.isProjectile = true;
            def.projectileSpeed = 40f;
            def.splashRadius = 6f;
            def.splashDamage = 110f;
            def.adsFov = 60f;
            return def;
        }

        public static WeaponDefinition CreateFusionRifle()
        {
            var def = ScriptableObject.CreateInstance<WeaponDefinition>();
            def.weaponName = "Overcharge Fusion Rifle";
            def.archetype = WeaponArchetype.FusionRifle;
            def.ammoType = AmmoType.Special;
            def.fireMode = FireMode.Charge;
            def.description = "Special. Hold to charge, release at full charge to fire a burst of bolts. Releasing early cancels the shot.";
            def.damagePerHit = 20f; // per bolt
            def.pelletsPerShot = 6; // bolts per charged shot
            def.chargeTime = 0.85f;
            def.fireRate = 0.9f; // cooldown between charges once fired
            def.magazineSize = 12;
            def.reloadTime = 2.2f;
            def.range = 35f;
            def.baseSpreadDegrees = 2.5f;
            def.adsSpreadMultiplier = 0.4f;
            def.adsFov = 60f;
            return def;
        }
    }
}
