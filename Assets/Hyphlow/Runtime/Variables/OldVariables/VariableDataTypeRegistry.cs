using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public static class VariableDataTypeRegistry
    {
        // Key is var type, value is content type
        private static readonly IDictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();

        /// <summary>
        /// Key: IVariable-implementor.
        /// Value: IVariableData-implementor.
        /// </summary>
        public static IReadOnlyDictionary<Type, Type> TypeMap => 
            _typeMap as IReadOnlyDictionary<Type, Type>;

        public static void Clear()
        {
            _typeMap.Clear();
        }

        public static void Register(Type varDataType)
        {
            if (varDataType == null)
            {
                Debug.LogWarning($"Passed null varDataType to VariableDataRegistry Register func");
                return;
            }

            if (!ShouldRegister(varDataType))
            {
                return;
            }

            VariableDataAttribute attr = varDataType.GetCustomAttribute<VariableDataAttribute>();
            if (attr == null)
            {
                Debug.LogWarning($"VariableDataTypeRegistry: {varDataType.Name} is missing VariableDataAttribute.");
                return;
            }

            IList<Type> compatibleVarTypes = attr.VariableTypes.Where((elem) => elem != null).ToList();

            foreach (var varTypeEl in compatibleVarTypes)
            {
                _typeMap.TryAdd(varTypeEl, varDataType);
            }
        }

        private static bool ShouldRegister(Type varDataType)
        {
            if (!Application.isPlaying) 
            {
                var activeScene = SceneManager.GetActiveScene();
                string sceneName = activeScene.name;
                bool isTestScene = string.IsNullOrEmpty(sceneName) ||
                    sceneName.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0;

                if (isTestScene)
                {
                    return true;
                }

                // Only check assemblies outside of Play Mode. It's during Play Mode
                // that unit tests might want to screw with the registry, and we
                // don't want to prevent that.
                string assemblyName = varDataType.Assembly.GetName().Name;
                
                if ( !string.IsNullOrEmpty(assemblyName) &&
                    assemblyName.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }

                string fullName = varDataType.FullName;
                if (!string.IsNullOrEmpty(fullName) &&
                    fullName.IndexOf(".tests.", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }

                return true;
            }
            return true;
        }

        public static IVariableData CreateForVar<TVarType>() where TVarType: IVariable
        {
            return CreateForVar(typeof(TVarType));
        }

        public static IVariableData CreateForVar(Type variableType)
        {
            var varDataType = GetDataTypeLinkedToVarType(variableType);
            IVariableData result = null;
            if (varDataType != null)
            {
                result = (IVariableData)Activator.CreateInstance(varDataType);
            }
            else
            {
                Debug.Log($"Couldn't make an instance for {variableType.Name}. The amount of types " +
                    $"in the registry: {_typeMap.Count}");
            }
            
            return result;
        }

        public static IVariableData CreateForContentType(Type contentType)
        {
            var dataType = _typeMap.Values.FirstOrDefault((dt) =>
            {
                VariableDataAttribute attr = dt.GetCustomAttribute<VariableDataAttribute>();
                return attr != null && attr.ContentType.Equals(contentType);
            });
            IVariableData result = null;
            if (dataType != null)
            {
                result = (IVariableData)Activator.CreateInstance(dataType);
            }
            else
            {
                Debug.Log($"Couldn't find a variable data type for content type {contentType.Name}. The amount of types " +
                    $"in the registry: {_typeMap.Count}");
            }
            
            return result;
        }

        public static Type GetDataTypeLinkedToVarType(Type variableType)
        {
            // For polymorphism, we need to check assignability
            foreach (var val in _typeMap.Values)
            {
                VariableDataAttribute attr = val.GetCustomAttribute<VariableDataAttribute>();
                if (attr != null)
                {
                    foreach (var varTypeEl in attr.VariableTypes)
                    {
                        if (varTypeEl != null && varTypeEl.IsAssignableFrom(variableType))
                        {
                            return val;
                        }
                    }
                }
            }
            return null;
        }

    }

}