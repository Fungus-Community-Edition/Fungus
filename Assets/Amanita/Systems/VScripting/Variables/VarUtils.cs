using System;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class VarUtils
    {
        /// <summary>
        /// Given how direct casts work with boxing, you'll want to use this when you want
        /// to "cast" an IVariable with a numeric value to some other numeric type.
        /// </summary>
        public static TVal GetValueAs<TVal>(this IVariable variable)
        {
            object val = variable.Value;
            if (val == null)
            {
                return default;
            }

            var targetType = typeof(TVal);
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // If already the right runtime type
            if (underlying.IsInstanceOfType(val))
            {
                return (TVal)val;
            }

            // Enums
            if (underlying.IsEnum)
            {
                if (val is string enumStr)
                {
                    return (TVal)Enum.Parse(underlying, enumStr);
                }
                return (TVal)Enum.ToObject(underlying, val);
            }

            // Use IConvertible / Convert.ChangeType for primitives
            if (val is IConvertible)
            {
                object changed = Convert.ChangeType(val, underlying);
                return (TVal)changed;
            }

            // Last resort - try direct cast (may throw)
            return (TVal)val;
        }

        /// <summary>
        /// If the arg is a legacy one, this will create and return a Muscariable version of it.
        /// If conversion fails, returns null.
        /// If the arg is already a Muscariable, it (unaltered) will be the return value. 
        /// </summary>
        public static Muscariable ToMuscariable(this IVariable var)
        {
            if (var is Muscariable muscari)
            {
                return muscari;
            }
            else
            {
                muscari = VariableFactory.Create(var.ContentType, var);
                bool conversionSuccess = muscari != null;
                if (!conversionSuccess)
                {
                    Debug.LogWarning($"Could not convert legacy {var.ContentType.Name} Variable {var.Key} " +
                        $"to a Muscariable.");
                }
            }

            return muscari;
        }
    }
}