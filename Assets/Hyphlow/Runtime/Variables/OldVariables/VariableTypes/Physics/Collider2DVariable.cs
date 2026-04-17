using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Collider2D variable type.
    /// </summary>
    [VariableInfo("Physics/TwoD", "Collider2D", typeof(Collider2D), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Collider2DVariable : VariableBase<Collider2D>
    { }

    
}