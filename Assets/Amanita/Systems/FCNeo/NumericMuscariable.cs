using System;
using UnityEngine;

namespace Amanita.VScripting
{
    public abstract class NumericMuscariable<T> : Muscariable<T>, IComparable<T>, IComparable<NumericMuscariable<T>>
        where T : IComparable<T>, IEquatable<T>
    {
        public override bool IsArithmeticSupported => true;
        public override bool IsComparisonSupported => true;

        public override void Apply(SetOperator setOperator, T toApply)
        {
            switch (setOperator)
            {
                case SetOperator.Assign:
                    Value = toApply; break;
                case SetOperator.Add:
                    Value = (dynamic)Value + toApply; break;
                case SetOperator.Subtract:
                    Value = (dynamic)Value - toApply; break;
                case SetOperator.Multiply:
                    Value = (dynamic)Value * toApply; break;
                case SetOperator.Divide:
                    Value = (dynamic)Value / toApply; break;
                case SetOperator.Negate:
                    Value = (dynamic)Value * -1; break;
                default:
                    Debug.LogError($"The {setOperator} set operator is not valid for {ContentType.Name} variable {Key}.");
                    break;
            }
        }

        public override bool Evaluate(CompareOperator op, T otherNumericValue)
        {
            bool result;

            var comparisonRes = this.Value.CompareTo(otherNumericValue);
            switch (op)
            {
                case CompareOperator.Equals:
                    result = comparisonRes == 0; break;
                case CompareOperator.NotEquals:
                    result = comparisonRes != 0; break;
                case CompareOperator.LessThan:
                    result = comparisonRes < 0; break;
                case CompareOperator.GreaterThan:
                    result = comparisonRes > 0; break;
                case CompareOperator.LessThanOrEquals:
                    result = comparisonRes <= 0; break;
                case CompareOperator.GreaterThanOrEquals:
                    result = comparisonRes >= 0; break;
                default:
                    string errorMessage = $"Muscariable<{typeof(T).Name}> {Key} not compatible with CompareOperator {op}";
                    throw new System.ArgumentException(errorMessage);
            }

            return result;
        }

        public virtual int CompareTo(T numericValue)
        {
            return Value.CompareTo(numericValue);
        }

        public virtual int CompareTo(NumericMuscariable<T> otherNumericVar)
        {
            return Value.CompareTo(otherNumericVar.Value);
        }

    }

    public class IntMuscariable : NumericMuscariable<int>
    {
        public static IntMuscariable operator +(IntMuscariable a, IntMuscariable b)
            => new IntMuscariable { Value = a.Value + b.Value };

        public static IntMuscariable operator -(IntMuscariable a, IntMuscariable b)
            => new IntMuscariable { Value = a.Value - b.Value };

        public static IntMuscariable operator *(IntMuscariable a, IntMuscariable b)
            => new IntMuscariable { Value = a.Value * b.Value };

        public static IntMuscariable operator /(IntMuscariable a, IntMuscariable b)
            => new IntMuscariable { Value = a.Value / b.Value };

        public static bool operator ==(IntMuscariable a, IntMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(IntMuscariable a, IntMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as IntMuscariable;
            return this == other;
        }

        public override int GetHashCode()
        {
            // Delegate to Vector2’s hash
            return Value.GetHashCode();
        }


    }

    public class FloatMuscariable : NumericMuscariable<float>
    {
        public static FloatMuscariable operator +(FloatMuscariable a, FloatMuscariable b)
            => new FloatMuscariable { Value = a.Value + b.Value };

        public static FloatMuscariable operator -(FloatMuscariable a, FloatMuscariable b)
            => new FloatMuscariable { Value = a.Value - b.Value };

        public static FloatMuscariable operator *(FloatMuscariable a, FloatMuscariable b)
            => new FloatMuscariable { Value = a.Value * b.Value };

        public static FloatMuscariable operator /(FloatMuscariable a, FloatMuscariable b)
            => new FloatMuscariable { Value = a.Value / b.Value };

        public static bool operator ==(FloatMuscariable a, FloatMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(FloatMuscariable a, FloatMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as FloatMuscariable;
            return this == other;
        }

        public override int GetHashCode()
        {
            // Delegate to Vector2’s hash
            return Value.GetHashCode();
        }


    }

    public class BoolMuscariable : NumericMuscariable<bool>
    {
        public static bool operator ==(BoolMuscariable a, BoolMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(BoolMuscariable a, BoolMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as BoolMuscariable;
            return this == other;
        }

        public override int GetHashCode()
        {
            // Delegate to Vector2’s hash
            return Value.GetHashCode();
        }

    }

    public class DoubleMuscariable : NumericMuscariable<double>
    {
        public static DoubleMuscariable operator +(DoubleMuscariable a, DoubleMuscariable b)
            => new DoubleMuscariable { Value = a.Value + b.Value };

        public static DoubleMuscariable operator -(DoubleMuscariable a, DoubleMuscariable b)
            => new DoubleMuscariable { Value = a.Value - b.Value };

        public static DoubleMuscariable operator *(DoubleMuscariable a, DoubleMuscariable b)
            => new DoubleMuscariable { Value = a.Value * b.Value };

        public static DoubleMuscariable operator /(DoubleMuscariable a, DoubleMuscariable b)
            => new DoubleMuscariable { Value = a.Value / b.Value };

        public static bool operator ==(DoubleMuscariable a, DoubleMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(DoubleMuscariable a, DoubleMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DoubleMuscariable;
            return this == other;
        }

        public override int GetHashCode()
        {
            // Delegate to Vector2’s hash
            return Value.GetHashCode();
        }

    }

}