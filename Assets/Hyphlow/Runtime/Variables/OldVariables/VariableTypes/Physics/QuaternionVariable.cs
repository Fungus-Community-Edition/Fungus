using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Quaternion variable type.
    /// </summary>
    [VariableInfo("Physics", "Quaternion", typeof(Quaternion))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class QuaternionVariable : VariableBase<Quaternion>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return setOperator == SetOperator.Multiply || base.IsArithmeticSupported(setOperator);
        }

        public override void Apply(SetOperator setOperator, Quaternion value)
        {
            Quaternion local = Value;

            switch (setOperator)
            {
                case SetOperator.Add:
                    Debug.LogWarning("Quarternion Add not supported");
                    break;

                case SetOperator.Subtract:
                    Debug.LogWarning("Quarternion Subtract not supported");
                    break;

                case SetOperator.Multiply:
                    Value = local * value;
                    break;

                case SetOperator.Divide:
                    Debug.LogWarning("Quarternion Divide not supported");
                    break;

                default:
                base.Apply(setOperator, value);
                break;
            }
        }
    }

    /// <summary>
    /// Container for a Quaternion variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Quaternion), typeof(QuaternionVariable))]
    public class QuaternionData : VariableData<Quaternion>
    {
        public QuaternionData() : base(default) { }

        public QuaternionData(Quaternion startVal) : base(startVal)
        {
        }

        

    }
}