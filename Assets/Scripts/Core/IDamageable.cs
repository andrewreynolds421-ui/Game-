using UnityEngine;

namespace TransformationFPS.Core
{
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    }
}
