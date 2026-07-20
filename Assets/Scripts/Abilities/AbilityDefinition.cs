using UnityEngine;

namespace TransformationFPS.Abilities
{
    public enum AbilitySlot
    {
        Melee,
        MorphBolt,   // Destiny "grenade" equivalent
        Adaptation,  // Destiny "class ability" equivalent (rift/barricade/dodge)
        Ultimate     // Destiny "super" equivalent - the full transformation
    }

    /// <summary>
    /// Data-driven definition for a single ability. Designers create these as
    /// ScriptableObject assets in the Editor (Create > TransformationFPS > Ability)
    /// and slot them into a TransformationForm.
    /// </summary>
    [CreateAssetMenu(menuName = "TransformationFPS/Ability", fileName = "NewAbility")]
    public class AbilityDefinition : ScriptableObject
    {
        public string abilityName = "New Ability";
        [TextArea] public string description;
        public AbilitySlot slot;

        [Header("Cost / Timing")]
        public float cooldownSeconds = 20f;
        [Tooltip("For Ultimate: energy required out of 100 to activate.")]
        public float energyCost = 100f;

        [Header("Effect")]
        public float damage;
        public float radius;
        public float durationSeconds;
    }
}
