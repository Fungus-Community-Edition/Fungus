using AtMycelia.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Type = System.Type;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [CreateAssetMenu(fileName = "NewVariableSourceAsset", menuName = "Atelier Mycelia/Amanita/VariableSource")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class VariableSourceAsset : ScriptableObject, IReorderableMuscariableSource,
        IForceResetUidHandler, IRefreshable
    {
        [SerializeField] private bool _includeInSaves = true;
        [FormerlySerializedAs("uniqueId")]
        [SerializeField, HideInInspector] private string _uniqueId = string.Empty;
        [SerializeField] private bool _alwaysKeepGuid = true;
        [SerializeReference] private List<Muscariable> variables = new List<Muscariable>();
        [SerializeField, HideInInspector] private VariableManager _varManager = new VariableManager();

        public virtual void ForceResetUid()
        {
            UniqueId = Guid.NewGuid().ToString();
        }

        public bool IncludeInSaves
        {
            get => _includeInSaves;
            set => _includeInSaves = value;
        }

        public string UniqueId
        {
            get => _uniqueId;
            set
            {
                if (!string.IsNullOrEmpty(_uniqueId))
                {
                    Debug.LogWarning($"Warning: Overwriting existing AssetId on VariableSourceAsset {name}.",
                        this);
                }

                string prevId = _uniqueId;
                _uniqueId = value;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssetIfDirty(this);
                    AssetDatabase.Refresh();
                }
#endif
            }
        }

        public virtual bool AlwaysKeepGuid
        {
            get => _alwaysKeepGuid;
            set => _alwaysKeepGuid = value;
        }

        public IReadOnlyList<IVariable> Variables
        {
            get
            {
                return _varManager.Variables;
            }
        }

        IReadOnlyList<Muscariable> IVariableSource<Muscariable>.Variables
        {
            get
            {
                return _varManager.Variables.OfType<Muscariable>().ToList();
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Creates and returns a new Muscariable of the content type,
        /// assigning it the passed key and starting value.
        /// Ignores the scope param, as VariableSourceAssets always
        /// have (functionally) global scopes for their vars.
        /// </summary>
        public virtual Muscariable<TContent> AddNewVariableOfContentType<TContent>(string key,
            TContent startingVal = default, VariableScope scope = VariableScope.Private)
        {
            var result = _varManager.AddNewVariable(key, startingVal, VariableScope.Public);
            return (Muscariable<TContent>)result; 
        }

        public virtual Muscariable AddNewVariableOfContentType(Type contentType, string key, object defaultVal,
            VariableScope scope = VariableScope.Private)
        {
            var result = _varManager.AddNewVariableOfContentType(contentType, key, defaultVal, scope);
            return result;
        }

        /// <summary>
        /// If the var is a legacy one, it will be converted to a Muscariable. Returns
        /// the variable added.
        /// </summary>
        public virtual IVariable AddVariable(IVariable var)
        {
            var result = _varManager.AddVariable(var);
            return result;
        }

        public event Action VariablesReordered
        {
            add
            {
                _varManager.Reordered += value;
            }
            remove
            {
                _varManager.Reordered -= value;
            }
        }

        [SerializeField, HideInInspector] protected byte _nextVarID = 1;
        public event Action<IVariable> VariableAdded
        {
            add
            {
                _varManager.VariableAdded += value;
            }
            remove
            {
                _varManager.VariableAdded -= value;
            }
        }

        public Muscariable GetVariableByName(string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            var result = _varManager.GetVariable(name, strCompare);
            return (Muscariable)result;
        }

        public virtual IVariable GetVariable(byte itemID)
        {
            var result = _varManager.GetVariable(itemID);
            return result;
        }

        /// <summary>
        /// Returns all variables in this source that have the specified content type. If strict is false,
        /// variables whose content types are assignable to the specified content type will also be returned. 
        /// If true, only variables whose content types are <i>exactly</i> the one passed will be returned.
        /// </summary>
        public virtual IList<Muscariable> GetVarsByContentType<TContent>(bool strict = false)
        {
            var result = GetVarsByContentType(typeof(TContent), strict);
            return result;
        }

        public virtual IList<Muscariable> GetVarsByContentType(Type contentType, bool strict = false)
        {
            var result = _varManager.GetMultiVariablesOfContentType(contentType, strict);
            return result.OfType<Muscariable>().ToList();
        }

        /// <summary>
        /// Returns all variables in this source that are of the specified type. If strict is false,
        /// variables whose types are assignable to the specified type will also be returned. If 
        /// strict is true, only variables whose types are exactly the one passed will be returned.
        /// 
        /// If you want a list of vars of a specified <i>content</i> type, 
        /// use GetVarsByContentType instead.
        /// 
        /// </summary>
        public virtual IList<Muscariable> GetVarsByType<TVar>(bool strict = false) where TVar : Muscariable
        {
            return GetVarsByType(typeof(TVar));
        }

        public virtual IList<Muscariable> GetVarsByType(Type varType)
        {
            IList<Muscariable> result = _varManager.GetMultiVariablesOfType(varType)
                .OfType<Muscariable>()
                .ToList();
            return result;
        }

        public void ReorderVariables(IList<IVariable> newOrder)
        {
            if (newOrder == null || newOrder.Count == 0) return;

            IList<IVariable> whatWeGot = Variables.ToList();
            bool sameContents = whatWeGot.SameContentsAs(newOrder);
            if (!sameContents)
            {
                Debug.LogWarning($"VariableSource: ReorderVariables called with a list that " +
                    "doesn't contain the same elements as this source.", this);
                return;
            }
            else
            {
                _varManager.ReorderVariables(newOrder);
            }
        }

        public virtual void RemoveVariable(string key, StringComparison strCompare = StringComparison.Ordinal)
        {
            _varManager.RemoveVariable(key, strCompare);
        }

        public virtual void RemoveVariable(IVariable variable)
        {
            if (variable is not Muscariable muscari)
            {
                string logMessage = $"Cannot remove {variable} (a non-Muscariable) from a VariableSource asset; " +
                    $"it can't hold that in the first place.";
                Debug.LogWarning(logMessage, this);
                return;
            }

            _varManager.RemoveVariable(variable);
        }

        public event Action<IVariable> VariableRemoved
        {
            add
            {
                _varManager.VariableRemoved += value;
            }
            remove
            {
                _varManager.VariableRemoved -= value;
            }
        }

        public virtual void Refresh()
        {
            EnsureValidUniqueId();

            _varManager.VarOwner = this;
            _varManager.Refresh();
        }

        public event Action Refreshed
        {
            add
            {
                _varManager.Refreshed += value;
            }
            remove
            {
                _varManager.Refreshed -= value;
            }
        }

        public Muscariable AddVariable(Muscariable toAdd)
        {
            // The manager will handle the post-signaling and the actual addition,
            // so we don't need to do either here.
            var result = _varManager.AddVariable(toAdd);
            return result;
        }

        public void RemoveVariable(Muscariable toRemove)
        {
            _varManager.RemoveVariable(toRemove);
        }

        protected virtual void OnEnable()
        {
            ToggleSubs(false);
            ToggleSubs(true);
            _varManager.VarOwner = this;
#if UNITY_EDITOR
            if (!AssetDatabase.Contains(this))
            {
                // We don't want to assign IDs to non-assets. At least, not necessarily right when they're created.
                return;
            }
#endif
            EnsureValidUniqueId();
            VsaSignals.VsaEnabled(this);
        }

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                _varManager.VariableAdded += OnPostVarAdded;
                _varManager.VariableRemoved += OnPostVarRemoved;
                _varManager.PreVariableAdded += OnPreVarAdded;
                _varManager.PreVariableRemoved += OnPreVarRemoved;
            }
            else
            {
                _varManager.VariableAdded -= OnPostVarAdded;
                _varManager.VariableRemoved -= OnPostVarRemoved;
                _varManager.PreVariableAdded -= OnPreVarAdded;
                _varManager.PreVariableRemoved -= OnPreVarRemoved;
            }

            EditorToggleSubs(on);
        }

        private void OnPostVarAdded(IVariable variable)
        {
            VsaSignals.VariableAdded(this, variable);
        }

        private void OnPostVarRemoved(IVariable variable)
        {
            VsaSignals.VariableRemoved(this, variable);
        }

        private void OnPreVarAdded(IVariable variable)
        {
            VsaSignals.PreVariableAdded(this, variable);
        }

        private void OnPreVarRemoved(IVariable variable)
        {
            VsaSignals.PreVariableRemoved(this, variable);
        }

        protected virtual void EditorToggleSubs(bool on)
        {
#if UNITY_EDITOR
            if (on)
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }
            else
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
#endif
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            // We want to make sure that the variables' states are returned to their 
            // pre-enter-play-mode values when we exit play mode. Thus,
            // we need to set up backups.
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                ReadyBackups();
                void ReadyBackups()
                {
                    var varsToReady = Variables;
                    // ^Caching here to avoid multiple calls to the property, which would cause
                    // needless allocations and iterations.
                    for (int i = 0; i < varsToReady.Count; i++)
                    {
                        var currentVar = varsToReady[i];
                        string key = $"{_uniqueId}_{currentVar.ItemId}";
                        string valueAsJson = EditorJsonUtility.ToJson(currentVar);
                        PlayerPrefs.SetString(key, valueAsJson);
                    }
                }

            }
            else if (change == PlayModeStateChange.ExitingPlayMode)//
            {
                RestoreFromBackups();
                void RestoreFromBackups()
                {
                    var varsToRestore = Variables;
                    for (int i = 0; i < varsToRestore.Count; i++)//
                    {
                        var currentVar = varsToRestore[i];
                        string key = $"{_uniqueId}_{currentVar.ItemId}";
                        if (PlayerPrefs.HasKey(key))
                        {
                            string valueAsJson = PlayerPrefs.GetString(key);
                            EditorJsonUtility.FromJsonOverwrite(valueAsJson, currentVar);

                            PlayerPrefs.DeleteKey(key);
                        }
                    }
                }
            }
        }

#endif

        protected virtual void EnsureValidUniqueId()
        {
            bool thisIsTestOnly = SceneManager.GetActiveScene().name.StartsWith("InitTestScene");
            if (thisIsTestOnly)
            {
                return;
            }

            if (string.IsNullOrEmpty(_uniqueId))
            {
                _uniqueId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
            VsaSignals.VsaDisabled(this);
        }

        protected virtual void OnValidate()
        {
            if (!AssetDatabase.Contains(this))
            {
                // We don't want to assign IDs to non-assets. At least, not necessarily right when they're created.
                return;
            }

            EnsureValidUniqueId();
        }

        public bool Contains(IVariable var)
        {
            return _varManager.Contains(var);
        }

        T IVariableSource.GetVariableOfType<T>()
        {
            var result = _varManager.GetVariableOfType<T>();
            return result;
        }

        IVariable IVariableSource.GetVariable(string name, StringComparison strCompare)
        {
            return GetVariableByName(name, strCompare);
        }

        T IVariableSource.GetVariableOfType<T>(string name, StringComparison strCompare)
        {
            var result = _varManager.GetVariableOfType<T>(name, strCompare);
            return result;
        }

        public IVariable GetVariableOfType(Type type, string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            var result = _varManager.GetVariableOfType(type, name, strCompare);
            return result;
        }

#if UNITY_EDITOR

        public void MigrateToVariableManager()
        {
            if (variables.Count == 0)
            {
                Debug.Log($"{this.name} has no variables to migrate.", this);
                return;
            }

            _varManager ??= new VariableManager();
            _varManager.Initialize(variables, new List<Variable>());
            _varManager.VarOwner = this;
            variables.Clear();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            AssetDatabase.Refresh();
        }

        Muscariable IMuscariableSource.AddNewVariableOfContentType<TContentType>(string k, TContentType defaultVal, VariableScope scope)
        {
            return ((IMuscariableSource)_varManager).AddNewVariableOfContentType(k, defaultVal, scope);
        }

        public Muscariable AddNewVariableOfContentType(Type contentType, string key)
        {
            return ((IMuscariableSource)_varManager).AddNewVariableOfContentType(contentType, key);
        }

#endif
    }

    public interface IForceResetUidHandler
    {
        void ForceResetUid();
    }

    public interface IVarConvertible<TTargetType> where TTargetType : IVariable
    {
        TTargetType ToVar();
    }

    
}