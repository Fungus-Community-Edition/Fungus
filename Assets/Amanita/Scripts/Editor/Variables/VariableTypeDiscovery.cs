using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Amanita.VScripting.EditorUtils
{
    public static class VariableTypeDiscovery
    {
        [InitializeOnLoadMethod]
        public static void DiscoverAndRegister()
        {
            return; // Let's go back to this later
            RefreshVariableTypeRegistry();
            // Optional: re-run after domain reload
            AssemblyReloadEvents.afterAssemblyReload -= RefreshVariableTypeRegistry;
            AssemblyReloadEvents.afterAssemblyReload += RefreshVariableTypeRegistry;
        }

        private static void RefreshVariableTypeRegistry()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
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

            foreach (var type in allTypes)
            {
                // 1) Legacy Amanita variables
                var legacyAttr = type.GetCustomAttribute<VariableInfoAttribute>();
                if (legacyAttr != null)
                {
                    //RegisterVariableType(type, legacyAttr.DataPropName);
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

        private static void RegisterVariableType(Type varType, string dataPropName)
        {
            // TODO: Replace with your actual compare/desc/set lambdas
            VariableTypeRegistry.Register(
                varType,
                dataPropName,
                (pair, op) => false, // compare
                pair => "TODO",      // desc
                (pair, op) => { }    // set
            );
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

    }
}