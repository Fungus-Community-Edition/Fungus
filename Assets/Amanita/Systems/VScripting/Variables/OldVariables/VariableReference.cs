using UnityEngine;
using Type = System.Type;

namespace Amanita.VScripting
{
    /// <summary>
    /// A reference to a variable belonging to a variable source (Flowchart or VariableSourceAsset).
    /// If you want this to work with a source that is not derived from either of those,
    /// you will need to subclass this.
    /// </summary>
    [System.Serializable]
    public class VariableReference
    {
        // What we do is store the id of the var, and then return the var itself based on
        // what source we're asked to work with. This minimizes the amount of data we need to serialize.
        [SerializeField] private byte itemId;
        [SerializeField] private Flowchart owningFc;
        [SerializeField] private VariableSourceAsset owningVsa;
        // ^We use these two so that we can have an easier time fetching the right variable
        // through the Variable property. Especially necessary for the editor.

        public virtual byte VarItemId
        {
            get { return itemId; }
        }

        /// <summary>
        /// Setter sets not just the variable, but also the owner and itemId to match.
        /// </summary>
        public IVariable Variable
        {
            get
            {
                // We want this calculated purely based on the stored id as well as the 
                // owner referenced
                RefreshOwner();
                IVariable result = null;
                if (VarOwner != null)
                {
                    result = VarOwner.GetVariable(itemId);
                }
                return result;
            }
            set
            {
                if (value == null)
                {
                    itemId = Muscariable.InvalidID;
                    VarOwner = null;
                }
                else
                {
                    itemId = value.ItemId;
                    VarOwner = value.Owner;
                }
            }
        }

        protected virtual void RefreshOwner()
        {
            // This func is one of the things that subclasses will need to override to 
            // make sure they work with non-FC and non-VSA owners.
            varOwner = null;
            varOwner ??= owningFc;
            varOwner ??= owningVsa;
        }

        private IVariableSource varOwner;
        // ^We have this for when users want to use this class with their own non-Flowchart
        // and non-VSA variable sources. In those cases, though, the users will need to
        // subclass this and override RefreshOwner to make sure it works properly.

        /// <summary>
        /// The owner of the var this is meant to reference. Changing this will
        /// change the context in which the variable is looked up. It is
        /// automatically changed when setting the Variable property.
        /// </summary>
        public IVariableSource VarOwner
        {
            get
            {
                RefreshOwner();
                return varOwner;
            }
            set
            {
                varOwner = value;
                owningFc = value as Flowchart;
                owningVsa = value as VariableSourceAsset;
            }
        }

        public virtual void Refresh()
        {
            RefreshOwner();
        }

        public T GetValue<T>()
        {
            T result = default;
            IVariable varToFetchFrom = Variable;

            if (varToFetchFrom == null)
            {
                Debug.LogError("VariableReference: Variable is null.");
            }
            else
            {
                var contentType = varToFetchFrom.ContentType;
                var targetType = typeof(T);
                bool typesAreCompatible = targetType.IsAssignableFrom(contentType);
                if (!typesAreCompatible)
                {
                    Debug.LogError($"VariableReference: Variable content type {contentType} is not " +
                        $"assignable to target type {targetType}.");
                }
                else
                {
                    result = (T)varToFetchFrom.BoxedValue;
                }
            }

            return result;
        }

        public void SetValue<T>(T val)
        {
            IVariable ourVar = Variable; // To reduce lookups, we cache it here.
            if (ourVar == null)
            {
                Debug.LogError("VariableReference: Variable is null.");
            }
            else
            {
                var ourContentType = ourVar.ContentType;
                var valueType = val?.GetType();
                bool typesAreCompatible = ourContentType.IsAssignableFrom(valueType);
                bool canBeAssigned = (ourContentType.IsClass && val == null) || typesAreCompatible;
                if (!canBeAssigned)
                {
                    Debug.LogError($"VariableReference: Value type {valueType} is not " +
                        $"assignable to variable content type {ourContentType}.");
                }
                else
                {
                    ourVar.BoxedValue = val;
                }
            }
        }
    }
}