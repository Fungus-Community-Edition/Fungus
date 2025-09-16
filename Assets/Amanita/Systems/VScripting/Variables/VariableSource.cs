using Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Type = System.Type;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Amanita.VScripting
{
    [CreateAssetMenu(fileName = "NewVariableSource", menuName = "Amanita/VariableSource")]
    public class VariableSource : ScriptableObject, IReorderableMuscariableSource
    {
        [SerializeReference] protected List<Muscariable> variables = new List<Muscariable>();

        public IReadOnlyList<IVariable> Variables => variables.ToList();

        public event Action<IVariable> VariableAdded = delegate { };
        public event Action<IVariable> VariableRemoved = delegate { };

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
        public virtual Muscariable AddVariable(IVariable var)
        {
            Muscariable muscari = ConvertAsNeeded(var);
            if (muscari != null && !variables.ContainsReference(muscari))
            {
                muscari.Key = UniqueKeyGenerator.GetUniqueKeyFor(muscari.Key, variables, muscari);
                IList<IHasItemID> toPass = variables.OfType<IHasItemID>().ToList();
                muscari.ItemID = UniqueIDGenerator.GetUniqueIDFor(muscari, toPass, _nextVarID);
                variables.Add(muscari);
                VariableAdded(muscari);
                SetDirtyAndSave();
            }

            return muscari;
        }

        public virtual void SetDirtyAndSave()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        [SerializeField] protected int _nextVarID = 0;

        protected virtual Muscariable ConvertAsNeeded(IVariable var)
        {
            Muscariable muscari = var as Muscariable;
            bool needToConvert = muscari == null;
            if (needToConvert)
            {
                muscari = VariableFactory.Create(var.ContentType, var);
                bool conversionSuccess = muscari != null && !variables.ContainsReference(muscari);
                if (!conversionSuccess)
                {
                    Debug.LogWarning($"Could not convert legacy variable {var.Key} to a Muscariable.");
                }
            }
            return muscari;
        }

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
            IList<Muscariable> result = variables.Where(VarIsOfContentType)
                .ToList();

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
                SetDirtyAndSave();
            }
        }

    }

    public interface IVariableSource
    {
        event Action<IVariable> VariableAdded;
        event Action<IVariable> VariableRemoved;
        IReadOnlyList<IVariable> Variables { get; }
    }

    public interface IMuscariableSource : IVariableSource
    {
        Muscariable GetVariable(string name);
        Muscariable AddVariable(IVariable toAdd);
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