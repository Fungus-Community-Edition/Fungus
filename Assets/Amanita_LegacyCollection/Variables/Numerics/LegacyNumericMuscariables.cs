using System;
using UnityEngine;

namespace Amanita.VScripting
{
    [Serializable]
    [VariableInfo("Numeric/Structured", "VectorFour", typeof(Vector4))]
    public class VectorFourMuscariable : Muscariable<Vector4>
    {
        public VectorFourMuscariable() : base() { }

        public override bool IsArithmeticSupported(SetOperator op) => true;
        public override bool IsRelationalSupported => true;

        public virtual float X
        {
            get => Value.x;
            set
            {
                Vector4 newVal = Value;
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
                Vector4 newVal = Value;
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
                Vector4 newVal = Value;
                newVal.z = value;
                Value = newVal;
                TriggerOnValueChanged();
            }
        }

        public virtual float W
        {
            get => Value.w;
            set
            {
                Vector4 newVal = Value;
                newVal.w = value;
                Value = newVal;
                TriggerOnValueChanged();
            }
        }

        public static VectorFourMuscariable operator +(VectorFourMuscariable a, VectorFourMuscariable b)
            => new VectorFourMuscariable { Value = a.Value + b.Value };

        public static VectorFourMuscariable operator -(VectorFourMuscariable a, VectorFourMuscariable b)
            => new VectorFourMuscariable { Value = a.Value - b.Value };

        public static VectorFourMuscariable operator +(VectorFourMuscariable a, VectorTwoMuscariable b)
            => new VectorFourMuscariable
            {
                Value = new Vector4(a.X + b.X,
                a.Y + b.Y,
                a.Z,
                a.W)
            };

        public static VectorFourMuscariable operator -(VectorFourMuscariable a, VectorTwoMuscariable b)
            => new VectorFourMuscariable
            {
                Value = new Vector4(a.X - b.X,
                a.Y - b.Y,
                a.Z,
                a.W)
            };

        public static VectorFourMuscariable operator *(VectorFourMuscariable a, int intVal)
            => new VectorFourMuscariable { Value = a.Value * intVal };

        public static VectorFourMuscariable operator *(VectorFourMuscariable a, float floatVal)
            => new VectorFourMuscariable { Value = a.Value * floatVal };

        public static VectorFourMuscariable operator *(VectorFourMuscariable a, IntMuscariable intVar)
            => new VectorFourMuscariable { Value = a.Value * intVar.Value };

        public static VectorFourMuscariable operator *(VectorFourMuscariable a, FloatMuscariable floatVar)
            => new VectorFourMuscariable { Value = a.Value * floatVar.Value };

        public static bool operator ==(VectorFourMuscariable a, VectorFourMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(VectorFourMuscariable a, VectorFourMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return a.Value == b.Value;
        }

        public override bool Equals(object obj)
        {
            var other = obj as VectorFourMuscariable;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


    }

    [Serializable]
    [VariableInfo("Numeric/Structured", "MatrixFourByFour", typeof(Matrix4x4))]
    public class MatrixFourByFourMuscariable : Muscariable<Matrix4x4>
    {
        public MatrixFourByFourMuscariable() : base() { }
        public override bool IsArithmeticSupported(SetOperator op) => false;
        public override bool IsRelationalSupported => false;
    }
}