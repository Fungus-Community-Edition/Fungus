using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Class for a single condition. A list of this is used for multiple conditions.
    /// </summary>
    [System.Serializable]
    [ExecuteInEditMode]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ConditionExpression
    {
        [SerializeField] protected CompareOperator compareOperator;
        [SerializeField] protected AnyVariableAndDataPair anyVar;

        public virtual AnyVariableAndDataPair AnyVar { get { return anyVar; } }
        public virtual CompareOperator CompareOperator { get { return compareOperator; } }

        public ConditionExpression()
        {
        }

        public ConditionExpression(CompareOperator op, AnyVariableAndDataPair variablePair)
        {
            compareOperator = op;
            anyVar = variablePair;
        }
    }

}