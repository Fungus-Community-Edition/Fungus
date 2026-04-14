using System;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public static class VarUtils
    {
        /// <summary>
        /// Given how direct casts work with boxing, you'll want to use this when you want
        /// to "cast" an IVariable with a numeric value to some other numeric type.
        /// </summary>
        public static TVal GetValueAs<TVal>(this IVariable variable)
        {
            object val = variable.BoxedValue;
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
        /// If the input var is a non-Muscariable, this will return a Muscariable version of it with the
        /// key, value, etc copied over. If false (and the input var is already a Muscariable) it
        /// will be returned directly.
        /// </summary>
        public static Muscariable ToMuscariable(this IVariable var, bool makeCopyIfAlreadyMuscari = false)
        {
            Muscariable result;
            if (makeCopyIfAlreadyMuscari || var is not Muscariable)
            {
                result = VariableFactory.CreateByContentType(var.ContentType, var);
            }
            else
            {
                result = (Muscariable)var;
            }

            return result;
        }
    }
}