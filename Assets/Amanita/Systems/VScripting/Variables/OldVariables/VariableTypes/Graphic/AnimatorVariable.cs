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

}