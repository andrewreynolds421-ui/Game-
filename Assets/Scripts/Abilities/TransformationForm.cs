using UnityEngine;

namespace TransformationFPS.Abilities
{
    public enum FormAffinity
    {
        Beast,      // melee/mobility focused
        Elemental,  // area damage focused
        Spectral    // utility/survivability focused
    }

    /// <summary>The form's defining aerial move - its equivalent of a Destiny class's jump identity.</summary>
    public enum AerialMoveType
    {
        MultiJump,
        Glide,
        Blink
    }

    /// <summary>
    /// The Destiny-"subclass"-equivalent: a full kit of four abilities plus passive
    /// stat modifiers and the visual/scale change applied while the Ultimate transformation
    /// is active. Designers create these as assets (Create > TransformationFPS > Transformation Form)
    /// and assign one as the player's equipped form.
    /// </summary>
    [CreateAssetMenu(menuName = "TransformationFPS/Transformation Form", fileName = "NewForm")]
    public class TransformationForm : ScriptableObject
    {
        public string formName = "New Form";
        public FormAffinity affinity;
        [TextArea] public string description;

        [Header("Kit")]
        public AbilityDefinition meleeAbility;
        public AbilityDefinition morphBoltAbility;
        public AbilityDefinition adaptationAbility;
        public AbilityDefinition ultimateAbility;

        [Header("Passive Modifiers (always active while form is equipped)")]
        public float passiveSpeedMultiplier = 1f;
        public float passiveJumpMultiplier = 1f;
        public float passiveDamageResist01 = 0f; // 0-1, fraction of incoming damage reduced

        [Header("Aerial Mobility (this form's jump identity)")]
        public AerialMoveType aerialMoveType = AerialMoveType.MultiJump;
        [Tooltip("MultiJump: extra jumps available in the air, on top of the ground jump.")]
        public int extraAirJumps = 1;
        public float airJumpHeightMultiplier = 0.9f;
        [Tooltip("Glide: how long the slow-fall + forward drift lasts once triggered.")]
        public float glideDuration = 1.5f;
        [Tooltip("Glide: fall speed is clamped to this while gliding.")]
        public float glideFallSpeed = 3f;
        public float glideForwardSpeed = 9f;
        [Tooltip("Blink: instant forward teleport distance, clamped by obstacles.")]
        public float blinkDistance = 12f;
        public float blinkCooldown = 3f;

        [Header("Ultimate Transformation Visuals")]
        [Tooltip("Uniform scale applied to the player body while the Ultimate is active.")]
        public float ultimateBodyScale = 1.4f;
        public Color ultimateTintColor = Color.white;
        [Tooltip("Multiplier stacked on top of passive modifiers while the Ultimate is active.")]
        public float ultimateSpeedMultiplier = 1.3f;
        public float ultimateDamageMultiplier = 2f;
    }
}
