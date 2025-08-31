using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// How many occurrences of a given variable exist in a target collection
    /// </summary>
    [CommandInfo("Collection",
                 "Occurrences",
                     "How many occurrences of a given variable exist in a target collection")]
    [AddComponentMenu("")]
    public class CollectionCommandOccurrences : CollectionBaseVarAndIntCommand
    {
        protected override void OnEnterInner()
        {
            integer.Value = collection.Value.Occurrences(variableToUse);
        }
    }
}
