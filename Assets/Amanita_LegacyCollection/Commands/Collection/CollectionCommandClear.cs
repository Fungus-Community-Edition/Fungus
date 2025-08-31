using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Clears a target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Clear",
                     "Clears a target collection")]
    [AddComponentMenu("")]
    public class CollectionCommandClear : CollectionBaseCommand
    {
        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                collection.Value.Clear();
            }

            Continue();
        }
    }
}
