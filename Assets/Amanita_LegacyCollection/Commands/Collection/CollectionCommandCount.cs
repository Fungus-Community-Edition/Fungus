using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Number of items in the collection
    /// </summary>
    [CommandInfo("Collection",
                 "Count",
                     "Number of items in the collection")]
    [CommandInfo("Collection",
                 "Length",
                     "Number of items in the collection")]
    [AddComponentMenu("")]
    public class CollectionCommandCount : CollectionBaseIntCommand
    {
        protected override void OnEnterInner()
        {
            integer.Value = collection.Value.Count;
        }
    }
}
