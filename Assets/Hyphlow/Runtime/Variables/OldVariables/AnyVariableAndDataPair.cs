using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Pairing of an AnyVariableData and an variable reference. Internal lookup for
    /// matching the right kind of variable with the correct data in the AnyVariableData.
    /// </summary>
    [Serializable]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class AnyVariableAndDataPair : ISerializationCallbackReceiver
    {
        [SerializeField]
        [FormerlySerializedAs("varRef")]
        private VariableReference _varRef = new VariableReference();

        [SerializeField]
        [FormerlySerializedAs("data")]
        private AnyVariableData _data = new AnyVariableData(); // RHS

        public AnyVariableData Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        public virtual IVariable LhsVariable
        {
            get
            {
                // Always derive from the serialized reference to avoid stale cache
                _varRef?.Refresh();
                return _varRef?.Variable;
            }
            set
            {
                _varRef ??= new VariableReference();
                _varRef.Variable = value;
            }
        }

        public bool HasReference(Variable variable)
        {
            // Only legacy comparison makes sense for this signature
            return ReferenceEquals(variable, LhsVariable) || _data.HasReference(variable);
        }

#if UNITY_EDITOR
        public void RefreshVariableCacheHelper(Flowchart flowchart, ref IList<IVariable> referencedVariables)
        {
            var eff = LhsVariable;

            if (eff is IVariable<string> asStringVar &&
                asStringVar != null &&
                !string.IsNullOrEmpty(asStringVar.Value))
            {
                flowchart.DetermineSubstituteVariables(asStringVar.Value, referencedVariables);
            }

            string text = _data.BoxedValue as string;
            if (!string.IsNullOrEmpty(text))
            {
                flowchart.DetermineSubstituteVariables(text, referencedVariables);
            }
        }
#endif

        public string GetDataDescription()
        {
            if (_data == null)
            {
                return "Null";
            }

            string desc = _data.GetDescription();
            if (!string.IsNullOrEmpty(desc))
            {
                return desc;
            }

            object boxed = _data.BoxedValue;
            return boxed != null ? 
                boxed.ToString() : 
                "Null";
        }

        protected static bool TryGetTypeActionsFor(Type varType, out VariableTypeActions result)
        {
            return VariableTypeRegistry.TryGetTypeActionsFor(varType, out result);
        }

        // Important: consider legacy first, then managed, and unwrap pointers as needed
        protected virtual Type VarType
        {
            get
            {
                var eff = LhsVariable;
                if (eff == null)
                    return null;

                return eff.GetType();
            }
        }

        public bool Compare(CompareOperator compareOperator, ref bool compareResult)
        {
            var eff = LhsVariable;
            bool foundActions = TryGetTypeActionsFor(VarType, out var typeActions);

            if (foundActions)
            {
                compareResult = typeActions.CompareFunc(eff, _data, compareOperator);
            }

            return foundActions;
        }

        public void SetOp(SetOperator setOperator)
        {
            var eff = LhsVariable;
            bool foundActions = TryGetTypeActionsFor(VarType, out VariableTypeActions typeActions);
            if (foundActions)
            {
                typeActions.SetFunc(eff, _data, setOperator);
            }
        }

        public void OnBeforeSerialize()
        {

        }

        public void Refresh()
        {
            if (variable != null)
            {
                // Migrate legacy variable reference to the new VariableReference system
                Debug.Log($"Migrating legacy variable reference {variable} to new VariableReference system.");
                LhsVariable = variable;
                variable = null;
            }
        }

        public void OnAfterDeserialize()
        {
            Refresh();
        }

        [SerializeField] [FormerlySerializedAs("variable")] [HideInInspector] public Variable variable;
    }
}