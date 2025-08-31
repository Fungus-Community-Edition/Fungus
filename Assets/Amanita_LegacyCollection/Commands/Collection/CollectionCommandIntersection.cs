using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Remove all items from collection that aren't also in RHS, similar to an overlap.
    /// </summary>
    [CommandInfo("Collection",
                 "Intersection",
                     "Remove all items from collection that aren't also in RHS, similar to an overlap.")]
    [AddComponentMenu("")]
    public class CollectionCommandIntersection : CollectionBaseTwoCollectionCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.Intersection(rhsCollection.Value);
        }
    }
}
