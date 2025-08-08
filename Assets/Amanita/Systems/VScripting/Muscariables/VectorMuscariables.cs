using UnityEngine;

namespace Amanita.VScripting
{
    [System.Serializable]
    [Muscariable("Physics/Vector", typeof(Vector2), "VectorTwo")]
    public class VectorTwoMuscariable : Muscariable<Vector2>
    {
        public override bool IsArithmeticSupported => true;
        public override bool IsRelationalSupported => true;

        public virtual float X
        {
            get => Value.x;
            set
            {
                Vector2 newVal = Value;
                newVal.x = value;
                Value = newVal;
                InvokeOnValueChanged();
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
                InvokeOnValueChanged();
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
                valOfType = src.valOfType
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

    [System.Serializable]
    [Muscariable("Physics/Vector", typeof(Vector3), "VectorThree")]
    public class VectorThreeMuscariable : Muscariable<Vector3>
    {
        public override bool IsArithmeticSupported => true;
        public override bool IsRelationalSupported => true;

        public virtual float X
        {
            get => Value.x;
            set
            {
                Vector3 newVal = Value;
                newVal.x = value;
                Value = newVal;
                InvokeOnValueChanged();
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
                InvokeOnValueChanged();
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
                InvokeOnValueChanged();
            }
        }

        public static VectorThreeMuscariable operator +(VectorThreeMuscariable a, VectorThreeMuscariable b)
            => new VectorThreeMuscariable { Value = a.Value + b.Value };

        public static VectorThreeMuscariable operator -(VectorThreeMuscariable a, VectorThreeMuscariable b)
            => new VectorThreeMuscariable { Value = a.Value - b.Value };

        public static VectorThreeMuscariable operator +(VectorThreeMuscariable a, VectorTwoMuscariable b)
            => new VectorThreeMuscariable { Value = new Vector3(a.X + b.X,
                a.Y + b.Y,
                a.Z) };

        public static VectorThreeMuscariable operator -(VectorThreeMuscariable a, VectorTwoMuscariable b)
            => new VectorThreeMuscariable { Value = new Vector3(a.X - b.X,
                a.Y - b.Y,
                a.Z) };

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