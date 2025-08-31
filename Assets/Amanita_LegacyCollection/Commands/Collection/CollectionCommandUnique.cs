using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Removes all duplicates.
    /// </summary>
    [CommandInfo("Collection",
                 "Unique",
                     "Removes all duplicates.")]
    [AddComponentMenu("")]
    public class CollectionCommandUnique : CollectionBaseCommand
    {
        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                collection.Value.Unique();
            }

            Continue();
        }
    }
}
