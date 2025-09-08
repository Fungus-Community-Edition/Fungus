using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Material variable type.
    /// </summary>
    [VariableInfo("Graphic", "Material", typeof(Material))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class MaterialVariable : VariableBase<Material>
    {
    }

    /// <summary>
    /// Container for a Material variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Material), typeof(MaterialVariable))]
    public class MaterialData : VariableData<Material>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(MaterialVariable))]
        public IVariable<Material> materialRef;

        public MaterialData() : base(default) { }
        public MaterialData(Material startVal = null) : base(startVal) { }

        public static implicit operator Material(MaterialData materialData)
        {
            return materialData.Value;
        }

        public override IVariable VarRef
        {
            get { return materialRef; }
            set
            {
                if (value == null) { materialRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    materialRef = value as MaterialVariable;
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