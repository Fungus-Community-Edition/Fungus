using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Clears target and then adds all of rhs to target.
    /// </summary>
    [CommandInfo("Collection",
                 "Copy",
                     "Clears target and then adds all of rhs to target.")]
    [AddComponentMenu("")]
    public class CollectionCommandCopy : CollectionBaseTwoCollectionCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.CopyFrom(rhsCollection.Value);
        }
    }
}
