namespace TransformationFPS.Weapons
{
    /// <summary>Runtime per-weapon state (current magazine ammo) for one equipped loadout slot.
    /// Plain C# class, not a MonoBehaviour/ScriptableObject - WeaponController owns an array of these.</summary>
    public class WeaponInstance
    {
        public readonly WeaponDefinition Definition;
        public int AmmoInMagazine;

        public WeaponInstance(WeaponDefinition definition)
        {
            Definition = definition;
            AmmoInMagazine = definition.magazineSize;
        }
    }
}
