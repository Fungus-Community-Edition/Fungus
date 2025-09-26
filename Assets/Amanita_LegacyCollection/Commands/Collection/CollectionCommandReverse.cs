using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Reverse the current order of a target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Reverse",
                     "Reverse the current order of a target collection")]
    [AddComponentMenu("")]
    public class CollectionCommandReverse : CollectionBaseCommand
    {
        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                collection.Value.Reverse();
            }

            Continue();
        }
    }
}
