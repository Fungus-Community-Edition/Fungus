using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using System.Linq;
using UnityObj = UnityEngine.Object;
using UnityEngine.Serialization;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Invokes a method of a component via reflection. Supports passing multiple parameters and storing returned values in a Fungus variable.
    /// </summary>
    [CommandInfo("Scripting", 
                 "Invoke Method", 
                 "Invokes a method of a component via reflection. Supports passing multiple " +
                 "parameters and storing returned values in an Amanita variable.")]
    [MovedFrom("AtMycelia.Amanita.VScripting.Commands")]
    public class InvokeMethod : Command
    {
        [Tooltip("A description of what this command does. Appears in the command summary.")]
        [HyphlowTextArea(3, 10)]
        [SerializeField] protected StringData _description = new StringData("");

        [Tooltip("GameObject containing the component method to be invoked")]
        [SerializeField] protected GameObjectData _targetObject = new GameObjectData();

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


        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_description);
            _variableDataCache.Add(_targetObject);
        }

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
            componentType ??= ReflectionHelper.GetType(targetComponentAssemblyName);
            if (componentType == null)
            {
                Debug.LogError($"Could not find type with assembly name: {targetComponentAssemblyName} " +
                    $"for method: {targetMethod}", this);
                return;
            }

            if (TargetObject == null)
            {
                Debug.LogError($"TargetObject is not assigned for method: {targetMethod}", this);
                return;
            }

            if (objComponent == null)
            {
                objComponent = TargetObject.GetComponent(componentType);
            }

            parameterTypes ??= GetParameterTypes();
            objMethod ??= UnityEvent.GetValidMethodInfo(objComponent, targetMethod, parameterTypes);
            
        }

        protected virtual IEnumerator ExecuteCoroutine()
        {
            yield return StartCoroutine((IEnumerator)objMethod.Invoke(objComponent, GetParameterValues()));

            if (callMode == CallMode.WaitUntilFinished)
            {
                Continue();
            }
        }

        protected virtual Type[] GetParameterTypes()
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
                    IVariable varFound = flowChart.GetVariable(item.variableKey);
                    if (varFound == null)
                    {
                        string errorMessage = $"No variable found with the name: {item.variableKey} to pass as parameter " +
                            $"to method: {targetMethod}";
                        Debug.LogError(errorMessage);
                        continue;
                    }

                    Type contentType = varFound.ContentType;

                    switch (item.objValue.typeFullname)
                    {
                        case "System.Int32":
                            if (contentType != typeof(int))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was int.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        case "System.Boolean":
                            if (contentType != typeof(bool))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was bool.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        case "System.Single":
                            if (contentType != typeof(float))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was float.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        case "System.String":
                            if (contentType != typeof(string))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was string.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        case "UnityEngine.Color":
                            if (contentType != typeof(Color))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was Color.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        case "UnityEngine.GameObject":
                            if (contentType != typeof(GameObject))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was GameObject.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        case "UnityEngine.Material":
                            if (contentType != typeof(Material))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was Material.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        case "UnityEngine.Sprite":
                            if (contentType != typeof(Sprite))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was Sprite.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        case "UnityEngine.Texture":
                            if (contentType != typeof(Texture))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was Texture.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        case "UnityEngine.Vector2":
                            if (contentType != typeof(Vector2))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was VectorTwo.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        case "UnityEngine.Vector3":
                            if (contentType != typeof(Vector3))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was Vector3.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
                            break;
                        default:
                            if (contentType != typeof(UnityObj))
                            {
                                Debug.LogError($"Variable with key: {item.variableKey} is of type {varFound.ContentType}, " +
                                    $"expected type was UnityObject.");
                                continue;
                            }
                            objValue = varFound.BoxedValue;
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
            IVariable varFound = flowChart.GetVariable(key);
            if (varFound == null)
            {
                string errorMessage = $"No variable found with the name: {key} to store the return value " +
                    $"of method: {targetMethod}";
                Debug.LogError(errorMessage);
                return;
            }
            varFound.BoxedValue = value;
        }

        #region Public members

        /// <summary>
        /// GameObject containing the component method to be invoked.
        /// </summary>
        public virtual GameObject TargetObject
        {
            get
            {
                if (_targetObject.Value != null)
                {
                    return _targetObject.Value;
                }

                if (targetObject != null) // This may execute before the migration is done, so...
                {
                    return targetObject;
                }

                return null;
            }
        }

        public override void OnEnter()
        {
            PrepareTargets();

            if (TargetObject == null || string.IsNullOrEmpty(targetComponentAssemblyName) 
                || string.IsNullOrEmpty(targetMethod))
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
            if (TargetObject == null)
            {
                return "Error: targetObject is not assigned";
            }

            if (!string.IsNullOrEmpty(_description))
            {
                return _description;
            }

            return $"{TargetObject.name}.{targetComponentText}.{targetMethodText}";
        }

        #endregion

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();

            if (!string.IsNullOrEmpty(description))
            {
                _description.Value = description;
                description = null;
            }
            if (targetObject != null)
            {
                _targetObject.Value = targetObject;
                targetObject = null;
            }
        }

        [FormerlySerializedAs("description")]
        [SerializeField] [HideInInspector] protected string description = "";
        [FormerlySerializedAs("targetObject")]
        [SerializeField] [HideInInspector] protected GameObject targetObject;
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
                else if (objType.IsEnum)
                    return Enum.ToObject(objType, intValue);

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
