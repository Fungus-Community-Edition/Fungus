using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow
{
    [CreateAssetMenu(fileName = "VariableRegistryConfig", menuName = "Atelier Mycelia/Amanita/Variable Registry Config")]
    public sealed class VariableRegistryConfig : ScriptableObject
    {
        [SerializeField] private List<VariableSourceAsset> globalSources = new List<VariableSourceAsset>();

        public IReadOnlyList<VariableSourceAsset> GlobalSources
        {
            get
            {
                EnsureGlobalSourcesList();
                return globalSources;
            }
        }

        private void EnsureGlobalSourcesList()
        {
            globalSources ??= new List<VariableSourceAsset>();
        }

        public void SetGlobalSources(IList<VariableSourceAsset> sources)
        {
            EnsureGlobalSourcesList();
            globalSources.Clear();

            if (sources == null)
            {
                string errorMessage = $"Attempted to set global sources list to null on {name} " +
                    $"({GetInstanceID()}). This is not allowed. The list will be cleared instead.";
                Debug.LogError(errorMessage, this);

                return;
            }

            globalSources.AddRange(sources);
            globalSources.RemoveAll(source => source == null);

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