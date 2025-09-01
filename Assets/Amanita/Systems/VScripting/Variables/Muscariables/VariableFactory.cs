using System;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class VariableFactory
    {
        public static Muscariable Create(Type contentType, IVariable toMakeCopyOf = null)
        {
            Type muscariType = VariableTypeRegistry.MuscariTypeFor(contentType);
            Muscariable result = (Muscariable) Activator.CreateInstance(muscariType);

            SetFromSourceVar();
            void SetFromSourceVar()
            {
                if (toMakeCopyOf != null)
                {
                    result.Key = toMakeCopyOf.Key;
                    result.Scope = toMakeCopyOf.Scope;
                    result.ItemID = toMakeCopyOf.ItemID;
                    if (toMakeCopyOf.Value == null || result.ContentType.IsInstanceOfType(toMakeCopyOf.Value))
                        result.Value = toMakeCopyOf.Value;
                }
            }

            return result;
        }

        // The reason we require a holder for the legacy vars hers is because they're all
        // MonoBehaviours, meaning that they need to be attached to a GameObject. 
        // Or in our case, a Flowchart.
        public static T AddLegacyVarTo<T>(Flowchart varHolder) where T : Variable
        {
            return (T)AddLegacyVarTo(varHolder, typeof(T));
        }

        public static Variable AddLegacyVarTo(Flowchart varHolder, Type contentType)
        {
            Variable result = null;
            Type legacyVarType = VariableTypeRegistry.LegacyTypeFor(contentType);

            var newVariable = varHolder.gameObject.AddComponent(legacyVarType) as Variable;
            if (newVariable == null)
            {
                string logMessage = $"Failed to add legacy variable component of type " +
                    $"{legacyVarType.Name} to {varHolder.name}";
                Debug.LogError(logMessage);
            }
            else
            {
                result = newVariable;
                varHolder.AddVariable(newVariable);
            }

            return result;
        }

    }
}