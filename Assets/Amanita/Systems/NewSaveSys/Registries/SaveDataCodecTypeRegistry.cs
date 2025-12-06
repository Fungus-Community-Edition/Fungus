using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Type = System.Type;
using System.Linq;
using Collections;

namespace Amanita.SaveSys
{
    public class SaveDataCodecTypeRegistry : MonoBehaviour
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
            _codecTypes.Clear();
            IList<Type> typesFound = TypeUtils.GetInstantiatableTypes(_iMainCodecType);//
            _codecTypes.AddRange(typesFound);
        }

        /// <summary>
        /// All of these types are concrete ones that implement ISaveReader.
        /// </summary>
        public static IList<Type> Types
        {
            get => _codecTypes.ToList(); // We don't want clients to be able to change the list directly
            private set
            {
                _codecTypes.Clear();
                _codecTypes.AddRange(value);
            }
        }
        private static readonly IList<Type> _codecTypes = new List<Type>();

        private static readonly Type _iMainCodecType = typeof(IMainSaveCodec);
    }
}