using Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveWriterTypeRegistry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [InitializeOnLoadMethod]
        public static void DiscoverAndRegister()
        {
            Debug.Log("VariableTypeDiscovery: DiscoverAndRegister called");
            RefreshTypeRegistry();

            AssemblyReloadEvents.afterAssemblyReload -= RefreshTypeRegistry;
            AssemblyReloadEvents.afterAssemblyReload += RefreshTypeRegistry;

        }

        private static void RefreshTypeRegistry()
        {
            _writerTypes.Clear();
            IList<Type> writerTypesFound = AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(SafeGetTypes)
                         .Where((elem) => IsInstantiatableType(elem, _saveWriterType))
                         .ToList();
            _writerTypes.AddRange(writerTypesFound);
        }

        /// <summary>
        /// All of these types are concrete ones that implement ISaveReader.
        /// </summary>
        public static IList<Type> WriterTypes
        {
            get => _writerTypes.ToList(); // We don't want clients to be able to change the list directly
            private set
            {
                _writerTypes.Clear();
                _writerTypes.AddRange(value);
            }
        }
        private static readonly IList<Type> _writerTypes = new List<Type>();

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

        private static readonly Type _saveWriterType = typeof(ISaveWriter);

        private static bool IsInstantiatableType(Type typeToCheck, Type baseVarType)
        {
            bool result = typeToCheck.IsConcrete() && baseVarType.IsAssignableFrom(typeToCheck);
            return result;
        }
    }
}