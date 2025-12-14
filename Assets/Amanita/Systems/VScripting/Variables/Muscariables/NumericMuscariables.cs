using System;
using UnityEngine;

namespace Amanita.VScripting
{
    [Serializable]
    public abstract class NumericMuscariable<T> : Muscariable<T>, IComparable<T>, IComparable<NumericMuscariable<T>>
        where T : IComparable<T>, IEquatable<T>
    {
        public override bool IsArithmeticSupported(SetOperator op) => true;
        public override bool IsRelationalSupported => true;
        public override bool IsComparisonSupported() => true;

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

        protected override object FilterForValueSet(object valueToConvert)
        {
            T result = (T)Convert.ChangeType(valueToConvert, this.ContentType);
            return result;
        }
        
    }

    [Serializable]
    [VariableInfo("Numeric", "Integer", typeof(int))]
    public class IntMuscariable : NumericMuscariable<int>, IVariable<int>
    {
        public IntMuscariable() : base() { }

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
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


    }

    [Serializable]
    [VariableInfo("Numeric", "Float", typeof(float))]
    public class FloatMuscariable : NumericMuscariable<float>
    {
        public FloatMuscariable(): base() { }

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
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


    }

    [Serializable]
    [VariableInfo("Numeric", "Boolean", typeof(bool))]
    public class BoolMuscariable : NumericMuscariable<bool>
    {
        public BoolMuscariable() : base() { }

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
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

    }

    [Serializable]
    [VariableInfo("Numeric", "Double", typeof(double))]
    public class DoubleMuscariable : NumericMuscariable<double>
    {
        public DoubleMuscariable() : base() { }

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
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

    }

    [Serializable]
    [VariableInfo("Numeric/Structured", "VectorTwo", typeof(Vector2))]
    public class VectorTwoMuscariable : Muscariable<Vector2>
    {
        public VectorTwoMuscariable() : base() { }

        public override bool IsArithmeticSupported(SetOperator op) => true;
        public override bool IsRelationalSupported => true;

        public virtual float X
        {
            get => Value.x;
            set
            {
                Vector2 newVal = Value;
                newVal.x = value;
                Value = newVal;
                TriggerOnValueChanged();
            }
        }

        public virtual float Y
        {
            get => Value.y;
            set
            {
                Vector2 newVal = Value;
                newVal.y = value;
                Value = newVal;
                TriggerOnValueChanged();
            }
        }

        public static VectorTwoMuscariable operator +(VectorTwoMuscariable a, VectorTwoMuscariable b)
        {
            VectorTwoMuscariable result = CloneMeta(a);
            result.Value += b.Value;
            return result;
        }

        public static VectorTwoMuscariable operator -(VectorTwoMuscariable a, VectorTwoMuscariable b)
        {
            VectorTwoMuscariable result = CloneMeta(a);
            result.Value -= b.Value;
            return result;
        }

        public static VectorTwoMuscariable operator *(VectorTwoMuscariable a, int intVal)
        {
            VectorTwoMuscariable result = CloneMeta(a);
            result.Value *= intVal;
            return result;
        }

        public static VectorTwoMuscariable operator *(VectorTwoMuscariable a, float floatVal)
        {
            VectorTwoMuscariable result = CloneMeta(a);
            result.Value *= floatVal;
            return result;
        }

        public static VectorTwoMuscariable operator *(VectorTwoMuscariable a, IntMuscariable intVar)
        {
            VectorTwoMuscariable result = CloneMeta(a);
            result.Value *= intVar.Value;
            return result;
        }

        public static VectorTwoMuscariable operator *(VectorTwoMuscariable a, FloatMuscariable floatVar)
        {
            VectorTwoMuscariable result = CloneMeta(a);
            result.Value *= floatVar.Value;
            return result;
        }

        public static bool operator ==(VectorTwoMuscariable a, VectorTwoMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator ==(VectorTwoMuscariable a, VectorThreeMuscariable b)
        {
            if (a is null || b is null) return false;
            return a.X == b.X &&
            a.Y == b.Y &&
            b.Z == 0; // Since in a 3D environment, VectorTwos are meant to be treated as if they have a Z of 0
        }


        public static bool operator !=(VectorTwoMuscariable a, VectorTwoMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return a.Value == b.Value;
        }

        public static bool operator !=(VectorTwoMuscariable a, VectorThreeMuscariable b)
        {
            bool oneIsNullTheOtherIsnt = (ReferenceEquals(a, null) && !ReferenceEquals(b, null)) ||
                (ReferenceEquals(b, null) && !ReferenceEquals(a, null));
            if (oneIsNullTheOtherIsnt) return true;

            bool bothAreValid = !ReferenceEquals(a, null) && !ReferenceEquals(b, null);

            if (bothAreValid)
            {
                return a.X != b.X ||
                    a.Y != b.Y ||
                    b.Z != 0;
            }

            return true;
        }

        public static VectorTwoMuscariable CloneMeta(VectorTwoMuscariable src)
        {
            return new VectorTwoMuscariable
            {
                scope = src.scope,
                key = src.key,
                itemID = src.itemID,
                value = src.value
            };
        }

        public override bool Equals(object obj)
        {
            var other = obj as VectorTwoMuscariable;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


    }

    [Serializable]
    [VariableInfo("Numeric/Structured", "VectorThree", typeof(Vector3))]
    public class VectorThreeMuscariable : Muscariable<Vector3>
    {
        public VectorThreeMuscariable() : base() { }

        public override bool IsArithmeticSupported(SetOperator op) => true;
        public override bool IsRelationalSupported => true;

        public virtual float X
        {
            get => Value.x;
            set
            {
                Vector3 newVal = Value;
                newVal.x = value;
                Value = newVal;
                TriggerOnValueChanged();
            }
        }

        public virtual float Y
        {
            get => Value.y;
            set
            {
                Vector3 newVal = Value;
                newVal.y = value;
                Value = newVal;
                TriggerOnValueChanged();
            }
        }

        public virtual float Z
        {
            get => Value.z;
            set
            {
                Vector3 newVal = Value;
                newVal.z = value;
                Value = newVal;
                TriggerOnValueChanged();
            }
        }

        public static VectorThreeMuscariable operator +(VectorThreeMuscariable a, VectorThreeMuscariable b)
            => new VectorThreeMuscariable { Value = a.Value + b.Value };

        public static VectorThreeMuscariable operator -(VectorThreeMuscariable a, VectorThreeMuscariable b)
            => new VectorThreeMuscariable { Value = a.Value - b.Value };

        public static VectorThreeMuscariable operator +(VectorThreeMuscariable a, VectorTwoMuscariable b)
            => new VectorThreeMuscariable
            {
                Value = new Vector3(a.X + b.X,
                a.Y + b.Y,
                a.Z)
            };

        public static VectorThreeMuscariable operator -(VectorThreeMuscariable a, VectorTwoMuscariable b)
            => new VectorThreeMuscariable
            {
                Value = new Vector3(a.X - b.X,
                a.Y - b.Y,
                a.Z)
            };

        public static VectorThreeMuscariable operator *(VectorThreeMuscariable a, int intVal)
            => new VectorThreeMuscariable { Value = a.Value * intVal };

        public static VectorThreeMuscariable operator *(VectorThreeMuscariable a, float floatVal)
            => new VectorThreeMuscariable { Value = a.Value * floatVal };

        public static VectorThreeMuscariable operator *(VectorThreeMuscariable a, IntMuscariable intVar)
            => new VectorThreeMuscariable { Value = a.Value * intVar.Value };

        public static VectorThreeMuscariable operator *(VectorThreeMuscariable a, FloatMuscariable floatVar)
            => new VectorThreeMuscariable { Value = a.Value * floatVar.Value };

        public static bool operator ==(VectorThreeMuscariable a, VectorThreeMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(VectorThreeMuscariable a, VectorThreeMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return a.Value == b.Value;
        }

        public override bool Equals(object obj)
        {
            var other = obj as VectorThreeMuscariable;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


    }

}