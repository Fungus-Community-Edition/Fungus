using Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.VScripting
{
    [SerializeField]
    public sealed class VariableManager : IVariableSource
    {
        [SerializeField] private readonly List<Muscariable> muscariables = new();
        [SerializeField] private readonly List<Variable> legacyVariables = new();
        [SerializeField] private byte nextValidVarID = 1;

        public VariableManager()
        {
            Refresh();
        }

        private Dictionary<byte, IVariable> lookup = new();

        public IReadOnlyList<IVariable> Variables
        {
            get
            {
                return lookup.Values.ToList();
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
            toAdd.Init(toAdd.BoxedValue);
            VariableAdded(toAdd);
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

        public void AddMulti(IEnumerable<IVariable> toAdd)
        {
            foreach (var elem in toAdd)
            {
                AddVariable(elem);
            }
        }

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
            legacyVariables.Clear();
            muscariables.Clear();
            lookup?.Clear();
        }

        public void ResetAll()
        {
            foreach (var variable in lookup.Values)
            {
                variable.OnReset();
            }
        }

        public byte NextId { get; }

        public string UniqueId => VarOwner.UniqueId;

    }
}