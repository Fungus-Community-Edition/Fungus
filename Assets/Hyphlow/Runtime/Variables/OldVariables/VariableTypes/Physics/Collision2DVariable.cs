using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Collision2D variable type.
    /// </summary>
    [VariableInfo("Physics/TwoD", "Collision2D", typeof(Collision2D), IsPreviewedOnly = true)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class Collision2DVariable : VariableBase<UnityEngine.Collision2D>
    { }

    [System.Serializable]
    [VariableData(typeof(Collision2D), typeof(Collision2DVariable))]
    public class Collision2DData : VariableData<Collision2D>
    {
        public static implicit operator Collision2D(Collision2DData Collision2DData)
        {
            return Collision2DData.Value;
        }

        public Collision2DData() : base(default) { }

        public Collision2DData(Collision2D startVal) : base(startVal) { }

    }
}