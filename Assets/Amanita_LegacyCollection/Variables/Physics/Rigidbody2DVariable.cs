using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Rigidbody2D variable type.
    /// </summary>
    [VariableInfo("Physics/TwoD", "Rigidbody2D", typeof(Rigidbody2D))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class Rigidbody2DVariable : VariableBase<Rigidbody2D>
    {
    }

    /// <summary>
    /// Container for a Rigidbody2D variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Rigidbody2D), typeof(Rigidbody2DVariable))]
    public class Rigidbody2DData : VariableData<Rigidbody2D>
    {
        public Rigidbody2DData() : base(default) { }

        public Rigidbody2DData(Rigidbody2D startVal) : base(startVal)
        {
        }

    }
}