using System;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class VariableFactory
    {
        #region Muscariables
        public static Muscariable<T> Create<T>(IVariable toMakeCopyOf = null)
        {
            return Create(typeof(T), toMakeCopyOf) as Muscariable<T>;
        }

        public static Muscariable Create(Type contentType, IVariable toMakeCopyOf = null)
        {
            Muscariable result = null;
            Type muscariType = VariableTypeRegistry.MuscariTypeFor(contentType);

            if (toMakeCopyOf != null && !toMakeCopyOf.ContentType.Equals(contentType))
            {
                Type wrongContentType = toMakeCopyOf.ContentType;
                string logMessage = $"Cannot copy over the values of a variable of ContentType " +
                    $"{wrongContentType.Name} when creating a Muscariable of ContentType {contentType.Name}. "
                    + "Returning null.";
                Debug.LogWarning(logMessage);
            }
            else
            {
                result = (Muscariable)Activator.CreateInstance(muscariType);

                SetFromSourceVar();
                void SetFromSourceVar()
                {
                    if (toMakeCopyOf == null)
                    {
                        return;
                    }

                    result.Key = toMakeCopyOf.Key;
                    result.Scope = toMakeCopyOf.Scope;
                    result.ItemID = toMakeCopyOf.ItemID;
                    if (toMakeCopyOf.Value == null || toMakeCopyOf.ContentType.Equals(contentType))
                    {
                        result.Value = toMakeCopyOf.Value;
                    }
                }
            }

            return result;
        }

        public static Muscariable<T> Create<T>(T startingValue)
        {
            Muscariable<T> result = Create(typeof(T), null) as Muscariable<T>;
            result.Value = startingValue;
            return result;
        }

        #endregion


        #region Legacy Variables
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

            string errorMessage = $"Failed to add legacy variable component of type " +
                    $"{legacyVarType.Name} to Flowchart {varHolder.name}. Returning null.";
            var newVariable = varHolder.gameObject.AddComponent(legacyVarType) as Variable;
            if (newVariable == null)
            {
                Debug.LogWarning(errorMessage);
            }
            else
            {
                result = newVariable;
                try
                {
                    varHolder.AddVariable(newVariable);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(errorMessage);
                    result = null;
                    return result;
                }
            }

            return result;
        }

        #endregion
    }
}