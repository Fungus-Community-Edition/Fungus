using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Animator variable type.
    /// </summary>
    [VariableInfo("Other", "Animator", "Animator")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class AnimatorVariable : VariableBase<Animator>
    {
    }

    [System.Serializable]
    [VariableData(typeof(Animator), typeof(AnimatorVariable))]
    public class AnimatorData : VariableData<Animator, IVariable<Animator>>
    {
        [SerializeField] [VariableProperty("<Value>", typeof(AnimatorVariable))]
        public AnimatorVariable animatorRef;

        public static implicit operator Animator(AnimatorData animatorData)
        {
            return animatorData.Value;
        }

        public AnimatorData() : base(default) { }
        public AnimatorData(Animator startVal = default) : base(startVal) { }

        public override IVariable VarRef
        {
            get { return animatorRef; }
            set
            {
                if (value == null) { animatorRef = null; return; }
                // TODO: Refactor this setter so it works with polymorphism
                if (value.ContentType.Equals(this.ContentType))
                {
                    animatorRef = value as AnimatorVariable;
                }
                else
                {
                    string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
                    throw new System.InvalidCastException(errorMessage);
                }

            }
        }
    }

}