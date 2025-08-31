using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Remove all items from collection that are also in RHS and add all the items in RHS that are not already 
    /// in target. Similar to a xor
    /// </summary>
    [CommandInfo("Collection",
                 "Exclusive",
                     "Remove all items from collection that are also in RHS and add all the items in RHS that are not already in target. " +
        "Similar to a xor")]
    [AddComponentMenu("")]
    public class CollectionCommandExclusive : CollectionBaseTwoCollectionCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.Exclusive(rhsCollection.Value);
        }
    }
}
