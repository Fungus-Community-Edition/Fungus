using Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveReaderTypeRegistry
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
            _readerTypes.Clear();
            IList<Type> readerTypesFound = AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(SafeGetTypes)
                         .Where((elem) => IsInstantiatableType(elem, _saveReaderType))
                         .ToList();
            _readerTypes.AddRange(readerTypesFound);
        }

        public static IList<Type> ReaderTypes
        {
            get => _readerTypes.ToList(); // We don't want clients to be able to change the list directly
            set
            {
                _readerTypes.Clear();
                _readerTypes.AddRange(value);
            }
        }
        private static readonly IList<Type> _readerTypes = new List<Type>();

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

        private static Type _saveReaderType = typeof(ISaveReader);

        private static bool IsInstantiatableType(Type typeToCheck, Type baseVarType)
        {
            bool result = typeToCheck.IsConcrete() && baseVarType.IsAssignableFrom(typeToCheck);
            return result;
        }
    }
}