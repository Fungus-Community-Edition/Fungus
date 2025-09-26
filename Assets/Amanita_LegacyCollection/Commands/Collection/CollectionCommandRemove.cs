using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Remove an item to a collection
    /// </summary>
    [CommandInfo("Collection",
                 "Remove",
                     "Remove an item to a collection")]
    [AddComponentMenu("")]
    public class CollectionCommandRemove : CollectionBaseVarCommand
    {
        [Tooltip("Should it remove ALL occurances of variable")]
        [SerializeField]
        protected BooleanData allOccurances = new BooleanData(false);

        protected override void OnEnterInner()
        {
            if (allOccurances.Value)
                collection.Value.RemoveAll(variableToUse);
            else
                collection.Value.Remove(variableToUse);
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(allOccurances.VarRef, variable) || base.HasReference(variable);
        }

        public override string GetSummary()
        {
            return base.GetSummary() + (allOccurances.Value ? " ALL" : "");
        }
    }
}
