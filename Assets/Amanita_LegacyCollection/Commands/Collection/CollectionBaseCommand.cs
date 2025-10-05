using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Base class for all FungusCollection commands
    /// </summary>
    [AddComponentMenu("")]
    public abstract class CollectionBaseCommand : Command
    {
        [SerializeField]
        protected CollectionData collection;

        public override Color GetButtonColor()
        {
            return LegaCollCommandColors.Collection;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(variable, collection.VarRef);
        }

        public override string GetSummary()
        {
            if (collection.Value == null)
                return "Error: no collection selected";

            return collection.Value.name;
        }
    }
}
