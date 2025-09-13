using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Type = System.Type;

namespace Amanita.VScripting
{
    [CreateAssetMenu(fileName = "NewVariableSource", menuName = "Amanita/VariableSource")]
    public class VariableSource : ScriptableObject, IVariableSource
    {
        [SerializeField] protected List<Muscariable> variables = new List<Muscariable>();

        public IReadOnlyList<IVariable> Variables => variables.ToList();

        public event System.Action<IVariable> VariableAdded = delegate { };
        public event System.Action<IVariable> VariableRemoved = delegate { };

        /// <summary>
        /// Creates and returns a new Muscariable of the content type,
        /// assigning it the passed key and starting value.
        /// </summary>
        public virtual Muscariable<TContent> AddNewVariableOfContentType<TContent>(string key,
            TContent startingVal = default)
        {
            Muscariable<TContent> result = (Muscariable<TContent>)AddNewVariableOfContentType(typeof(TContent), key);
            result.Value = startingVal;
            VariableAdded(result);
            return result;
        }

        public virtual Muscariable AddNewVariableOfContentType(Type contentType, string key)
        {
            Muscariable var = VariableFactory.Create(contentType, null);
            var.Key = key;
            AddVariable(var);
            VariableAdded(var);
            return var;
        }

        /// <summary>
        /// If the var is a legacy one, it will be converted to a Muscariable. Returns
        /// the variable added.
        /// </summary>
        public virtual Muscariable AddVariable(IVariable var)
        {
            bool legacyVar = var is not Muscariable;
            Muscariable varAdded = null;
            if (legacyVar)
            {
                Muscariable convertedVar = VariableFactory.Create(var.ContentType, var);
                if (convertedVar != null && !variables.Contains(convertedVar))
                {
                    variables.Add(convertedVar);
                    varAdded = convertedVar;
                }
                else
                {
                    Debug.LogWarning($"Could not convert legacy variable {var.Key} to a Muscariable.");
                }
            }
            else
            {
                Muscariable muscariVar = var as Muscariable;
                if (!variables.Contains(muscariVar))
                {
                    variables.Add(muscariVar);
                    varAdded = muscariVar;
                }
            }

            VariableAdded(varAdded);
            return varAdded;
        }

        public Muscariable GetVariable(string name)
        {
            return variables.Find(elem => elem.Key == name);
        }

        public virtual IList<Muscariable> GetVarsByContentType<TContent>()
        {
            return GetVarsByContentType(typeof(TContent));
        }

        public virtual IList<Muscariable> GetVarsByContentType(System.Type contentType)
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

        public virtual IList<Muscariable> GetVarsByType(System.Type varType)
        {
            IList<Muscariable> result = variables.Where((elem) => varType.IsAssignableFrom(elem.GetType())).ToList();
            return result;
        }

    }
}