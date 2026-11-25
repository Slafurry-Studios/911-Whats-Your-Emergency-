using UnityEngine;

namespace Slafurry.Core.Interface
{
    /// <summary>Implemented by anything a thrown item can hit (e.g. the kidnapper).</summary>
    public interface IThrowHittable
    {
        /// <summary>A thrown item hit this object.</summary>
        /// <param name="hitPoint">World position of the item when it hit.</param>
        /// <param name="direction">Normalized travel direction of the item.</param>
        void OnHitByThrow(Vector2 hitPoint, Vector2 direction);
    }
}
