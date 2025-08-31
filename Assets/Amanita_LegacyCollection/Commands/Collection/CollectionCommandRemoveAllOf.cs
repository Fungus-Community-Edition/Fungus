using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Remove all items in given rhs collection to target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Remove All Of",
                     "Remove all items in given rhs collection to target collection")]
    [AddComponentMenu("")]
    public class CollectionCommandRemoveAllOf : CollectionBaseTwoCollectionCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.RemoveAll(rhsCollection);
        }
    }
}
