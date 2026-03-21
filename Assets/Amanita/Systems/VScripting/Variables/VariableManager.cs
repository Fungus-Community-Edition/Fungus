using AtMycelia.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Amanita.VScripting
{
    [Serializable]
    public sealed class VariableManager : IVariableSource, IMuscariableSource
    {
        // Note: Unity does not serialize readonly fields, even if they're plain 
        // old Lists of types it otherwise serializes just fine. So, we have
        // to make these non-readonly and just be careful not to reassign them.
        [SerializeReference] private List<Muscariable> muscariables = new();
        [SerializeField] private List<Variable> legacyVariables = new();
        [SerializeField] private byte nextValidVarID = 1;
        [SerializeField] private bool isInitted = false;

        public void Initialize()
        {
            if (IsInitted)
            {
                Debug.LogWarning("VariableManager is already initialized. Reinitializing will clear " +
                    "all variables and reset the manager. Proceeding with reinitialization.");
            }
            Clear();
            nextValidVarID = 1;
            IsInitted = true;
        }

        public bool IsInitted
        {
            get => isInitted;
            private set => isInitted = value;
        }

        public void Clear()
        {
            // Remove them one by one so the right events fire
            while (legacyVariables.Count > 0)
            {
                RemoveLegacyVarAtIndex(0);
            }

            while (muscariables.Count > 0)
            {
                RemoveMuscariAtIndex(0);
            }
        }

        public IVariable RemoveLegacyVarAtIndex(int index)
        {
            if (index < 0 || index >= legacyVariables.Count)
            {
                string errorMessage = $"Index {index} is out of range for legacy variables. Valid range is " +
                    $"0 to {legacyVariables.Count - 1}. No variable removed.";

                throw new IndexOutOfRangeException(errorMessage);
            }

            Variable toRemove = legacyVariables[index];
            RemoveFromCachesThenSignal(toRemove);
            return toRemove;
        }

        private void RemoveFromCachesThenSignal(IVariable toRemove)
        {
            legacyVariables.RemoveByReference(toRemove as Variable);
            muscariables.RemoveByReference(toRemove as Muscariable);

            lookup.Remove(toRemove.ItemId);
            VariableRemoved(toRemove);
        }

        private Dictionary<byte, IVariable> lookup = new(); // For faster retrieval by ID. Must be kept in sync with the lists.

        public event Action<IVariable> VariableRemoved = delegate { };

        public IVariable RemoveMuscariAtIndex(int index)
        {
            if (index < 0 || index >= muscariables.Count)
            {
                string errorMessage = $"Index {index} is out of range for muscariables. Valid range is " +
                    $"0 to {muscariables.Count - 1}. No variable removed.";
                throw new IndexOutOfRangeException(errorMessage);
            }
            Muscariable toRemove = muscariables[index];
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
                legacyVariables.Add(legacy);
                RegisterIntoVarLookup(new[] { legacy });
            }

            UpdateNextValidId();
            void UpdateNextValidId()
            {
                // We want it set to one more than the max ID currently in use, so that the next
                // variable added will get an ID that is not already taken.
                byte maxIdInUse = 0;
                foreach (var elem in lookup.Values)
                {
                    if (elem.ItemId > maxIdInUse)
                    {
                        maxIdInUse = elem.ItemId;
                    }
                }
                nextValidVarID = (byte)(maxIdInUse + 1);
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
            bool alreadyRegistered = legacyVariables.ContainsReference(toAdd) ||
                muscariables.ContainsReference(toAdd);
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
            #region Ensure valid id and key
            if (toAdd.ItemId == Muscariable.InvalidID)
            {
                toAdd.ItemId = NextValidVarID();
            }
            else if (lookup.ContainsKey(toAdd.ItemId))
            {
                Debug.LogWarning($"Duplicate variable ID {toAdd.ItemId} found. Reassigning.");
                toAdd.ItemId = NextValidVarID();
            }

            toAdd.Key = UniqueKeyGenerator.GetUniqueKeyFor(toAdd.Key, (IList<IVariable>)Variables, null);
            #endregion

            #region Establish ownership and parent flowchart references
            toAdd.ParentFlowchart = VarOwner as Flowchart;
            toAdd.Owner = VarOwner;
            #endregion

            AddToCachesThenSignal(toAdd);
        }

        private void AddToCachesThenSignal(IVariable toAdd)
        {
            if (toAdd is Muscariable)
            {
                muscariables.Add(toAdd as Muscariable);
            }
            else if (toAdd is Variable)
            {
                legacyVariables.Add(toAdd as Variable);
            }
            lookup[toAdd.ItemId] = toAdd;
            VariableAdded(toAdd);
        }

        public event Action<IVariable> VariableAdded = delegate { };

        public void OnEnable()
        {
            if (VarOwner is UnityObj ownerUnityObj && Application.IsPlaying(ownerUnityObj))
            {
                EnsureValidIds();
                foreach (var elem in lookup.Values)
                {
                    elem.Init(elem.BoxedValue);
                }
            }
        }

        public void Refresh()
        {
            lookup ??= new Dictionary<byte, IVariable>();
            lookup.Clear();
            RegisterIntoVarLookup(muscariables);
            RegisterIntoVarLookup(legacyVariables);
            EnsureValidIds();
        }

        private void RegisterIntoVarLookup(IEnumerable<IVariable> varsToRegister)
        {
            foreach (var elem in varsToRegister)
            {
                if (elem.ItemId == Muscariable.InvalidID)
                {
                    elem.ItemId = NextValidVarID();
                }
                else if (lookup.ContainsKey(elem.ItemId))
                {
                    Debug.LogWarning($"Duplicate variable ID {elem.ItemId} found. Reassigning.");
                    elem.ItemId = NextValidVarID();
                }
                lookup[elem.ItemId] = elem;
            }
        }

        /// <summary>
        /// Checks for duplicate IDs and reassigns them if necessary
        /// </summary>
        public void EnsureValidIds()
        {
            var idGroups = lookup.Values.GroupBy(elem => elem.ItemId);
            foreach (var group in idGroups)
            {
                if (group.Count() > 1)
                {
                    Debug.LogWarning($"Duplicate variable ID {group.Key}. Reassigning IDs.");
                    foreach (var elem in group)
                    {
                        elem.ItemId = NextValidVarID();
                    }
                }
            }
        }

        private byte NextValidVarID()
        {
            byte toReturn = nextValidVarID;
            nextValidVarID++;
            return toReturn;
        }

        

        public IReadOnlyList<IVariable> Variables
        {
            get
            {
                return lookup.Values.ToList();
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
                if (_varOwner != value)
                {
                    _varOwner = value;
                    _varOwner ??= this;
                    foreach (var elem in lookup.Values)
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
            bool alreadyRegistered = lookup.Values.Contains(toRemove);
            if (!alreadyRegistered)
            {
                return;
            }

            RemoveFromCachesThenSignal(toRemove);
        }

        public IVariable GetVariable(byte id)
        {
            lookup.TryGetValue(id, out IVariable result);
            return result;
        }

        public bool Contains(IVariable var)
        {
            return lookup.TryGetValue(var.ItemId, out IVariable found) && found == var;
        }

        public void ResetAll()
        {
            foreach (var variable in lookup.Values)
            {
                variable.OnReset();
            }
        }

        public IVariable GetVariableByName(string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            var result = lookup.Values.FirstOrDefault(var => var.Key.Equals(name, strCompare));
            return result;
        }

        /// <summary>
        /// Gets a variable by name, returning it as the specified generic type if it is of that type. Null otherwise.
        /// </summary>
        public IVariable<TContent> GetVariable<TContent>(string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            var result = lookup.Values.FirstOrDefault(var => var.Key.Equals(name, strCompare));
            return result as IVariable<TContent>;
        }

        public Muscariable AddNewVariableOfContentType(Type contentType, string key)
        {
            EnsureInitialized();
            Muscariable muscaVar = VariableFactory.CreateByContentType(contentType, null);
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
            lookup.TryGetValue(itemId, out IVariable found);
            T result = found as T;
            return result;
        }

        public T GetVarByName<T>(string name, StringComparison strCompare = StringComparison.Ordinal) where T : class, IVariable
        {
            return lookup.Values
                .OfType<T>()
                .FirstOrDefault(var => var.Key.Equals(name, strCompare));
        }

        public IList<T> GetMultiVariables<T>(StringComparison strCompare = StringComparison.Ordinal) where T : IVariable
        {
            return lookup.Values
                .OfType<T>()
                .ToList();
        }

        public IList<T> GetVariablesOfScope<T>(VariableScope scope) where T : IVariable
        {
            return lookup.Values
                .OfType<T>()
                .Where(var => var.Scope == scope)
                .ToList();
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

        public string UniqueId => VarOwner.UniqueId;

        public int VariableCount => lookup.Count;

        IReadOnlyList<Muscariable> IVariableSource<Muscariable>.Variables => Variables.Cast<Muscariable>().ToList();

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
            foreach (var elem in lookup.Values)
            {
                if (elem.ItemId > maxIdInUse)
                {
                    maxIdInUse = elem.ItemId;
                }
            }
            nextValidVarID = (byte)(maxIdInUse + 1);
        }


        public T GetVariableOfType<T>() where T : class, IVariable
        {
            var result = lookup.Values.OfType<T>().FirstOrDefault();
            return result;
        }

        IVariable IVariableSource.GetVariableByName(string name, StringComparison strCompare)
        {
            return GetVariableByName(name, strCompare);
        }

        public T GetVariableOfTypeByName<T>(string name, StringComparison strCompare = StringComparison.Ordinal) where T : class, IVariable
        {
            return GetVariableOfTypeByName(typeof(T), name, strCompare) as T;
        }

        public IVariable GetVariableOfTypeByName(Type type, string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            IVariable result = null;
            var found = GetVariableByName(name, strCompare);
            if (found != null && type.IsAssignableFrom(found.GetType()))
            {
                result = found;
            }
            return result;
        }
    }
}