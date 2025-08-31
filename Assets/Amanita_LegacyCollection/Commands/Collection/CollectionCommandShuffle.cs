using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Randomly reorders all elements of a target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Shuffle",
                     "Randomly reorders all elements of a target collection")]
    [AddComponentMenu("")]
    public class CollectionCommandShuffle : CollectionBaseCommand
    {
        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                collection.Value.Shuffle();
            }

            Continue();
        }
    }
}
