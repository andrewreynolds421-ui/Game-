using UnityEngine;

namespace TransformationFPS.Abilities
{
    /// <summary>
    /// Builds three sample Transformation Forms entirely in code via ScriptableObject.CreateInstance,
    /// so the test arena has playable content without requiring hand-authored .asset files.
    /// In a real content pipeline, designers would instead create these as assets in the Editor
    /// (Create > TransformationFPS > Transformation Form) and this factory would go away.
    /// </summary>
    public static class SampleFormLibrary
    {
        public static TransformationForm CreateBeastForm()
        {
            var form = ScriptableObject.CreateInstance<TransformationForm>();
            form.formName = "Beastkin";
            form.affinity = FormAffinity.Beast;
            form.description = "Melee and mobility focused form. Ultimate: Apex Predator - a temporary full-body transformation into a powerful beast.";
            form.passiveSpeedMultiplier = 1.1f;
            form.passiveJumpMultiplier = 1.0f;
            form.passiveDamageResist01 = 0f;
            form.ultimateBodyScale = 1.5f;
            form.ultimateTintColor = new Color(0.65f, 0.35f, 0.15f);
            form.ultimateSpeedMultiplier = 1.4f;
            form.ultimateDamageMultiplier = 2.2f;

            form.aerialMoveType = AerialMoveType.MultiJump;
            form.extraAirJumps = 1;
            form.airJumpHeightMultiplier = 0.9f;

            form.meleeAbility = MakeAbility("Rending Claw", AbilitySlot.Melee, 3.5f, 0f, damage: 45f);
            form.morphBoltAbility = MakeAbility("Pounce Bolt", AbilitySlot.MorphBolt, 12f, 0f, damage: 60f, radius: 3f);
            form.adaptationAbility = MakeAbility("Quickstep", AbilitySlot.Adaptation, 8f, 0f);
            form.ultimateAbility = MakeAbility("Apex Predator", AbilitySlot.Ultimate, 0f, 100f, durationSeconds: 15f);

            return form;
        }

        public static TransformationForm CreateElementalForm()
        {
            var form = ScriptableObject.CreateInstance<TransformationForm>();
            form.formName = "Emberkin";
            form.affinity = FormAffinity.Elemental;
            form.description = "Area damage focused form. Ultimate: Wildfire Ascendance - transform into a blazing elemental.";
            form.passiveSpeedMultiplier = 0.95f;
            form.passiveJumpMultiplier = 1.0f;
            form.passiveDamageResist01 = 0.05f;
            form.ultimateBodyScale = 1.3f;
            form.ultimateTintColor = new Color(1f, 0.45f, 0.1f);
            form.ultimateSpeedMultiplier = 1.15f;
            form.ultimateDamageMultiplier = 1.8f;

            form.aerialMoveType = AerialMoveType.Glide;
            form.glideDuration = 1.8f;
            form.glideFallSpeed = 2.5f;
            form.glideForwardSpeed = 10f;

            form.meleeAbility = MakeAbility("Cinder Fist", AbilitySlot.Melee, 4f, 0f, damage: 35f);
            form.morphBoltAbility = MakeAbility("Magma Bolt", AbilitySlot.MorphBolt, 14f, 0f, damage: 70f, radius: 5f);
            form.adaptationAbility = MakeAbility("Ash Veil", AbilitySlot.Adaptation, 10f, 0f);
            form.ultimateAbility = MakeAbility("Wildfire Ascendance", AbilitySlot.Ultimate, 0f, 100f, durationSeconds: 12f);

            return form;
        }

        public static TransformationForm CreateSpectralForm()
        {
            var form = ScriptableObject.CreateInstance<TransformationForm>();
            form.formName = "Wraithkin";
            form.affinity = FormAffinity.Spectral;
            form.description = "Utility and survivability focused form. Ultimate: Ghostwalk - transform into a near-untouchable spectral state.";
            form.passiveSpeedMultiplier = 1.05f;
            form.passiveJumpMultiplier = 1.15f;
            form.passiveDamageResist01 = 0.1f;
            form.ultimateBodyScale = 1.1f;
            form.ultimateTintColor = new Color(0.55f, 0.75f, 0.95f, 0.8f);
            form.ultimateSpeedMultiplier = 1.5f;
            form.ultimateDamageMultiplier = 1.4f;

            form.aerialMoveType = AerialMoveType.Blink;
            form.blinkDistance = 14f;
            form.blinkCooldown = 2.5f;

            form.meleeAbility = MakeAbility("Phase Strike", AbilitySlot.Melee, 3f, 0f, damage: 30f);
            form.morphBoltAbility = MakeAbility("Wisp Bolt", AbilitySlot.MorphBolt, 10f, 0f, damage: 40f, radius: 4f);
            form.adaptationAbility = MakeAbility("Fade Step", AbilitySlot.Adaptation, 6f, 0f);
            form.ultimateAbility = MakeAbility("Ghostwalk", AbilitySlot.Ultimate, 0f, 100f, durationSeconds: 18f);

            return form;
        }

        private static AbilityDefinition MakeAbility(string name, AbilitySlot slot, float cooldown, float energyCost, float damage = 0f, float radius = 0f, float durationSeconds = 0f)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.abilityName = name;
            ability.slot = slot;
            ability.cooldownSeconds = cooldown;
            ability.energyCost = energyCost;
            ability.damage = damage;
            ability.radius = radius;
            ability.durationSeconds = durationSeconds;
            return ability;
        }
    }
}
