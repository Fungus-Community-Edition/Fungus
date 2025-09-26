using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Use the collection as a source of random selection. Picking a random item each run.
    /// </summary>
    [CommandInfo("Collection",
                 "RandomItem",
                     "Use the collection as a source of random selection. Picking a random item each run.")]
    [AddComponentMenu("")]
    public class CollectionRandom : CollectionBaseVarCommand
    {
        protected override void OnEnterInner()
        {
            collection.Value.Get(Random.Range(0, collection.Value.Count - 1), ref variableToUse);
        }
    }
}
