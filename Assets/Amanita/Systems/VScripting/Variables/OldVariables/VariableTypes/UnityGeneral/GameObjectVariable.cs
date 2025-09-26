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
    public class GameObjectData : VariableData<GameObject>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(GameObjectVariable))]
        public IVariable<GameObject> gameObjectRef;

        public GameObjectData() : base(default) { }
        public GameObjectData(GameObject startVal = null) : base(startVal) { }

        public override void Refresh()
        {
            varRef ??= gameObjectRef;
        }
    }
}