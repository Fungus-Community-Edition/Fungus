using Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Type = System.Type;

namespace Amanita.VScripting
{
    [CreateAssetMenu(fileName = "NewVariableSourceAsset", menuName = "Amanita/VariableSource")]
    public class VariableSourceAsset : ScriptableObject, IReorderableMuscariableSource
    {
        [SerializeReference] protected List<Muscariable> variables = new List<Muscariable>();

        public IReadOnlyList<IVariable> Variables => variables.ToList();

        IReadOnlyList<Muscariable> IVariableSource<Muscariable>.Variables => variables.ToList();

        protected IList<MuscariableHolder> holders = new List<MuscariableHolder>();

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
            Muscariable var = VariableFactory.Create(contentType, null);
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
            var.Key = UniqueKeyGenerator.GetUniqueKeyFor(var.Key, variables.Cast<IVariable>().ToList(), var);
            IList<IHasItemID> toPass = variables.OfType<IHasItemID>().ToList();
            var.ItemID = UniqueIDGenerator.GetUniqueIDFor(var, toPass, _nextVarID);
            var.Owner = this;
        }

        [SerializeField, HideInInspector] protected int _nextVarID = 0;
        public event Action<IVariable> VariableAdded = delegate { };

        public Muscariable GetVariable(string name)
        {
            return variables.Find(elem => elem.Key == name);
        }

        public virtual IList<Muscariable> GetVarsByContentType<TContent>()
        {
            return GetVarsByContentType(typeof(TContent));
        }

        public virtual IList<Muscariable> GetVarsByContentType(Type contentType)
        {
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
            IList<Muscariable> result = variables.Where((elem) => varType.IsAssignableFrom(elem.GetType())).ToList();
            return result;
        }

        public void ReorderVariables(IList<IVariable> newOrder)
        {
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
            IVariable toRemove = variables.Find(elem => elem.Key == key);
            RemoveVariable(toRemove);
        }

        public virtual void RemoveVariable(IVariable variable)
        {
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

        public virtual IVariable GetVar(int itemID)
        {
            IVariable result = variables.Where((elem) => elem.ItemID == itemID).FirstOrDefault();
            return result;
        }

        public Muscariable AddVariable(Muscariable toAdd)
        {
            if (!variables.ContainsReference(toAdd))
            {
                MakeUniqueForThisSource(toAdd);
                _nextVarID = toAdd.ItemID + 1;
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
            throw new NotImplementedException();
        }
    }

    public interface IVariableSource
    {
        event Action<IVariable> VariableAdded;
        event Action<IVariable> VariableRemoved;
        IReadOnlyList<IVariable> Variables { get; }
        IVariable AddVariable(IVariable toAdd);
        void RemoveVariable(IVariable toRemove);
        IVariable GetVar(int itemId);
    }

    public interface IVariableSource<TVar> : IVariableSource where TVar: IVariable
    {
        new IReadOnlyList<TVar> Variables { get; }
        TVar AddVariable(TVar toAdd);
        void RemoveVariable(TVar toRemove);
        new TVar GetVar(int itemId);
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
}