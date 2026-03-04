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

        public void Initialize()
        {
            if (IsInitted)
            {
                Debug.LogWarning("VariableManager is already initialized. Reinitializing will clear " +
                    "all variables and reset the manager. Proceeding with reinitialization.");
            }
            Clear();
            Refresh();
            IsInitted = true;
        }

        public void Initialize(IList<Muscariable> initMuscaris, IList<Variable> initLegacies)
        {
            Initialize();

            AddMultiVars(initMuscaris);

            // We don't want to convert the legacies yet
            for (int i = 0; i < initLegacies.Count; i++)
            {
                Variable legacy = initLegacies[i];
                legacyVariables.Add(legacy);
                RegisterIntoVarLookup(new[] { legacy });
            }
        }

        public void OnEnable()
        {
            if (VarOwner is UnityObj ownerUnityObj && Application.IsPlaying(ownerUnityObj))
            {
                foreach (var elem in lookup.Values)
                {
                    elem.Init(elem.BoxedValue);
                }
            }
        }
        public bool IsInitted
        {
            get => isInitted;
            private set => isInitted = value;
        }
        [SerializeField] private bool isInitted = false;

        public void Refresh()
        {
            lookup ??= new Dictionary<byte, IVariable>();
            lookup.Clear();
            RegisterIntoVarLookup(muscariables);
            RegisterIntoVarLookup(legacyVariables);
            EnsureValidIds();
        }

        private Dictionary<byte, IVariable> lookup = new();


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

        public void AddMultiVars(IEnumerable<IVariable> toAdd)
        {
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

            muscariables.Add(toAdd);
            lookup[toAdd.ItemId] = toAdd;
            VariableAdded(toAdd);
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
                        elem.Owner = _varOwner;
                    }
                }
            }
        }

        private IVariableSource _varOwner;

        public event Action<IVariable> VariableAdded = delegate { };

        
        public void RemoveVariable(IVariable toRemove)
        {
            bool alreadyRegistered = lookup.Values.Contains(toRemove);
            if (!alreadyRegistered)
            {
                return;
            }

            RemoveFromCachesThenSignal(toRemove);
        }

        private void RemoveFromCachesThenSignal(IVariable toRemove)
        {
            legacyVariables.Remove(toRemove as Variable);
            muscariables.Remove(toRemove as Muscariable);
            lookup.Remove(toRemove.ItemId);
            VariableRemoved(toRemove);
        }

        public event Action<IVariable> VariableRemoved = delegate { };

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

        public IVariable GetVariable(byte id)
        {
            lookup.TryGetValue(id, out IVariable result);
            return result;
        }

        public bool Contains(IVariable var)
        {
            return lookup.TryGetValue(var.ItemId, out IVariable found) && found == var;
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
            Muscariable muscaVar = VariableFactory.CreateByContentType(contentType, null);
            Integrate(muscaVar);
            return muscaVar;
        }

        public Muscariable AddVariable(Muscariable toAdd)
        {
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