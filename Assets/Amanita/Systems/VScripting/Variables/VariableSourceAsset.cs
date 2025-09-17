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
            if (muscari != null && !variables.ContainsReference(muscari))
            {
                MakeUniqueForThisSource(muscari);
                _nextVarID = muscari.ItemID + 1;

                variables.Add(muscari);
                VariableAdded(muscari);
            }

            return muscari;
        }

        // So that we can avoid what (at least look like) duplicates
        protected virtual void MakeUniqueForThisSource(Muscariable var)
        {
            var.Key = UniqueKeyGenerator.GetUniqueKeyFor(var.Key, variables, var);
            IList<IHasItemID> toPass = variables.OfType<IHasItemID>().ToList();
            var.ItemID = UniqueIDGenerator.GetUniqueIDFor(var, toPass, _nextVarID);
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

        public event Action VariablesReordered = delegate { };

        public virtual void RemoveVariable(string key)
        {
            IVariable toRemove = variables.Find(elem => elem.Key == key);
            RemoveVariable(toRemove);
        }

        public virtual void RemoveVariable(IVariable variable)
        {
            Muscariable muscari = variable as Muscariable;
            if (muscari == null)
            {
                string logMessage = $"Cannot remove {variable} (a non-Muscariable) from a VariableSource asset; " +
                    $"it can't hold that in the first place.";
                Debug.LogWarning(logMessage);
                return;
            }

            variables.Remove(muscari);
            VariableRemoved(muscari);
        }

        public event Action<IVariable> VariableRemoved = delegate { };

        public virtual void Refresh()
        {
            variables.RemoveAll(elem => elem == null);
            Refreshed();
        }

        public event Action Refreshed = delegate { };

    }

    public interface IVariableSource
    {
        event Action<IVariable> VariableAdded;
        event Action<IVariable> VariableRemoved;
        IReadOnlyList<IVariable> Variables { get; }
        IVariable AddVariable(IVariable toAdd);
        void RemoveVariable(IVariable toRemove);
    }

    public interface IMuscariableSource : IVariableSource
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
}