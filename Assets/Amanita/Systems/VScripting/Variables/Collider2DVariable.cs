using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Collider2D variable type.
    /// </summary>
    [VariableInfo("Physics/TwoD", "Collider2D", typeof(Collider2D), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "Amanita.VScripting", "Amanita.Core")]
    public class Collider2DVariable : VariableBase<Collider2D>
    { }

    
}