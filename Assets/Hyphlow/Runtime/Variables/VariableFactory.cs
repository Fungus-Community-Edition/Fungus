using System;
using System.Reflection;
using UnityEngine;


using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public static class VariableFactory
    {
        #region Muscariables
        public static Muscariable<T> Create<T>(IVariable toMakeCopyOf = null)
        {
            return (Muscariable<T>)CreateByContentType(typeof(T), toMakeCopyOf);
        }

        public static Muscariable CreateByVarTypeName(string typeName, IVariable toMakeCopyOf = null)
        {
            Type varType = VariableTypeRegistry.MuscariTypeByName(typeName);

            if (varType == null)
            {
                Debug.LogWarning($"Variable type name '{typeName}' is not a valid Muscariable type. Returning null.");
                return null;
            }
            return CreateByVarType(varType, toMakeCopyOf);
        }

        public static Muscariable CreateByVarType(Type varType, IVariable toMakeCopyOf = null)
        {
            VariableInfoAttribute varInfo = varType.GetCustomAttribute<VariableInfoAttribute>();
            if (varInfo == null)
            {
                Debug.LogWarning($"Type {varType.Name} is not a valid Muscariable type. Returning null.");
                return null;
            }
            Type contentType = varInfo.ContentType;
            return CreateByContentType(contentType, toMakeCopyOf);
        }

        public static Muscariable CreateByContentType(Type contentType, IVariable toMakeCopyOf = null)
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
                    result.ItemId = toMakeCopyOf.ItemId;

                    if (toMakeCopyOf.BoxedValue == null || toMakeCopyOf.ContentType.Equals(contentType))
                    {
                        // Convert legacy boxed numeric types (e.g. boxed double) into the target contentType
                        // so that Muscariable.CanHoldAsValue (which checks runtime type) accepts it.
                        object srcVal = toMakeCopyOf.BoxedValue;
                        if (srcVal != null)
                        {
                            srcVal = ConvertValueToType(srcVal, contentType);
                        }

                        result.BoxedValue = srcVal;
                    }
                }
            }

            return result;
        }

        // Helper: attempt to convert boxed value to the requested runtime type so assignment
        // to a Muscariable (which checks obj.GetType()) will succeed. Handles numeric conversions
        // (e.g. boxed double -> float), enums and nullable underlying types.
        private static object ConvertValueToType(object source, Type targetType)
        {
            if (source == null) return null;

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // If already the right runtime type, return as-is.
            if (underlying.IsInstanceOfType(source)) return source;

            try
            {
                // Enum handling
                if (underlying.IsEnum)
                {
                    if (source is string sourceStr)
                    {
                        return Enum.Parse(underlying, sourceStr);
                    }
                    return Enum.ToObject(underlying, source);
                }

                // Use IConvertible -> Convert.ChangeType for primitive-like conversions
                if (source is IConvertible)
                {
                    return Convert.ChangeType(source, underlying);
                }

                // Fallback: try direct cast (may throw)
                return Convert.ChangeType(source, underlying);
            }
            catch
            {
                // If conversion fails, return original value and let Muscariable.Value validation fail as before.
                return source;
            }
        }

        public static Muscariable<T> Create<T>(T startingValue)
        {
            Type contentType = typeof(T);
            Muscariable<T> result = CreateByContentType(contentType, null) as Muscariable<T>;
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
                catch (Exception)
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