using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Animator variable type.
    /// </summary>
    [VariableInfo("Graphic", "Animator", typeof(Animator), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Amanita.VScripting", "AtMycelia.Amanita.Core")]
    public class AnimatorVariable : VariableBase<Animator>
    {
    }

}
