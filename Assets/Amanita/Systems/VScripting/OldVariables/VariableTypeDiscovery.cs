using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            foreach (var type in allTypes)
            {
                // 1) Legacy Amanita variables
                var legacyAttr = type.GetCustomAttribute<VariableInfoAttribute>();
                if (legacyAttr != null)
                {
                    RegisterVariableType(type);
                    continue;
                }

                // 2) Muscariables
                var muscariAttr = type.GetCustomAttribute<MuscariableAttribute>();
                if (muscariAttr != null)
                {
                    // Use whatever info MuscariableAttribute already exposes
                    RegisterMuscariableType(type, muscariAttr);
                }
            }
        }

        private static IList<Type> allTypes;

        private static void RegisterVariableType(Type varType)
        {
            VariableTypeActions typeActions = new VariableTypeActions()
            {
                CompareFunc = VarCompareFunc,
                DescFunc = VarGetDescription,
                SetFunc = VarSetFunc
            };

            VariableTypeRegistry.Register(varType, typeActions);
            //UnityEngine.Debug.Log($"Registering variable type: {varType.FullName}");
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

        private static void RegisterMuscariableType(Type type, MuscariableAttribute attr)
        {
            // If MuscariableAttribute already contains everything needed for registration,
            // we just pass it through to the registry.
            //VariableTypeRegistry.Register(
            //    type,
            //    (pair, op) => attr.Compare(pair, op),   // or whatever compare delegate Muscariable uses
            //    (pair) => attr.Describe(pair),          // description delegate
            //    (pair, op) => attr.Set(pair, op)        // set delegate
            //);
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

        private static void RegisterVariableDataType(Type varDataType)
        {

        }

    }
}