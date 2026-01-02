using UnityEngine;
using ArcaneBrigade.Core;

namespace ArcaneBrigade.Entities
{
    /// <summary>
    /// Base for everything that can fight on the field — Champion, Ally, Enemy.
    /// Concrete subclasses hook in their controller / AI; this class just provides
    /// shared bookkeeping (allegiance, position helpers, alive flag).
    /// </summary>
    public abstract class Unit : MonoBehaviour
    {
        [SerializeField] private Allegiance allegiance;

        public Allegiance Allegiance => allegiance;
        public bool IsAlive { get; protected set; } = true;
        public Vector2 Position2D => transform.position;
    }
}
