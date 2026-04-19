using UnityEngine;

using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

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
        [SerializeField]
        [FormerlySerializedAs("compareOperator")]
        protected CompareOperator _compareOperator;
        
        [SerializeField]
        [FormerlySerializedAs("anyVar")]
        protected AnyVariableAndDataPair _anyVar;

        public virtual AnyVariableAndDataPair AnyVar { get { return _anyVar; } }
        public virtual CompareOperator CompareOperator { get { return _compareOperator; } }

        public ConditionExpression()
        {
        }

        public ConditionExpression(CompareOperator op, AnyVariableAndDataPair variablePair)
        {
            _compareOperator = op;
            _anyVar = variablePair;
        }
    }

}