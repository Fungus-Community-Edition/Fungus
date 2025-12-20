using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using Amanita.VScripting;

namespace Amanita.SaveSys
{
    public class VarCodecRegistry
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [UnityEditor.InitializeOnLoadMethod]
        public static void DiscoverAndRegister_Editor()
        {
            UnityEditor.AssemblyReloadEvents.afterAssemblyReload -= Refresh;
            UnityEditor.AssemblyReloadEvents.afterAssemblyReload += Refresh;
        }
#endif

        private static void Refresh()
        {
            codecs.Clear();

            IEnumerable<Type> codecTypes = AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(SafeGetTypes)
                         .Where((elem) => IsInstantiatableType(elem, _iVarCodecType));

            foreach (Type typeEl in codecTypes)
            {
                VarCodecAttribute attr = typeEl.GetCustomAttribute<VarCodecAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (!attr.Active)
                {
                    Debug.Log("CodecRegistry: Skipping inactive codec type " + typeEl.Name);
                    continue;
                }

                // The IsInstantiatableType check should have already filtered out things that
                // don't implement IVarCodec, but just to be safe...
                if (Activator.CreateInstance(typeEl) is IVarCodec codecInstance)
                {
                    codecs.Add(codecInstance);
                }
                else
                {
                    Debug.LogError($"CodecRegistry: Failed to instantiate codec of type " +
                        $"{typeEl.Name}. Reason: it does not implement IVarCodec.");
                }
            }

        }
        private static readonly List<IVarCodec> codecs = new()
        {
            new NumericVarCodec(),
            new BooleanVarCodec(),
            new StringVarCodec(),
            new VectorVarCodec(),
            new ColorVarCodec(),
            new TransformVarCodec(),
            
            /* ... */
        };

        public static IReadOnlyList<IVarCodec> Codecs => codecs.AsReadOnly();

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

        private static readonly Type _iVarCodecType = typeof(IVarCodec);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void DiscoverAndRegister_Runtime()
        {
            Refresh();
        }

        public static IVarCodec GetCodec(IVariable variable)
        {
            var result = codecs.Find(toCheck => toCheck.CanHandle(variable));
            return result;
        }

        public static IVarCodec GetCodec(VariableSaveData saveData)
        {
            var result = codecs.Find(toCheck => toCheck.CanHandle(saveData));
            return result;
        }

        public static IVarCodec GetCodec(string typeName)
        {
            var result = codecs.Find(toCheck => toCheck.CanHandle(typeName));
            return result;
        }

    }
}