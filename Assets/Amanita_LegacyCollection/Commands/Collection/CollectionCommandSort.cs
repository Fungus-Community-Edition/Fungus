using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Sort a target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Sort",
                     "Sort a target collection")]
    [AddComponentMenu("")]
    public class CollectionCommandSort : CollectionBaseCommand
    {
        public override void OnEnter()
        {
            if (collection.Value != null)
            {
                collection.Value.Sort();
            }

            Continue();
        }
    }
}
