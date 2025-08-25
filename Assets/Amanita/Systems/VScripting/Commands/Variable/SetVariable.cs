using UnityEngine;

namespace Amanita.VScripting.Commands
{
    /// <summary>
    /// Sets a Boolean, Integer, Float or String variable to a new value using a simple arithmetic operation. The value can be a constant or reference another variable of the same type.
    /// </summary>
    [CommandInfo("Variable",
                 "Set Variable",
                 "Sets a Boolean, Integer, Float or String variable to a new value using a simple arithmetic operation. The value can be a constant or reference another variable of the same type.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class SetVariable : Command, ISerializationCallbackReceiver
    {
        [SerializeField] protected AnyVariableAndDataPair anyVar = new AnyVariableAndDataPair();
        
        [Tooltip("The type of math operation to be performed")]
        [SerializeField] protected SetOperator setOperator;
               
        protected virtual void DoSetOperation()
        {
            if (anyVar.Variable == null)
            {
                return;
            }

            anyVar.SetOp(setOperator);
        }

        #region Public members

        /// <summary>
        /// The type of math operation to be performed.
        /// </summary>
        public virtual SetOperator SetOperator { get { return setOperator; } }

        public override void OnEnter()
        {
            DoSetOperation();

            Continue();
        }

        public override string GetSummary()
        {
            if (anyVar.Variable == null)
            {
                return "Error: Variable not selected";
            }

            string description = anyVar.Variable.Key;
            description += " " + VariableUtil.GetSetOperatorDescription(setOperator) + " ";
            description += anyVar.GetDataDescription();

            return description;
        }

        public override bool HasReference(Variable variable)
        {
            return anyVar.HasReference(variable);
        }

        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }

        #endregion

        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();

            anyVar?.RefreshVariableCacheHelper(GetFlowchart(), ref referencedVariables);
        }
#endif
        #endregion Editor caches

        #region backwards compat


        [Tooltip("Variable to use in expression")]
        [VariableProperty]
        [SerializeField] protected Variable variable;

        [Tooltip("Boolean value to compare against")]
        [SerializeField] protected BooleanData booleanData;

        [Tooltip("Integer value to compare against")]
        [SerializeField] protected IntegerData integerData;

        [Tooltip("Float value to compare against")]
        [SerializeField] protected FloatData floatData;

        [Tooltip("String value to compare against")]
        [SerializeField] protected StringDataMulti stringData;

        [Tooltip("Animator value to compare against")]
        [SerializeField] protected AnimatorData animatorData;

        [Tooltip("AudioSource value to compare against")]
        [SerializeField] protected AudioSourceData audioSourceData;

        [Tooltip("Color value to compare against")]
        [SerializeField] protected ColorData colorData;

        [Tooltip("GameObject value to compare against")]
        [SerializeField] protected GameObjectData gameObjectData;

        [Tooltip("Material value to compare against")]
        [SerializeField] protected MaterialData materialData;

        [Tooltip("Object value to compare against")]
        [SerializeField] protected ObjectData objectData;

        [Tooltip("Rigidbody2D value to compare against")]
        [SerializeField] protected Rigidbody2DData rigidbody2DData;

        [Tooltip("Sprite value to compare against")]
        [SerializeField] protected SpriteData spriteData;

        [Tooltip("Texture value to compare against")]
        [SerializeField] protected TextureData textureData;

        [Tooltip("Transform value to compare against")]
        [SerializeField] protected TransformData transformData;

        [Tooltip("Vector2 value to compare against")]
        [SerializeField] protected Vector2Data vector2Data;

        [Tooltip("Vector3 value to compare against")]
        [SerializeField] protected Vector3Data vector3Data;

        public void OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (variable == null)
            {
                return;
            }
            else
            {
                anyVar.Variable = variable;
            }

            variable = null;
        }
        #endregion
    }
}
