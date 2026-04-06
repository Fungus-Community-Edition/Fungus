using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityObj = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AtMycelia.Amanita.VScripting
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class VariableManagerComponent : MonoBehaviour, IVariableSource, IMuscariableSource,
        IReorderableVariableSource, IReorderableMuscariableSource
    {
        [SerializeField, HideInInspector] private UnityObj _unityObjOwner;
        [SerializeField, HideInInspector] private VariableManager _variableManager = new VariableManager();
        [SerializeField, HideInInspector] private Flowchart _cachedFlowchart;

        public IVariableSource Owner
        {
            get
            {
                if (_unityObjOwner is IVariableSource varSource)
                {
                    return varSource;
                }
                else
                {
                    return this;
                }
            }
            set
            {
                _unityObjOwner = value as UnityObj;
                _variableManager.VarOwner = value;
            }
        }
        public IReadOnlyList<IVariable> Variables => _variableManager.Variables;

        public string UniqueId => _variableManager.UniqueId;

        public string Name { get => _variableManager.Name; set => _variableManager.Name = value; }

        IReadOnlyList<Muscariable> IVariableSource<Muscariable>.Variables => ((IVariableSource<Muscariable>)_variableManager).Variables;

        public event Action<IVariable> VariableAdded
        {
            add
            {
                _variableManager.VariableAdded += value;
            }

            remove
            {
                _variableManager.VariableAdded -= value;
            }
        }

        public event Action<IVariable> VariableRemoved
        {
            add
            {
                _variableManager.VariableRemoved += value;
            }

            remove
            {
                _variableManager.VariableRemoved -= value;
            }
        }

        protected virtual void Awake()
        {
            EnsureOwner();
            _variableManager.Refresh();
        }

        protected virtual void EnsureOwner()
        {
            if (_unityObjOwner is IVariableSource)
            {
                return;
            }

            if (_cachedFlowchart == null)
            {
                _cachedFlowchart = GetComponent<Flowchart>();
            }

            if (_cachedFlowchart == null)
            {
                Debug.LogWarning("VariableManagerComponent requires a Flowchart component to act as owner.");
                return;
            }

            Owner = _cachedFlowchart;
        }

        private void OnEnable()
        {
            EnsureOwner();
            _variableManager.OnEnable();
        }

        private void OnDisable()
        {
            _variableManager.OnDisable();
        }

        public void Refresh()
        {
            EnsureOwner();
            _variableManager.Refresh();
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Atelier Mycelia/Amanita/Migrate Flowchart Variables", false, 2000)]
        private static void MigrateAllFlowchartVariables()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("VariableManagerComponent: Cannot migrate while in Play Mode.");
                return;
            }

            Flowchart[] flowcharts = FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
            int migratedCount = 0;

            for (int i = 0; i < flowcharts.Length; i++)
            {
                Flowchart flowchart = flowcharts[i];
                if (flowchart == null)
                {
                    continue;
                }

                if (!flowchart.gameObject.scene.IsValid() || !flowchart.gameObject.scene.isLoaded)
                {
                    continue;
                }

                VariableManagerComponent component = flowchart.GetComponent<VariableManagerComponent>();
                if (component == null)
                {
                    component = Undo.AddComponent<VariableManagerComponent>(flowchart.gameObject);
                }

                component.MigrateFromFlowchart();
                migratedCount++;
            }

            Debug.Log($"VariableManagerComponent: Migrated variables for {migratedCount} Flowchart(s).");
        }

        public void MigrateFromFlowchart()
        {
            EnsureOwner();
            _cachedFlowchart = _unityObjOwner as Flowchart;
            if (_cachedFlowchart == null)
            {
                return;
            }

            _cachedFlowchart.GetVariableManagerMigrationData(out VariableManager legacyManager,
                out IList<Muscariable> oldMuscariables,
                out IList<Variable> legacyVariables);

            List<Muscariable> muscariablesToMigrate = new List<Muscariable>();
            List<Variable> legacyVarsToMigrate = new List<Variable>();

            if (legacyManager != null)
            {
                muscariablesToMigrate.AddRange(legacyManager.Variables.OfType<Muscariable>());
                legacyVarsToMigrate.AddRange(legacyManager.Variables.OfType<Variable>());
            }

            if (oldMuscariables != null)
            {
                muscariablesToMigrate.AddRange(oldMuscariables);
            }

            if (legacyVariables != null)
            {
                legacyVarsToMigrate.AddRange(legacyVariables);
            }

            if (muscariablesToMigrate.Count == 0 && legacyVarsToMigrate.Count == 0)
            {
                Debug.Log("VariableManagerComponent: No Flowchart variables found to migrate.");
                return;
            }

            _variableManager.MigrateLegacyVariables(muscariablesToMigrate, legacyVarsToMigrate);
            _cachedFlowchart.ClearVariableManagerMigrationData();
            _variableManager.Refresh();
            Owner = _cachedFlowchart;

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(_cachedFlowchart);
        }

        public IVariable AddVariable(IVariable toAdd)
        {
            return _variableManager.AddVariable(toAdd);
        }

        public void RemoveVariable(IVariable toRemove)
        {
            _variableManager.RemoveVariable(toRemove);
        }

        public IVariable GetVariable(byte itemId)
        {
            return _variableManager.GetVariable(itemId);
        }

        T IVariableSource.GetVariableOfType<T>()
        {
            return _variableManager.GetVariableOfType<T>();
        }

        public IVariable GetVariable(string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            return _variableManager.GetVariable(name, strCompare);
        }

        T IVariableSource.GetVariableOfType<T>(string name, StringComparison strCompare)
        {
            return _variableManager.GetVariableOfType<T>(name, strCompare);
        }

        public IVariable GetVariableOfType(Type type, string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            return _variableManager.GetVariableOfType(type, name, strCompare);
        }

        public bool Contains(IVariable var)
        {
            return _variableManager.Contains(var);
        }

        public void ReorderVariables(IList<IVariable> newlyOrderedVars)
        {
            _variableManager.ReorderVariables(newlyOrderedVars);
        }

        /// <summary>
        /// Value is default, scope is private. If you want to specify those, use the generic version of this method.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public Muscariable AddNewVariableOfContentType(Type contentType, string key)
        {
            var result = _variableManager.AddNewVariableOfContentType(contentType, key);
            return result;
        }

        public Muscariable AddVariable(Muscariable toAdd)
        {
            return _variableManager.AddVariable(toAdd);
        }

        public void RemoveVariable(Muscariable toRemove)
        {
            _variableManager.RemoveVariable(toRemove);
        }

        public void Clear()
        {
            _variableManager.Clear();
        }

        public Muscariable AddNewVariableOfContentType<TContentType>(string key, TContentType defaultVal = default, 
            VariableScope scope = VariableScope.Private)
        {
            return _variableManager.AddNewVariableOfContentType(key, defaultVal, scope);
        }
#endif
    }

}