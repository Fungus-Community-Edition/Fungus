using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    [CreateAssetMenu(fileName = "VariableRegistryConfig", menuName = "Atelier Mycelia/Amanita/Variable Registry Config")]
    public sealed class VariableRegistryConfig : ScriptableObject
    {
        [SerializeField]
        [FormerlySerializedAs("globalSources")]
        private List<VariableSourceAsset> _globalSources = new List<VariableSourceAsset>();

        public IReadOnlyList<VariableSourceAsset> GlobalSources
        {
            get
            {
                EnsureGlobalSourcesList();
                return _globalSources;
            }
        }

        private void EnsureGlobalSourcesList()
        {
            _globalSources ??= new List<VariableSourceAsset>();
        }

        public void SetGlobalSources(IList<VariableSourceAsset> sources)
        {
            EnsureGlobalSourcesList();
            _globalSources.Clear();

            if (sources == null)
            {
                string errorMessage = $"Attempted to set global sources list to null on {name} " +
                    $"({GetInstanceID()}). This is not allowed. The list will be cleared instead.";
                Debug.LogError(errorMessage, this);

                return;
            }

            _globalSources.AddRange(sources);
            _globalSources.RemoveAll(source => source == null);

            Changed();
        }

        public event Action Changed = delegate { };

        private void OnValidate()
        {
            EnsureGlobalSourcesList();
            Changed();
        }

    }
}