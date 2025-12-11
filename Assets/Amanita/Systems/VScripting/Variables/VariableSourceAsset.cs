using Amanita.SaveSys;
using Collections;
using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Type = System.Type;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Amanita.VScripting
{
    [CreateAssetMenu(fileName = "NewVariableSourceAsset", menuName = "Amanita/VariableSource")]
    public class VariableSourceAsset : ScriptableObject, IReorderableMuscariableSource, IForceResetUidHandler
    {
        [SerializeField] protected bool includeInSaves = true;
        [SerializeField, HideInInspector] protected string uniqueId = string.Empty;
        [SerializeReference] protected List<Muscariable> variables = new List<Muscariable>();

        public virtual void ForceResetUid()
        {
            UniqueId = Guid.NewGuid().ToString();
        }

        public bool IncludeInSaves
        {
            get => includeInSaves;
            set => includeInSaves = value;
        }

        public string UniqueId
        {
            get => uniqueId;
            set
            {
                if (!string.IsNullOrEmpty(uniqueId))
                {
                    Debug.LogWarning("Warning: Overwriting existing AssetId on VariableSourceAsset.");
                }

                string prevId = uniqueId;
                uniqueId = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
                AssetDatabase.Refresh();
#endif
            }
        }

        // Always return a list, even if the backing field was deserialized as null.
        public IReadOnlyList<IVariable> Variables
        {
            get
            {
                EnsureVariablesList();
                return variables.Cast<IVariable>().ToList();
            }
        }

        IReadOnlyList<Muscariable> IVariableSource<Muscariable>.Variables
        {
            get
            {
                EnsureVariablesList();
                return variables.ToList();
            }
        }

        /// <summary>
        /// Creates and returns a new Muscariable of the content type,
        /// assigning it the passed key and starting value.
        /// </summary>
        public virtual Muscariable<TContent> AddNewVariableOfContentType<TContent>(string key,
            TContent startingVal = default)
        {
            Muscariable<TContent> result = (Muscariable<TContent>)AddNewVariableOfContentType(typeof(TContent), key);
            result.Value = startingVal;
            return result; 
        }

        public virtual Muscariable AddNewVariableOfContentType(Type contentType, string key)
        {
            EnsureVariablesList();
            Muscariable var = VariableFactory.CreateByContentType(contentType, null);
            var.Key = key;
            AddVariable(var);
            return var;
        }

        /// <summary>
        /// If the var is a legacy one, it will be converted to a Muscariable. Returns
        /// the variable added.
        /// </summary>
        public virtual IVariable AddVariable(IVariable var)
        {
            EnsureVariablesList();
            Muscariable muscari = var.ToMuscariable();
            if (muscari == null)
            {
                string logMessage = $"Cannot add {var} (a non-Muscariable) to a VariableSource asset; " +
                    $"it can't hold that in the first place.";
                Debug.LogWarning(logMessage);
                return null;
            }
            else
            {
                return AddVariable(muscari);
            }
        }

#if UNITY_EDITOR
        // We only want editor code to respond to these events.
        public static event Action<Muscariable> AnyRightBeforeVarAdded = delegate { };
        public static event Action<Muscariable> AnyRightBeforeVarRemoved = delegate { };
#endif

        public event Action VariablesReordered = delegate { };

        // So that we can avoid what (at least look like) duplicates
        protected virtual void MakeUniqueForThisSource(Muscariable var)
        {
            EnsureVariablesList();
            var.Key = UniqueKeyGenerator.GetUniqueKeyFor(var.Key, variables.Cast<IVariable>().ToList(), var);
            IList<IHasItemID> toPass = variables.OfType<IHasItemID>().ToList();
            var.ItemId = _nextVarID;
            _nextVarID++;
            var.Owner = this;
        }

        [SerializeField, HideInInspector] protected byte _nextVarID = 1;
        public event Action<IVariable> VariableAdded = delegate { };

        public Muscariable GetVariable(string name)
        {
            EnsureVariablesList();
            for (int i = 0; i < variables.Count; i++)
            {
                Muscariable var = variables[i];
                if (var.Key == name)
                {
                    return var;
                }
            }

            return null;
        }

        public virtual IVariable GetVariable(byte itemID)
        {
            EnsureVariablesList();
            IVariable result = variables.Where((elem) => elem.ItemId == itemID).FirstOrDefault();
            return result;
        }

        public virtual IList<Muscariable> GetVarsByContentType<TContent>()
        {
            return GetVarsByContentType(typeof(TContent));
        }

        public virtual IList<Muscariable> GetVarsByContentType(Type contentType)
        {
            EnsureVariablesList();
            IList<Muscariable> result = variables.Where(VarIsOfContentType).ToList();

            bool VarIsOfContentType(Muscariable elem)
            {
                return elem.ContentType.IsAssignableFrom(contentType);
            }

            return result;
        }

        public virtual IList<Muscariable> GetVarsByType<TVar>() where TVar : Muscariable
        {
            return GetVarsByType(typeof(TVar));
        }

        public virtual IList<Muscariable> GetVarsByType(Type varType)
        {
            EnsureVariablesList();
            IList<Muscariable> result = variables.Where((elem) => varType.IsAssignableFrom(elem.GetType())).ToList();
            return result;
        }

        public void ReorderVariables(IList<IVariable> newOrder)
        {
            EnsureVariablesList();
            if (newOrder == null || newOrder.Count == 0) return;

            IList<Muscariable> toCompareTo = newOrder.OfType<Muscariable>().ToList();
            if (variables.SameContentsAs(toCompareTo) == false)
            {
                Debug.LogWarning("VariableSource: ReorderVariables called with a list that doesn't contain the same elements as this source.");
                return;
            }
            else
            {
                variables.Clear();
                variables.AddRange(toCompareTo);

                VariablesReordered();

            }
        }

        public virtual void RemoveVariable(string key)
        {
            EnsureVariablesList();
            IVariable toRemove = variables.Find(elem => elem.Key == key);
            RemoveVariable(toRemove);
        }

        public virtual void RemoveVariable(IVariable variable)
        {
            EnsureVariablesList();
            if (variable is not Muscariable muscari)
            {
                string logMessage = $"Cannot remove {variable} (a non-Muscariable) from a VariableSource asset; " +
                    $"it can't hold that in the first place.";
                Debug.LogWarning(logMessage);
                return;
            }

            AnyRightBeforeVarRemoved(muscari);
            // For the sake of Undo/Redo, we'd best NOT unregister ourselves as the owner.
            // Even if it'd be sorta misleading...//
            variables.Remove(muscari);
            VariableRemoved(muscari);
        }

        public event Action<IVariable> VariableRemoved = delegate { };

        public virtual void Refresh()
        {
            EnsureVariablesList();
            EnsureValidUniqueId();

            variables.RemoveAll(elem => elem == null);

            AssertOwnership();
            void AssertOwnership()
            {
                foreach (var elem in variables)
                {
                    elem.Owner = this;
                }
            }

            Refreshed();
        }

        public event Action Refreshed = delegate { };

        public Muscariable AddVariable(Muscariable toAdd)
        {
            EnsureVariablesList();
            if (!variables.ContainsReference(toAdd))
            {
                MakeUniqueForThisSource(toAdd);
                _nextVarID = (byte)(toAdd.ItemId + 1);
#if UNITY_EDITOR
                AnyRightBeforeVarAdded(toAdd);
#endif
                variables.Add(toAdd);
                VariableAdded(toAdd);
            }

            return toAdd;
        }

        public void RemoveVariable(Muscariable toRemove)
        {
            throw new NotImplementedException();
        }

        Muscariable IVariableSource<Muscariable>.GetVar(int itemId)
        {
            EnsureVariablesList();
            for (int i = 0; i < variables.Count; i++)
            {
                Muscariable var = variables[i];
                if (var.ItemId == itemId)
                {
                    return var;
                }
            }

            return null;
        }

        protected virtual void OnEnable()
        {
            EnsureVariablesList();
#if UNITY_EDITOR
            if (!AssetDatabase.Contains(this))
            {
                // We don't want to assign IDs to non-assets. At least, not necessarily right when they're created.
                return;
            }
#endif
            EnsureValidUniqueId();
            EnsureValidVarIDs();
            EditorOnEnable();
        }

        protected virtual void EnsureValidUniqueId()
        {
            bool thisIsTestOnly = SceneManager.GetActiveScene().name.StartsWith("InitTestScene");
            if (thisIsTestOnly)
            {
                return;
            }

            if (string.IsNullOrEmpty(uniqueId))
            {
                uniqueId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        protected virtual void EnsureValidVarIDs()
        {
            EnsureVariablesList();
            HashSet<int> usedIDs = new HashSet<int>();
            foreach (var var in variables)
            {
                if (var.ItemId == 0 || usedIDs.Contains(var.ItemId))
                {
                    var.ItemId = _nextVarID;
                    _nextVarID++;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                }
                usedIDs.Add(var.ItemId);
            }
            
        }

        protected virtual void EditorOnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            // We want to make sure that the variables' states are returned to their 
            // pre-enter-play-mode values when we exit play mode. Thus,
            // we need to set up backups when entering play mode.
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                ReadyBackups();
                void ReadyBackups()
                {                     
                    EnsureVariablesList();
                    backupMuscariables.Clear();
                    foreach (var var in variables)
                    {
                        Muscariable backupVar = var.Clone();
                        backupMuscariables.Add(backupVar);
                    }
                }
                
            }
            else if (change == PlayModeStateChange.ExitingPlayMode)
            {
                RestoreFromBackups();
                void RestoreFromBackups()
                {
                    EnsureVariablesList();
                    // Rather than recreating the vars as "restored" ones, we apply the values
                    // of the backups to the ones we got.
                    for (int i = 0; i < backupMuscariables.Count; i++)
                    {
                        Muscariable backupVar = backupMuscariables[i];
                        Muscariable varToRestoreTo = variables.Where((elem) => elem.ItemId == backupVar.ItemId)
                            .FirstOrDefault();
                        if (varToRestoreTo != null)
                        {
                            varToRestoreTo.BoxedValue = backupVar.BoxedValue;
                        }
                        else
                        {
                            Debug.LogError($"Could not find variable with ID {backupVar.ItemId} to restore its value to.");
                        }
                    }
                }
            }
        }

        protected List<Muscariable> backupMuscariables = new List<Muscariable>();
#endif

        protected virtual void OnDisable()
        {
            EditorOnDisable();
        }

        protected virtual void EditorOnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

        protected virtual void OnValidate()
        {
            EnsureVariablesList();
            if (!AssetDatabase.Contains(this))//
            {
                // We don't want to assign IDs to non-assets. At least, not necessarily right when they're created.
                return;
            }

            //variables.RemoveAll(elem => elem == null);

            EnsureValidUniqueId();
            EnsureValidVarIDs();
        }

        // Centralized guard to materialize the list if it was deserialized as null. Will
        // use this a lot to compensate for Unity's serialization quirks.
        protected void EnsureVariablesList()
        {
            if (variables == null)
            {
                variables = new List<Muscariable>();
#if UNITY_EDITOR
                if (AssetDatabase.Contains(this))
                {
                    EditorUtility.SetDirty(this);
                }
#endif
            }
        }

        public bool Contains(IVariable var)
        {
            EnsureVariablesList();
            return variables.ContainsReference(var);
        }
    }

    public interface IVariableSource : IHasUniqueID
    {
        event Action<IVariable> VariableAdded;
        event Action<IVariable> VariableRemoved;
        IReadOnlyList<IVariable> Variables { get; }
        IVariable AddVariable(IVariable toAdd);
        void RemoveVariable(IVariable toRemove);
        IVariable GetVariable(byte itemId);
        bool Contains(IVariable var);
    }

    public interface IHasUniqueID
    {
        string UniqueId { get; }
    }

    public interface IForceResetUidHandler
    {
        void ForceResetUid();
    }

    public interface IVariableSource<TVar> : IVariableSource where TVar: IVariable
    {
        new IReadOnlyList<TVar> Variables { get; }
        TVar AddVariable(TVar toAdd);
        void RemoveVariable(TVar toRemove);
        TVar GetVar(int itemId);
    }

    public interface IMuscariableSource : IVariableSource<Muscariable>
    {
        Muscariable GetVariable(string name);
        Muscariable AddNewVariableOfContentType(Type contentType, string key);
    }

    public interface IReorderableVariableSource : IVariableSource
    {
        void ReorderVariables(IList<IVariable> newlyOrderedVars);
    }

    public interface IReorderableMuscariableSource : IReorderableVariableSource, IMuscariableSource
    {
        
    }

    public interface IVarConvertible<TTargetType> where TTargetType : IVariable
    {
        TTargetType ToVar();
    }

    public class VSAConverter : fsDirectConverter<VariableSourceAsset>
    {
        protected override fsResult DoSerialize(VariableSourceAsset model, Dictionary<string, fsData> serialized)
        {
            VariableSourceAssetSaveData saveData = new VariableSourceAssetSaveData();
            saveData.UniqueId = model.UniqueId;
            saveData.SavedVars = (IList<VariableSaveData>)model.Variables;
            SerializeMember(serialized, null, "saveData", saveData);
            return fsResult.Success;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref VariableSourceAsset model)
        {
            // We assume that the data contains a VariableSourceAssetSaveData under "saveData".
            fsData saveDataData;
            if (data.TryGetValue("saveData", out saveDataData))
            {
                fsResult result;
                VariableSourceAssetSaveData saveData = null;
                result = DeserializeMember(data, null, "saveData", out saveData);
                if (result.Failed)
                {
                    return result;
                }
                // Now, we can reconstruct the VariableSourceAsset from the save data.
                model = ScriptableObject.CreateInstance<VariableSourceAsset>();
                model.IncludeInSaves = true;
                model.Refresh();
                
                model.UniqueId = saveData.UniqueId;
                foreach (var varSave in saveData.SavedVars)
                {
                    Muscariable var = VariableFactory.CreateByVarTypeName(varSave.VarTypeName, null);
                    model.AddVariable(var);
                }
                return fsResult.Success;
            }
            else
            {
                return fsResult.Fail("No 'saveData' found in data for VariableSourceAsset deserialization.");
            }
        }
    }
}