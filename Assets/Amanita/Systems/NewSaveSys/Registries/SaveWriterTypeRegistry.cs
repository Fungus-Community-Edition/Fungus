using Collections;
using System;
using System.Collections.Generic;
using System.Linq;
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
            RefreshTypeRegistry();

            AssemblyReloadEvents.afterAssemblyReload -= RefreshTypeRegistry;
            AssemblyReloadEvents.afterAssemblyReload += RefreshTypeRegistry;
        }

        private static void RefreshTypeRegistry()
        {
            _writerTypes.Clear();
            IList<Type> writerTypesFound = TypeUtils.GetInstantiatableTypes(_iSaveWriterType);
            _writerTypes.AddRange(writerTypesFound);
        }

        /// <summary>
        /// All of these types are concrete ones that implement ISaveReader.
        /// </summary>
        public static IList<Type> Types
        {
            get => _writerTypes.ToList(); // We don't want clients to be able to change the list directly
            private set
            {
                _writerTypes.Clear();
                _writerTypes.AddRange(value);
            }
        }

        private static readonly IList<Type> _writerTypes = new List<Type>();

        private static readonly Type _iSaveWriterType = typeof(ISaveWriter);

    }
}