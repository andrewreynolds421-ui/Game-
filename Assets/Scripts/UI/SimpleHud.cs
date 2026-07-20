using UnityEngine;
using TransformationFPS.Abilities;
using TransformationFPS.Player;

namespace TransformationFPS.UI
{
    /// <summary>
    /// Bare-bones OnGUI readout (health/shield/ammo/ability state) for quickly verifying the
    /// gameplay loop in-editor. Not a final UI - swap for a proper Canvas-based HUD later.
    /// </summary>
    public class SimpleHud : MonoBehaviour
    {
        private PlayerStats _stats;
        private WeaponController _weapon;
        private TransformationManager _transformation;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _weapon = GetComponent<WeaponController>();
            _transformation = GetComponent<TransformationManager>();
        }

        private void OnGUI()
        {
            const int pad = 16;
            int y = Screen.height - 140;

            GUI.Label(new Rect(pad, y, 400, 24), $"Health: {_stats.CurrentHealth:0} / {_stats.maxHealth:0}   Shield: {_stats.CurrentShield:0} / {_stats.maxShield:0}");
            y += 22;
            GUI.Label(new Rect(pad, y, 400, 24), $"Ammo: {_weapon.CurrentAmmo}/{_weapon.magazineSize}{(_weapon.IsReloading ? " (reloading)" : string.Empty)}");
            y += 22;

            if (_transformation.equippedForm != null)
            {
                GUI.Label(new Rect(pad, y, 500, 24), $"Form: {_transformation.equippedForm.formName}   Ultimate Energy: {_transformation.ultimateEnergy:0}/100{(_transformation.IsTransformed ? "   [TRANSFORMED]" : string.Empty)}");
                y += 22;
                GUI.Label(new Rect(pad, y, 600, 24),
                    $"Melee(F): {(_transformation.MeleeCooldownRemaining > 0f ? _transformation.MeleeCooldownRemaining.ToString("0.0") : "Ready")}   " +
                    $"MorphBolt(G): {(_transformation.MorphBoltCooldownRemaining > 0f ? _transformation.MorphBoltCooldownRemaining.ToString("0.0") : "Ready")}   " +
                    $"Adaptation(Q): {(_transformation.AdaptationCooldownRemaining > 0f ? _transformation.AdaptationCooldownRemaining.ToString("0.0") : "Ready")}   " +
                    $"Ultimate(X): {(_transformation.ultimateEnergy >= 100f ? "Ready" : "Charging")}");
            }
        }
    }
}
