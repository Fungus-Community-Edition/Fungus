using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;

namespace Amanita.VScripting
{
    public static class VariableTypeDiscovery
    {
        [InitializeOnLoadMethod]
        public static void DiscoverAndRegister()
        {
            UnityEngine.Debug.Log("VariableTypeDiscovery: DiscoverAndRegister called");
            RefreshVariableTypeRegistry();
            RefreshVariableDataTypeRegistry();

            AssemblyReloadEvents.afterAssemblyReload -= RefreshVariableTypeRegistry;
            AssemblyReloadEvents.afterAssemblyReload -= RefreshVariableDataTypeRegistry;

            AssemblyReloadEvents.afterAssemblyReload += RefreshVariableTypeRegistry;
            AssemblyReloadEvents.afterAssemblyReload += RefreshVariableDataTypeRegistry;
        }

        private static void RefreshVariableTypeRegistry()
        {
            allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .Where(TypeIsConcreteImplementation)
                .ToArray();

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

            static bool TypeIsConcreteImplementation(Type toEvaluate)
            {
                return !toEvaluate.IsAbstract && !toEvaluate.IsInterface;
            }

            VariableTypeRegistry.Clear();
            foreach (var elem in allTypes)
            {
                var attr = elem.GetCustomAttribute<VariableInfoAttribute>();
                if (attr == null)
                {
                    continue;
                }

                RegisterVariableType(elem, attr.IsLegacy);
            }
        }

        private static IList<Type> allTypes;

        private static void RegisterVariableType(Type varType, bool isLegacy)
        {
            VariableTypeActions typeActions = new VariableTypeActions()
            {
                CompareFunc = VarCompareFunc,
                DescFunc = VarGetDescription,
                SetFunc = VarSetFunc
            };

            VariableTypeRegistry.RegisterVariable(varType, typeActions, isLegacy);
        }

        private static bool VarCompareFunc(IVariable varInvolved, IVariableData varData, CompareOperator compareOp)
        {
            bool result = varInvolved.Evaluate(compareOp, varData.Value);
            return result;
        }

        private static string VarGetDescription(IVariableData varData)
        {
            return varData.GetDescription();
        }

        private static void VarSetFunc(IVariable iVar, IVariableData varData, SetOperator setOp)
        {
            iVar.Apply(setOp, varData.Value);
        }

        private static void RefreshVariableDataTypeRegistry()
        {
            VariableDataRegistry.Clear();

            foreach (var elem in allTypes)
            {
                VariableDataAttribute attr = elem.GetCustomAttribute<VariableDataAttribute>();
                if (attr != null)
                {
                    VariableDataRegistry.Register(elem, attr);
                }
            }
        }

    }
}