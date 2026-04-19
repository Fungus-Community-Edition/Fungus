using System.Collections.Generic;
using System.Text;
using UnityEngine;

using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public abstract class VariableCondition : Condition, ISerializationCallbackReceiver
    {
        public enum AnyOrAll
        {
            AnyOf_OR, // Use as a chain of ORs
            AllOf_AND, // Use as a chain of ANDs
        }

        [Tooltip("Selecting AnyOf will result in true if at least one of the conditions is true. Selecting AllOF will result in true only when all the conditions are true.")]
        [SerializeField]
        [FormerlySerializedAs("anyOrAllConditions")]
        protected AnyOrAll _anyOrAllConditions;
        [SerializeField]
        [FormerlySerializedAs("conditions")]
        protected List<ConditionExpression> _conditions = new List<ConditionExpression>();

        [HideInInspector]
        [FormerlySerializedAs("compareOperator")]
        [SerializeField] protected CompareOperator _compareOperator;

        [HideInInspector]
        [FormerlySerializedAs("anyVar")]
        [SerializeField] protected AnyVariableAndDataPair _anyVar;

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();

            _conditions ??= new List<ConditionExpression>();

            if (_conditions.Count == 0)
            {
                _conditions.Add(new ConditionExpression());
            }
        }

        protected override bool EvaluateCondition()
        {
            if (_conditions == null || _conditions.Count == 0)
            {
                return false;
            }

            bool resultAny = false, resultAll = true;
            foreach (ConditionExpression condition in _conditions)
            {
                bool curResult = false;
                if (condition.AnyVar == null)
                {
                    resultAll &= curResult;
                    resultAny |= curResult;
                    continue;
                }
                condition.AnyVar.Compare(condition.CompareOperator, ref curResult);
                resultAll &= curResult;
                resultAny |= curResult;
            }

            if (_anyOrAllConditions == AnyOrAll.AnyOf_OR) return resultAny;

            return resultAll;
        }

        protected override bool HasNeededProperties()
        {
            if (_conditions == null || _conditions.Count == 0)
            {
                return false;
            }

            foreach (ConditionExpression condition in _conditions)
            {
                if (condition.AnyVar == null || condition.AnyVar.LhsVariable == null)
                {
                    return false;
                }
            }
            return true;
        }
        
        public override string GetSummary()
        {
            if (!this.HasNeededProperties())
            {
                return "Error: No variable selected";
            }

            string connector = "";
            if (_anyOrAllConditions == AnyOrAll.AnyOf_OR)
            {
                connector = " <b>OR</b> ";
            }
            else
            {
                connector = " <b>AND</b> ";
            }

            StringBuilder summary = new StringBuilder("");
            for (int i = 0; i < _conditions.Count; i++)
            {
                var currentCond = _conditions[i];
                var anyVar = currentCond.AnyVar;
                var lhsVar = anyVar.LhsVariable;
                string lhsVarStr = lhsVar != null ? lhsVar.Key : "null";
                if (lhsVar != null && lhsVar.Owner != null && !ReferenceEquals(lhsVar.Owner, GetFlowchart()))
                {
                    lhsVarStr = lhsVar.Owner.Name + "." + lhsVarStr;
                }

                string opDesc = VariableUtil.GetCompareOperatorDescription(currentCond.CompareOperator);
                string whatToAppend = $"{lhsVarStr} {opDesc} {anyVar.GetDataDescription()}";
                summary.Append(whatToAppend);

                if (i < _conditions.Count - 1)
                {
                    summary.Append(connector);
                }
            }
            return summary.ToString();
        }

        public override bool HasReference(Variable variable)
        {
            return _anyVar.HasReference(variable);
        }

        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();

            if (_conditions != null)
            {
                foreach (var item in _conditions)
                {
                    item.AnyVar.RefreshVariableCacheHelper(GetFlowchart(), ref referencedVariables);
                }
            }
        }
#endif
        #endregion Editor caches

        #region Backwards compat

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

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return; // In case the object was deleted before the delayed call
                _anyVar?.Refresh();
            };
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.isPlaying)
            {
                return;
            }

            if (variable != null)
            {
                _anyVar.LhsVariable = variable;
                // ^This should immediately update the var data
            }

            // just checking for anyVar != null fails here. Is any var being reintilaized somewhere?

            if (_anyVar != null && _anyVar.LhsVariable != null)
            {
                ConditionExpression cond = new ConditionExpression(_compareOperator, _anyVar);
                if (!_conditions.Contains(cond))
                {
                    _conditions.Add(cond);
                }

                _anyVar = null;
                variable = null;
            }
        }

        #endregion backwards compat
    }
}