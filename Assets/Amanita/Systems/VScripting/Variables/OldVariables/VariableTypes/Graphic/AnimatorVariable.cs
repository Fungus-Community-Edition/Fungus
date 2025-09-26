using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Animator variable type.
    /// </summary>
    [VariableInfo("Graphic", "Animator", typeof(Animator))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class AnimatorVariable : VariableBase<Animator>
    {
    }

    [System.Serializable]
    [VariableData(typeof(Animator), typeof(AnimatorVariable))]
    public class AnimatorData : VariableData<Animator>
    {
        [SerializeField, SerializeReference] [VariableProperty("<Value>", typeof(AnimatorVariable))]
        public IVariable<Animator> animatorRef;

        public static implicit operator Animator(AnimatorData animatorData)
        {
            return animatorData.Value;
        }

        public AnimatorData() : base(default) { }
        public AnimatorData(Animator startVal = default) : base(startVal) { }

        public override void Refresh()
        {
            varRef ??= animatorRef;
        }
    }

}