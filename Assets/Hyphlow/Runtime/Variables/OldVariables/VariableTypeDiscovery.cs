using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public static class VariableTypeDiscovery
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [InitializeOnLoadMethod]
        public static void DiscoverAndRegister()
        {
            RefreshVariableTypeRegistry();
            RefreshVariableDataTypeRegistry();

            AssemblyReloadEvents.afterAssemblyReload -= RefreshVariableTypeRegistry;
            AssemblyReloadEvents.afterAssemblyReload -= RefreshVariableDataTypeRegistry;

            AssemblyReloadEvents.afterAssemblyReload += RefreshVariableTypeRegistry;
            AssemblyReloadEvents.afterAssemblyReload += RefreshVariableDataTypeRegistry;
        }

        private static void RefreshVariableTypeRegistry()
        {
            IEnumerable<Type> varSubtypes = AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(SafeGetTypes)
                         .Where((elem) => IsInstantiatableType(elem, _iVariableType) && 
                         !ShouldExcludeDueToBeingForTests(elem));

            SetVarTypeRegistry();
            void SetVarTypeRegistry()
            {
                VariableTypeRegistry.Clear();
                VariableTypeActions typeActions = new VariableTypeActions()
                {
                    CompareFunc = VarCompareFunc,
                    DescFunc = VarGetDescription,
                    SetFunc = VarSetFunc
                };
                VariableTypeRegistry.RegisterMultiVariableTypes(varSubtypes, typeActions);
            }
        }

        static IEnumerable<Type> SafeGetTypes(Assembly toGetTypesFrom)
        {
            try
            {
                return toGetTypesFrom.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(typeFound => typeFound != null);
            }
        }

        private static bool IsInstantiatableType(Type typeToCheck, Type baseVarType)
        {
            bool result = typeToCheck.IsConcrete() && baseVarType.IsAssignableFrom(typeToCheck);
            return result;
        }

        private static readonly Type _iVariableType = typeof(IVariable);

        private static bool VarCompareFunc(IVariable varInvolved, IVariableData varData, CompareOperator compareOp)
        {
            bool result = varInvolved.Evaluate(compareOp, varData.BoxedValue);
            return result;
        }

        private static bool ShouldExcludeDueToBeingForTests(Type typeToCheck)
        {
            VariableInfoAttribute attr = typeToCheck.GetCustomAttribute<VariableInfoAttribute>();
            // We should only include test types if the currently-running scene is a test scene,
            // to avoid cluttering the variable type registry with test types during normal use.
            // This is because test types are often used for testing and debugging purposes, and
            // may not be relevant or useful in a non-test context.
            if (attr != null)
            {
                var activeScene = SceneManager.GetActiveScene();
                string sceneName = activeScene.name;
                bool weAreInTestScene = sceneName.Contains("Test");
                if (attr.IsTest && (!weAreInTestScene || Application.isEditor))
                {
                    Debug.Log($"Excluding test type {typeToCheck.Name} from variable type registry" +
                        $"because the active scene is not a test scene.");
                    return true;
                }
            }

            return false;
        }

        private static string VarGetDescription(IVariableData varData)
        {
            return varData.GetDescription();
        }

        private static void VarSetFunc(IVariable iVar, IVariableData varData, SetOperator setOp)
        {
            iVar.Apply(setOp, varData.BoxedValue);
        }

        private static void RefreshVariableDataTypeRegistry()
        {
            IEnumerable<Type> varDataSubtypes = AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(SafeGetTypes)
                         .Where((elem) => IsInstantiatableType(elem, iVariableDataType));

            VariableDataTypeRegistry.Clear();

            foreach (var elem in varDataSubtypes)
            {
                VariableDataAttribute attr = elem.GetCustomAttribute<VariableDataAttribute>();
                if (attr != null)
                {
                    VariableDataTypeRegistry.Register(elem);
                }
            }
        }

        private static readonly Type iVariableDataType = typeof(IVariableData);

    }
}