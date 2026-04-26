using AtMycelia.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [Serializable]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public sealed class VariableManager : IVariableSource, IMuscariableSource,
        IReorderableVariableSource, IReorderableMuscariableSource
    {
        // Note: Unity does not serialize readonly fields, even if they're plain 
        // old Lists of types it otherwise serializes just fine. So, we have
        // to make these non-readonly and just be careful not to reassign them.
        [FormerlySerializedAs("muscariables")]
        [SerializeReference] private List<Muscariable> _muscariables = new();
        [FormerlySerializedAs("legacyVariables")]
        [SerializeField] private List<Variable> _legacyVariables = new();
        [FormerlySerializedAs("nextValidVarID")]
        [SerializeField] private byte _nextValidVarID = 1;
        [FormerlySerializedAs("isInitted")]
        [SerializeField] private bool _isInitted = false;

        public void Initialize()
        {
            if (IsInitted)
            {
                Debug.LogWarning($"VariableManager for {Name} is already initialized. Reinitializing will clear " +
                    "all variables and reset the manager. Proceeding with reinitialization.");
            }
            Clear();
            _nextValidVarID = 1;
            IsInitted = true;
        }

        public bool IsInitted
        {
            get => _isInitted;
            private set => _isInitted = value;
        }

        public void Clear()
        {
            // Remove them one by one so the right events fire
            while (_legacyVariables.Count > 0)
            {
                RemoveLegacyVarAtIndex(0);
            }

            while (_muscariables.Count > 0)
            {
                RemoveMuscariAtIndex(0);
            }
        }

        public IVariable RemoveLegacyVarAtIndex(int index)
        {
            if (index < 0 || index >= _legacyVariables.Count)
            {
                string errorMessage = $"Index {index} is out of range for legacy variables. Valid range is " +
                    $"0 to {_legacyVariables.Count - 1}. No variable removed.";

                throw new IndexOutOfRangeException(errorMessage);
            }

            Variable toRemove = _legacyVariables[index];
            RemoveFromCachesThenSignal(toRemove);
            return toRemove;
        }

        private void RemoveFromCachesThenSignal(IVariable toRemove)
        {
            PreVariableRemoved(toRemove);
            _legacyVariables.RemoveByReference(toRemove as Variable);
            _muscariables.RemoveByReference(toRemove as Muscariable);

            _lookup.Remove(toRemove.ItemId);
            MarkOwnerAsDirty();
            VariableRemoved(toRemove);
            VariableSignals.VariableRemoved(toRemove);
        }

        private Dictionary<byte, IVariable> _lookup = new();
        // ^For faster retrieval by ID. Must be kept in sync with the lists.
        public event Action<IVariable> PreVariableRemoved = delegate { };

        public event Action<IVariable> VariableRemoved = delegate { };

        public IVariable RemoveMuscariAtIndex(int index)
        {
            if (index < 0 || index >= _muscariables.Count)
            {
                string errorMessage = $"Index {index} is out of range for muscariables. Valid range is " +
                    $"0 to {_muscariables.Count - 1}. No variable removed.";
                throw new IndexOutOfRangeException(errorMessage);
            }
            Muscariable toRemove = _muscariables[index];
            RemoveFromCachesThenSignal(toRemove);
            return toRemove;
        }

        public void Initialize(IList<Muscariable> initMuscaris, IList<Variable> initLegacies)
        {
            Initialize();
            AddMultiVars(initMuscaris);

            // For the sake of backwards compatibility with Fungus projects, we won't convert 
            // any legacies we're being initialized with. At least, not here.
            for (int i = 0; i < initLegacies.Count; i++)
            {
                Variable legacy = initLegacies[i];
                _legacyVariables.Add(legacy);
                RegisterIntoVarLookup(new[] { legacy });
            }

            UpdateNextValidId();
            void UpdateNextValidId()
            {
                // We want it set to one more than the max ID currently in use, so that the next
                // variable added will get an ID that is not already taken.
                byte maxIdInUse = 0;
                foreach (var elem in _lookup.Values)
                {
                    if (elem.ItemId > maxIdInUse)
                    {
                        maxIdInUse = elem.ItemId;
                    }
                }
                _nextValidVarID = (byte)(maxIdInUse + 1);
            }

            EnsureValidIds();
        }

        public void AddMultiVars(IEnumerable<IVariable> toAdd)
        {
            EnsureInitialized();
            foreach (var elem in toAdd)
            {
                AddVariable(elem);
            }
        }

#if UNITY_EDITOR
        public void MigrateLegacyVariables(IList<Muscariable> oldMuscariables, IList<Variable> oldLegacyVariables)
        {
            EnsureInitialized();//

            bool addedAny = false;

            if (oldMuscariables != null)
            {
                for (int i = 0; i < oldMuscariables.Count; i++)
                {
                    var muscariable = oldMuscariables[i];
                    if (muscariable == null || IsRegistered(muscariable))
                    {
                        continue;
                    }
                    _muscariables.Add(muscariable);
                    addedAny = true;
                }
            }

            if (oldLegacyVariables != null)
            {
                for (int i = 0; i < oldLegacyVariables.Count; i++)
                {
                    var legacyVar = oldLegacyVariables[i];
                    if (legacyVar == null || IsRegistered(legacyVar))
                    {
                        continue;
                    }

                    if (legacyVar.ItemId == Muscariable.InvalidId || _lookup.ContainsKey(legacyVar.ItemId))
                    {
                        legacyVar.ItemId = NextValidVarID();
                    }

                    _legacyVariables.Add(legacyVar);
                    _lookup[legacyVar.ItemId] = legacyVar;
                    addedAny = true;
                }
            }

            if (addedAny)
            {
                Refresh();
            }
        }

        private bool IsRegistered(IVariable variable)
        {
            if (variable == null)
            {
                return false;
            }

            foreach (var existing in _lookup.Values)
            {
                if (ReferenceEquals(existing, variable))
                {
                    return true;
                }
            }

            return false;
        }
#endif

        /// <summary>
        /// Adds a variable to the manager before returning it. If the variable is already registered, it 
        /// will not be added again. If the variable is a legacy var, a Muscariable version of it will 
        /// be created, added, and returned instead.
        /// </summary>
        public IVariable AddVariable(IVariable toAdd)
        {
            EnsureInitialized();
            Muscariable result = AddAsMuscari(toAdd);
            return result;
        }

        /// <summary>
        /// Adds a variable to the manager, converting it to a Muscari beforehand as appropriate. 
        /// Returns the Muscariable that was added, or null if the variable was already registered 
        /// and thus not added.
        /// </summary>
        public Muscariable AddAsMuscari(IVariable toAdd)
        {
            EnsureInitialized();
            bool alreadyRegistered = _legacyVariables.ContainsReference(toAdd) ||
                _muscariables.ContainsReference(toAdd);
            if (alreadyRegistered)
            {
                return null;
            }

            Muscariable muscari = toAdd.ToMuscariable();
            Integrate(muscari);
            return muscari;
        }


        /// <summary>
        /// Adds the given Muscariable to the caches, ensuring it has a valid ID and key, 
        /// and setting its owner and parent flowchart references. Also sends the signal
        /// for var-adding.
        /// </summary>
        void Integrate(Muscariable toAdd)
        {
            UpdateNextValidId();
            #region Ensure valid id and key
            bool duplicateId = _lookup.ContainsKey(toAdd.ItemId);
            if (toAdd.ItemId == Muscariable.InvalidId)
            {
                toAdd.ItemId = NextValidVarID();
            }
            else if (duplicateId)
            {
                Debug.LogWarning($"Duplicate variable ID {toAdd.ItemId} found for {_varOwner?.Name}. Reassigning.");
                toAdd.ItemId = NextValidVarID();
            }

            toAdd.Key = UniqueKeyGenerator.GetUniqueKeyFor(toAdd.Key, (IList<IVariable>)Variables, null);
            #endregion

            #region Establish ownership and parent flowchart references
            toAdd.ParentFlowchart = VarOwner as Flowchart;
            toAdd.Owner = _varOwner;
            #endregion

            AddToCachesThenSignal(toAdd);
        }

        private void AddToCachesThenSignal(IVariable toAdd)
        {
            PreVariableAdded(toAdd);
            if (toAdd is Muscariable)
            {
                _muscariables.Add(toAdd as Muscariable);
            }
            else if (toAdd is Variable)
            {
                _legacyVariables.Add(toAdd as Variable);
            }
            _lookup[toAdd.ItemId] = toAdd;
            MarkOwnerAsDirty();
            VariableAdded(toAdd);
            VariableSignals.VariableAdded(toAdd);
        }

        public event Action<IVariable> PreVariableAdded = delegate { };
        public event Action<IVariable> VariableAdded = delegate { };

        /// <summary>
        /// Meant to be called through Unity's OnEnable message. This function ensures that 
        /// all variables have valid IDs, and initializes them with their start values if 
        /// the application is playing. It also registers the manager with the 
        /// SceneObjectReferenceRestorer so that it can restore references for this manager 
        /// when scenes are loaded. This is important because if the manager is disabled, 
        /// it may be in a state where it can't properly restore references 
        /// (for example, if it's been destroyed but not yet removed from the scene), 
        /// and trying to do so could cause errors.
        /// </summary>
        public void OnEnable()
        {
            if (VarOwner is UnityObj ownerUnityObj)
            {
                EnsureValidIds();
                foreach (var elem in _lookup.Values)
                {
                    elem.Init(elem.BoxedValue);
                }
            }
            Refresh();
        }

        /// <summary>
        /// Meant to be called through Unity's OnDisable message. This function unregisters the 
        /// manager from the SceneObjectReferenceRestorer so that it won't try to restore 
        /// references for this manager while it's disabled. This is important because if 
        /// the manager is disabled, it may be in a state where it can't properly restore 
        /// references (for example, if it's been destroyed but not yet removed from the 
        /// scene), and trying to do so could cause errors.
        /// </summary>
        public void OnDisable()
        {
            // No-op for now, but we might want to add some cleanup logic here in the
            // future, and if we do, this is where it should go.
        }

        public void Refresh()
        {
            RemoveAllNulls();
            _lookup ??= new Dictionary<byte, IVariable>();
            _lookup.Clear();
            RegisterIntoVarLookup(_muscariables);
            RegisterIntoVarLookup(_legacyVariables);
            EnsureValidIds();
            Refreshed();
        }

        private void RemoveAllNulls()
        {
            _muscariables.RemoveAll(var => var == null);
            _legacyVariables.RemoveAll(var => var == null);
        }

        public event Action Refreshed = delegate { };

        private void RegisterIntoVarLookup(IEnumerable<IVariable> varsToRegister)
        {
            foreach (var elem in varsToRegister)
            {
                if (elem.ItemId == Muscariable.InvalidId)
                {
                    elem.ItemId = NextValidVarID();
                }
                else if (_lookup.ContainsKey(elem.ItemId))
                {
                    Debug.LogWarning($"Duplicate variable ID {elem.ItemId} found for {_varOwner?.Name}. Reassigning.");
                    elem.ItemId = NextValidVarID();
                }
                _lookup[elem.ItemId] = elem;
            }
        }

        /// <summary>
        /// Checks for duplicate IDs and reassigns them if necessary
        /// </summary>
        public void EnsureValidIds()
        {
            var idGroups = _lookup.Values.GroupBy(elem => elem.ItemId);
            foreach (var group in idGroups)
            {
                if (group.Count() > 1)
                {
                    Debug.LogWarning($"Duplicate variable ID {group.Key} found for {_varOwner?.Name}. Reassigning IDs.");
                    foreach (var elem in group)
                    {
                        elem.ItemId = NextValidVarID();
                    }
                }
            }

            // Find the vars that have an itemId of 0, then reassign them valid IDs. We have to do
            // this separately from the duplicate ID check because 0 is a valid byte value, so it
            // won't be caught by the duplicate ID check even though it's not a valid ID for our purposes.
            var zeroIdVars = _lookup.Values.Where(elem => elem.ItemId == Muscariable.InvalidId).ToList();
            foreach (var elem in zeroIdVars)
            {
                elem.ItemId = NextValidVarID();
            }
        }

        private byte NextValidVarID()
        {
            byte toReturn = _nextValidVarID;
            _nextValidVarID++;
            return toReturn;
        }

        public IReadOnlyList<IVariable> Variables
        {
            get
            {
                var result = new List<IVariable>(_muscariables.Count + _legacyVariables.Count);
                result.AddRange(_muscariables);
                result.AddRange(_legacyVariables);
                return result;
            }
        }

        public IVariableSource VarOwner
        {
            get
            {
                _varOwner ??= this;
                return _varOwner;
            }
            set
            {
                if (!ReferenceEquals(_varOwner, value))
                {
                    _varOwner = value;
                    _varOwner ??= this;

                    foreach (var elem in _lookup.Values)
                    {
                        if (elem is not Variable)
                        {
                            elem.Owner = _varOwner;
                        }
                    }
                }
            }
        }

        private IVariableSource _varOwner;

        public void RemoveVariable(IVariable toRemove)
        {
            bool alreadyRegistered = _lookup.Values.Contains(toRemove);
            if (!alreadyRegistered)
            {
                return;
            }

            RemoveFromCachesThenSignal(toRemove);
        }

        public void RemoveVariable(string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            var toRemove = GetVariable(name, strCompare);
            if (toRemove != null)
            {
                RemoveVariable(toRemove);
            }
        }

        public IVariable GetVariable(byte id)
        {
            if (_lookup == null || _lookup.Count == 0)
            {
                Refresh();
            }

            _lookup.TryGetValue(id, out IVariable result);
            return result;
        }

        public bool Contains(IVariable var)
        {
            return _lookup.TryGetValue(var.ItemId, out IVariable found) && found == var;
        }

        public void ResetAll()
        {
            foreach (var variable in _lookup.Values)
            {
                variable.OnReset();
            }
        }


        /// <summary>
        /// Gets a variable by name, returning it as the specified generic type if it is of that type. Null otherwise.
        /// </summary>
        public IVariable<TContent> GetVariable<TContent>(string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            var result = _lookup.Values.FirstOrDefault(var => var.Key.Equals(name, strCompare));
            return result as IVariable<TContent>;
        }

        public Muscariable AddNewVariableOfContentType<T>(string key, T defaultValue, 
            VariableScope scope = VariableScope.Private)
        {
            return AddNewVariableOfContentType(typeof(T), key, defaultValue, scope);
        }

        public Muscariable AddNewVariableOfContentType(Type contentType, string key, 
            object defaultValue, VariableScope scope = VariableScope.Private)
        {
            EnsureInitialized();
            Muscariable muscaVar = VariableFactory.CreateByContentType(contentType, null);
            muscaVar.BoxedValue = defaultValue;
            muscaVar.Scope = scope;
            Integrate(muscaVar);
            return muscaVar;
        }

        public Muscariable AddVariable(Muscariable toAdd)
        {
            EnsureInitialized();
            return AddAsMuscari(toAdd);
        }

        public void RemoveVariable(Muscariable toRemove)
        {
            RemoveVariable(toRemove as IVariable);
        }


        public T GetVariable<T>(byte itemId) where T : class, IVariable
        {
            _lookup.TryGetValue(itemId, out IVariable found);
            T result = found as T;
            return result;
        }

        /// <summary>
        /// Returns a list of the variables this manager has that are of the specified variable
        /// type. If you just want to get variables of a certain content type, use 
        /// GetMultiVariablesOfContentType instead.
        /// </summary>
        public IList<T> GetMultiVariablesOfType<T>(bool strict = false) where T : IVariable
        {
            var result = GetMultiVariablesOfType(typeof(T), strict)
                .OfType<T>()
                .ToList();
            return result;
        }

        public IList<IVariable> GetMultiVariablesOfType(Type varType, bool strict = false)
        {
            var result = _lookup.Values.Where(IsMatch).ToList();
            bool IsMatch(IVariable var)
            {
                if (strict)
                {
                    return var.GetType() == varType;
                }
                else
                {
                    return varType.IsAssignableFrom(var.GetType());
                }
            }
            return result;
        }

        public IList<T> GetMultiVariablesOfContentType<T>()
        {
            var result = GetMultiVariablesOfContentType(typeof(T)).OfType<T>().ToList();
            return result;
        }

        public IList<IVariable> GetMultiVariablesOfContentType(Type contentType, bool strict = false)
        {
            return _lookup.Values.Where(IsMatch).ToList();

            bool IsMatch(IVariable var)
            {
                if (strict)
                {
                    return var.ContentType == contentType;
                }
                else
                {
                    return contentType.IsAssignableFrom(var.ContentType);
                }
            }
        }

        public TVarType AddNewMuscari<TValueType, TVarType>(string key = "", TValueType initValue = default,
            VariableScope scope = VariableScope.Private) where TVarType : Muscariable<TValueType>, new()
        {
            EnsureInitialized();
            TVarType result = new TVarType();
            result.Value = initValue;
            result.Scope = scope;
            result.Key = key;
            Integrate(result);
            return result;
        }

        public byte NextId { get; }

        public string UniqueId
        {
            get
            {
                if (VarOwner != this)
                {
                    return VarOwner.UniqueId;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public int VariableCount => _lookup.Count;

        IReadOnlyList<Muscariable> IVariableSource<Muscariable>.Variables => Variables.Cast<Muscariable>().ToList();

        public string Name
        {
            get => _varOwner.Name;
            set => _varOwner.Name = value;
        }

        public IVariable<TValHeld> AddNewVariable<TValHeld>(string key,
            TValHeld value = default,
            VariableScope scope = VariableScope.Private)
        {
            EnsureInitialized();
            Type valueType = typeof(TValHeld);

            IVariable<TValHeld> newVar = VariableFactory.CreateByContentType(valueType) as IVariable<TValHeld>;

            newVar.Key = UniqueKeyGenerator.GetUniqueKeyFor(key, (IList<IVariable>)Variables);
            newVar.Value = value;
            newVar.Scope = scope;
            newVar.ItemId = NextValidVarID();

            IVariable toRegister = newVar;
            AddVariable(toRegister);

            if (Application.IsPlaying(VarOwner as UnityObj))
            {
                newVar.Init(value);
            }

            VariableAdded(toRegister);

            return newVar;
        }

        /// <summary>
        /// This function exists to help make sure we don't lose our vars during any setup process (especially
        /// those in unit tests). This should be called at the beginning of any public function that modifies
        /// the variables in any way, to ensure that if we haven't been initialized yet for some reason, 
        /// we will be before we try to do anything with the vars. 
        /// 
        /// This is especially important for functions that might be called from outside the manager, since 
        /// we can't guarantee that the caller will have called Initialize() first. It's less crucial for 
        /// private functions that are only called from other functions in this class, since we can just 
        /// make sure to call EnsureInitialized() at the beginning of those public functions, but it 
        /// doesn't hurt to be extra safe.
        /// </summary>
        private void EnsureInitialized()
        {
            if (IsInitted)
            {
                return;
            }

            Refresh();
            UpdateNextValidId();
            IsInitted = true;
        }

        private void UpdateNextValidId()
        {
            byte maxIdInUse = 0;
            foreach (var elem in _lookup.Values)
            {
                if (elem.ItemId > maxIdInUse)
                {
                    maxIdInUse = elem.ItemId;
                }
            }
            _nextValidVarID = (byte)(maxIdInUse + 1);
        }


        public T GetVariableOfType<T>() where T : class, IVariable
        {
            var result = _lookup.Values.OfType<T>().FirstOrDefault();
            return result;
        }

        IVariable IVariableSource.GetVariable(string name, StringComparison strCompare)
        {
            return GetVariable(name, strCompare);
        }

        public IVariable GetVariable(string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            var result = _lookup.Values.FirstOrDefault(var => var.Key.Equals(name, strCompare));
            return result;
        }

        public T GetVariableOfType<T>(string name, StringComparison strCompare = StringComparison.Ordinal) where T : class, IVariable
        {
            return GetVariableOfType(typeof(T), name, strCompare) as T;
        }

        public IVariable GetVariableOfType(Type type, string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            IVariable result = null;
            var found = GetVariable(name, strCompare);
            if (found != null && type.IsAssignableFrom(found.GetType()))
            {
                result = found;
            }
            return result;
        }

        public void ReorderVariables(IList<IVariable> newlyOrderedVars)
        {
            if (newlyOrderedVars == null || newlyOrderedVars.Count == 0)
            {
                return;
            }

            var orderedSnapshot = newlyOrderedVars.ToList();
            var whatWeGot = _lookup.Values.ToList();
            if (!orderedSnapshot.SameContentsAs(whatWeGot))
            {
                Debug.LogWarning("Attempted to reorder variables with a list that doesn't have the same " +
                    "contents as the current variables. Reorder aborted.");
                return;
            }

            Clear();
            AddMultiVars(orderedSnapshot);
            Reordered();
        }

        public event Action Reordered = delegate { };

        public void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                foreach (var variable in _lookup.Values)
                {
                    variable.Init(variable.BoxedValue);
                    // ^To accomodate any changes that might have been made to the variables while in edit mode, since those changes won't be serialized and thus would be lost when entering play mode if we didn't do this.
                }
            }
        }


        public void RemoveAll(Predicate<IVariable> match)
        {
            var toRemove = _lookup.Values.Where(var => match(var)).ToList();
            
            foreach (var elem in toRemove)
            {
                RemoveVariable(elem);
            }
        }

        public Muscariable AddNewVariableOfContentType(Type contentType, string key)
        {
            var result = VariableFactory.CreateByContentType(contentType, null);
            result.Key = key;
            Integrate(result);
            return result;
        }

        private void MarkOwnerAsDirty()
        {
#if UNITY_EDITOR
            if (VarOwner is UnityObj unityObj)
            {
                EditorUtility.SetDirty(unityObj);
            }
#endif
        }
    }
}