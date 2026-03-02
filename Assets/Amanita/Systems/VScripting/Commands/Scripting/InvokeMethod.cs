using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using MarkerMetro.Unity.WinLegacy.Reflection;
using System.Linq;

namespace AtMycelia.Amanita.VScripting.Commands
{
    /// <summary>
    /// Invokes a method of a component via reflection. Supports passing multiple parameters and storing returned values in a Fungus variable.
    /// </summary>
    [CommandInfo("Scripting", 
                 "Invoke Method", 
                 "Invokes a method of a component via reflection. Supports passing multiple parameters and storing returned values in a Fungus variable.")]
    public class InvokeMethod : Command
    {
        [Tooltip("A description of what this command does. Appears in the command summary.")]
        [SerializeField] protected string description = "";

        [Tooltip("GameObject containing the component method to be invoked")]
        [SerializeField] protected GameObject targetObject;

        [HideInInspector]
        [Tooltip("Name of assembly containing the target component")]
        [SerializeField] protected string targetComponentAssemblyName;

        [HideInInspector]
        [Tooltip("Full name of the target component")]
        [SerializeField] protected string targetComponentFullname;

        [HideInInspector]
        [Tooltip("Display name of the target component")]
        [SerializeField] protected string targetComponentText;

        [HideInInspector]
        [Tooltip("Name of target method to invoke on the target component")]
        [SerializeField] protected string targetMethod;

        [HideInInspector]
        [Tooltip("Display name of target method to invoke on the target component")]
        [SerializeField] protected string targetMethodText;

        [HideInInspector]
        [Tooltip("List of parameters to pass to the invoked method")]
        [SerializeField] protected InvokeMethodParameter[] methodParameters;

        [HideInInspector]
        [Tooltip("If true, store the return value in a flowchart variable of the same type.")]
        [SerializeField] protected bool saveReturnValue;

        [HideInInspector]
        [Tooltip("Name of Fungus variable to store the return value in")]
        [SerializeField] protected string returnValueVariableKey;

        [HideInInspector]
        [Tooltip("The type of the return value")]
        [SerializeField] protected string returnValueType;

        [HideInInspector]
        [Tooltip("If true, list all inherited methods for the component")]
        [SerializeField] protected bool showInherited;

        [HideInInspector]
        [Tooltip("The coroutine call behavior for methods that return IEnumerator")]
        [SerializeField] protected CallMode callMode;

        protected Type componentType;
        protected Component objComponent;
        protected Type[] parameterTypes = null;
        protected MethodInfo objMethod;

        protected virtual void Awake()
        {
            try
            {
                PrepareTargets();
            }
            catch (Exception)
            {
                Debug.LogError("Rethrowing Exception thrown by:" + GetLocationIdentifier());
                throw;
            }
        }
        
        protected virtual void PrepareTargets()
        { 
            if (componentType == null)
            {
                componentType = ReflectionHelper.GetType(targetComponentAssemblyName);
            }

            if (objComponent == null)
            {
                objComponent = targetObject.GetComponent(componentType);
            }

            if (parameterTypes == null)
            {
                parameterTypes = GetParameterTypes();
            }

            if (objMethod == null)
            {
                objMethod = UnityEvent.GetValidMethodInfo(objComponent, targetMethod, parameterTypes);
            }
        }

        protected virtual IEnumerator ExecuteCoroutine()
        {
            yield return StartCoroutine((IEnumerator)objMethod.Invoke(objComponent, GetParameterValues()));

            if (callMode == CallMode.WaitUntilFinished)
            {
                Continue();
            }
        }

        protected virtual System.Type[] GetParameterTypes()
        {
            System.Type[] types = new System.Type[methodParameters.Length];

            for (int i = 0; i < methodParameters.Length; i++)
            {
                var item = methodParameters[i];
                var objType = ReflectionHelper.GetType(item.objValue.typeAssemblyname);

                types[i] = objType;
            }

            return types;
        }

        protected virtual object[] GetParameterValues()
        {
            object[] values = new object[methodParameters.Length];
            var flowChart = GetFlowchart();

            for (int i = 0; i < methodParameters.Length; i++)
            {
                var item = methodParameters[i];

                if (string.IsNullOrEmpty(item.variableKey))
                {
                    values[i] = item.objValue.GetValue();
                }
                else
                {
                    object objValue = null;

                    switch (item.objValue.typeFullname)
                    {
                        case "System.Int32":
                            var intvalue = flowChart.GetVariableOfTypeByName<IntegerVariable>(item.variableKey);
                            if (intvalue != null)
                                objValue = intvalue.BoxedValue;
                            break;
                        case "System.Boolean":
                            var boolean = flowChart.GetVariableOfTypeByName<BooleanVariable>(item.variableKey);
                            if (boolean != null)
                                objValue = boolean.BoxedValue;
                            break;
                        case "System.Single":
                            var floatvalue = flowChart.GetVariableOfTypeByName<FloatVariable>(item.variableKey);
                            if (floatvalue != null)
                                objValue = floatvalue.BoxedValue;
                            break;
                        case "System.String":
                            var stringvalue = flowChart.GetVariableOfTypeByName<StringVariable>(item.variableKey);
                            if (stringvalue != null)
                                objValue = stringvalue.BoxedValue;
                            break;
                        case "UnityEngine.Color":
                            var color = flowChart.GetVariableOfTypeByName<ColorVariable>(item.variableKey);
                            if (color != null)
                                objValue = color.BoxedValue;
                            break;
                        case "UnityEngine.GameObject":
                            var gameObject = flowChart.GetVariableOfTypeByName<GameObjectVariable>(item.variableKey);
                            if (gameObject != null)
                                objValue = gameObject.BoxedValue;
                            break;
                        case "UnityEngine.Material":
                            var material = flowChart.GetVariableOfTypeByName<MaterialVariable>(item.variableKey);
                            if (material != null)
                                objValue = material.BoxedValue;
                            break;
                        case "UnityEngine.Sprite":
                            var sprite = flowChart.GetVariableOfTypeByName<SpriteVariable>(item.variableKey);
                            if (sprite != null)
                                objValue = sprite.BoxedValue;
                            break;
                        case "UnityEngine.Texture":
                            var texture = flowChart.GetVariableOfTypeByName<TextureVariable>(item.variableKey);
                            if (texture != null)
                                objValue = texture.BoxedValue;
                            break;
                        case "UnityEngine.Vector2":
                            var vector2 = flowChart.GetVariableOfTypeByName<Vector2Variable>(item.variableKey);
                            if (vector2 != null)
                                objValue = vector2.BoxedValue;
                            break;
                        case "UnityEngine.Vector3":
                            var vector3 = flowChart.GetVariableOfTypeByName<Vector3Variable>(item.variableKey);
                            if (vector3 != null)
                                objValue = vector3.BoxedValue;
                            break;
                        default:
                            var obj = flowChart.GetVariableOfTypeByName<ObjectVariable>(item.variableKey);
                            if (obj != null)
                                objValue = obj.BoxedValue;
                            break;
                    }

                    values[i] = objValue;
                }
            }

            return values;
        }

        protected virtual void SetVariable(string key, object value, string returnType)
        {
            var flowChart = GetFlowchart();

            switch (returnType)
            {
                case "System.Int32":
                    flowChart.GetVariableOfTypeByName<IntegerVariable>(key).Value = (int)value;
                    break;
                case "System.Boolean":
                    flowChart.GetVariableOfTypeByName<BooleanVariable>(key).Value = (bool)value;
                    break;
                case "System.Single":
                    flowChart.GetVariableOfTypeByName<FloatVariable>(key).Value = (float)value;
                    break;
                case "System.String":
                    flowChart.GetVariableOfTypeByName<StringVariable>(key).Value = (string)value;
                    break;
                case "UnityEngine.Color":
                    flowChart.GetVariableOfTypeByName<ColorVariable>(key).Value = (UnityEngine.Color)value;
                    break;
                case "UnityEngine.GameObject":
                    flowChart.GetVariableOfTypeByName<GameObjectVariable>(key).Value = (UnityEngine.GameObject)value;
                    break;
                case "UnityEngine.Material":
                    flowChart.GetVariableOfTypeByName<MaterialVariable>(key).Value = (UnityEngine.Material)value;
                    break;
                case "UnityEngine.Sprite":
                    flowChart.GetVariableOfTypeByName<SpriteVariable>(key).Value = (UnityEngine.Sprite)value;
                    break;
                case "UnityEngine.Texture":
                    flowChart.GetVariableOfTypeByName<TextureVariable>(key).Value = (UnityEngine.Texture)value;
                    break;
                case "UnityEngine.Vector2":
                    flowChart.GetVariableOfTypeByName<Vector2Variable>(key).Value = (UnityEngine.Vector2)value;
                    break;
                case "UnityEngine.Vector3":
                    flowChart.GetVariableOfTypeByName<Vector3Variable>(key).Value = (UnityEngine.Vector3)value;
                    break;
                default:
                    flowChart.GetVariableOfTypeByName<ObjectVariable>(key).Value = (UnityEngine.Object)value;
                    break;
            }
        }

        #region Public members

        /// <summary>
        /// GameObject containing the component method to be invoked.
        /// </summary>
        public virtual GameObject TargetObject { get { return targetObject; } }

        public override void OnEnter()
        {
            PrepareTargets();

            if (targetObject == null || string.IsNullOrEmpty(targetComponentAssemblyName) || string.IsNullOrEmpty(targetMethod))
            {
                Continue();
                return;
            }

            if (returnValueType != "System.Collections.IEnumerator")
            {
                var objReturnValue = objMethod.Invoke(objComponent, GetParameterValues());

                if (saveReturnValue)
                {
                    SetVariable(returnValueVariableKey, objReturnValue, returnValueType);
                }

                Continue();
            }
            else
            {
                StartCoroutine(ExecuteCoroutine());

                if (callMode == CallMode.Continue)
                {
                    Continue();
                }
                else if (callMode == CallMode.Stop)
                {
                    StopParentBlock();
                }
            }
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override string GetSummary()
        {
            if (targetObject == null)
            {
                return "Error: targetObject is not assigned";
            }

            if (!string.IsNullOrEmpty(description))
            {
                return description;
            }

            return targetObject.name + "." + targetComponentText + "." + targetMethodText;
        }

        #endregion
    }

    [System.Serializable]
    public class InvokeMethodParameter
    {
        [SerializeField]
        public ObjectValue objValue;

        [SerializeField]
        public string variableKey;
    }

    [System.Serializable]
    public class ObjectValue
    {
        public string typeAssemblyname;
        public string typeFullname;

        public int intValue;
        public bool boolValue;
        public float floatValue;
        public string stringValue;

        public Color colorValue;
        public GameObject gameObjectValue;
        public Material materialValue;
        public UnityEngine.Object objectValue;
        public Sprite spriteValue;
        public Texture textureValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;

        public object GetValue()
        {
            switch (typeFullname)
            {
            case "System.Int32":
                return intValue;
            case "System.Boolean":
                return boolValue;
            case "System.Single":
                return floatValue;
            case "System.String":
                return stringValue;
            case "UnityEngine.Color":
                return colorValue;
            case "UnityEngine.GameObject":
                return gameObjectValue;
            case "UnityEngine.Material":
                return materialValue;
            case "UnityEngine.Sprite":
                return spriteValue;
            case "UnityEngine.Texture":
                return textureValue;
            case "UnityEngine.Vector2":
                return vector2Value;
            case "UnityEngine.Vector3":
                return vector3Value;
            default:
                var objType = ReflectionHelper.GetType(typeAssemblyname);

                if (objType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    return objectValue;
                }
                else if (objType.IsEnum())
                    return System.Enum.ToObject(objType, intValue);

                break;
            }

            return null;
        }
    }

    public static class ReflectionHelper
    {
        static Dictionary<string, System.Type> types = new Dictionary<string, System.Type>();

        public static System.Type GetType(string AssemblyQualifiedNameTypeName)
        {
            if (types.ContainsKey(AssemblyQualifiedNameTypeName) && types[AssemblyQualifiedNameTypeName] != null)
                return types[AssemblyQualifiedNameTypeName];

            types[AssemblyQualifiedNameTypeName] = AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.AssemblyQualifiedName == AssemblyQualifiedNameTypeName);

            return types[AssemblyQualifiedNameTypeName];
        }
    }
}
