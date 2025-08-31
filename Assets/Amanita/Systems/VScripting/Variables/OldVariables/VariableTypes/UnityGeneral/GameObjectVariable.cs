


using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// GameObject variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "GameObject", typeof(GameObject))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class GameObjectVariable : VariableBase<GameObject>
    {
    }

    /// <summary>
    /// Container for a GameObject variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(GameObject), typeof(GameObjectVariable))]
    public class GameObjectData : VariableData<GameObject, IVariable<GameObject>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(GameObjectVariable))]
        public GameObjectVariable gameObjectRef;

        public GameObjectData() : base(default) { }
        public GameObjectData(GameObject startVal = null) : base(startVal) { }

        public static implicit operator GameObject(GameObjectData gameObjectData)
        {
            return gameObjectData.Value;
        }

        public override IVariable VarRef
        {
            get { return gameObjectRef; }
            set
            {
                if (value == null) { gameObjectRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    gameObjectRef = value as GameObjectVariable;
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