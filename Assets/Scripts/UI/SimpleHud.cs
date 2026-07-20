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
        private VehicleMountController _vehicleMount;
        private FirstPersonController _movement;
        private AerialMobilityController _aerialMobility;
        private LedgeMantleController _mantle;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _weapon = GetComponent<WeaponController>();
            _transformation = GetComponent<TransformationManager>();
            _vehicleMount = GetComponent<VehicleMountController>();
            _movement = GetComponent<FirstPersonController>();
            _aerialMobility = GetComponent<AerialMobilityController>();
            _mantle = GetComponent<LedgeMantleController>();
        }

        private void OnGUI()
        {
            const int pad = 16;
            int y = Screen.height - 160;

            if (_vehicleMount != null && _vehicleMount.IsMounted)
            {
                GUI.Label(new Rect(pad, y, 400, 24), $"Riding (V to dismount)   Speed: {_vehicleMount.ActiveVehicle.CurrentSpeed:0} m/s");
                y += 22;
            }

            GUI.Label(new Rect(pad, y, 400, 24), $"Health: {_stats.CurrentHealth:0} / {_stats.maxHealth:0}   Shield: {_stats.CurrentShield:0} / {_stats.maxShield:0}");
            y += 22;

            if (_vehicleMount == null || !_vehicleMount.IsMounted)
            {
                GUI.Label(new Rect(pad, y, 400, 24), $"Ammo: {_weapon.CurrentAmmo}/{_weapon.magazineSize}{(_weapon.IsReloading ? " (reloading)" : string.Empty)}");
                y += 22;
            }

            if (_transformation.equippedForm != null)
            {
                var form = _transformation.equippedForm;
                GUI.Label(new Rect(pad, y, 500, 24), $"Form: {form.formName}   Ultimate Energy: {_transformation.ultimateEnergy:0}/100{(_transformation.IsTransformed ? "   [TRANSFORMED]" : string.Empty)}");
                y += 22;
                GUI.Label(new Rect(pad, y, 600, 24),
                    $"Melee(F): {(_transformation.MeleeCooldownRemaining > 0f ? _transformation.MeleeCooldownRemaining.ToString("0.0") : "Ready")}   " +
                    $"MorphBolt(G): {(_transformation.MorphBoltCooldownRemaining > 0f ? _transformation.MorphBoltCooldownRemaining.ToString("0.0") : "Ready")}   " +
                    $"Adaptation(Q): {(_transformation.AdaptationCooldownRemaining > 0f ? _transformation.AdaptationCooldownRemaining.ToString("0.0") : "Ready")}   " +
                    $"Ultimate(X): {(_transformation.ultimateEnergy >= 100f ? "Ready" : "Charging")}");
                y += 22;

                if (_aerialMobility != null)
                {
                    string aerialStatus = form.aerialMoveType switch
                    {
                        AerialMoveType.MultiJump => $"Air Jumps: {_aerialMobility.AirJumpsRemaining}/{form.extraAirJumps}",
                        AerialMoveType.Glide => _aerialMobility.IsGliding ? "Gliding" : "Glide: Ready (Jump while airborne)",
                        AerialMoveType.Blink => _aerialMobility.BlinkCooldownRemaining > 0f ? $"Blink: {_aerialMobility.BlinkCooldownRemaining:0.0}s" : "Blink: Ready (Jump while airborne)",
                        _ => string.Empty
                    };
                    GUI.Label(new Rect(pad, y, 500, 24), $"Aerial Move ({form.aerialMoveType}): {aerialStatus}");
                    y += 22;
                }
            }

            if (_movement != null && _movement.IsSliding)
            {
                GUI.Label(new Rect(pad, y, 400, 24), "Sliding");
                y += 22;
            }
            if (_mantle != null && _mantle.IsMantling)
            {
                GUI.Label(new Rect(pad, y, 400, 24), "Mantling");
                y += 22;
            }

            GUI.Label(new Rect(pad, y, 400, 24), "V: Summon/Mount Hover Vehicle   Sprint+C: Slide");
        }
    }
}
