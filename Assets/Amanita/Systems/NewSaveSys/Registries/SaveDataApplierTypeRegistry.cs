using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Type = System.Type;
using System.Linq;
using Collections;

namespace Amanita.SaveSys
{
    public class SaveDataApplierTypeRegistry : MonoBehaviour
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
            _applierTypes.Clear();
            IList<Type> typesFound = TypeUtils.GetInstantiatableTypes(_iSaveDataApplierType);
            _applierTypes.AddRange(typesFound);
        }

        /// <summary>
        /// All of these types are concrete ones that implement ISaveReader.
        /// </summary>
        public static IList<Type> Types
        {
            get => _applierTypes.ToList(); // We don't want clients to be able to change the list directly
            private set
            {
                _applierTypes.Clear();
                _applierTypes.AddRange(value);
            }
        }
        private static readonly IList<Type> _applierTypes = new List<Type>();

        private static readonly Type _iSaveDataApplierType = typeof(ISaveDataApplier);
    }
}