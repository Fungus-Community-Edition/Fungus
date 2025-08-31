using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Reserve space for given number of items in the collection
    /// </summary>
    [CommandInfo("Collection",
                 "Reserve",
                     "Reserve space for given number of items in the collection")]
    [AddComponentMenu("")]
    public class CollectionCommandReserve : CollectionBaseIntCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.Reserve(integer.Value);
        }
    }
}
