using UnityEngine;
using Type = System.Type;

namespace Amanita.VScripting
{
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

        protected virtual void RefreshOwner()
        {
            varOwner ??= owningFc;
            varOwner ??= owningVsa;
        }

        private IVariableSource varOwner;
        // ^We have this for when users want to use this class with their own non-Flowchart
        // and non-VSA variable sources. In those cases, though, the users will need to
        // subclass this and override RefreshOwner to make sure it works properly.

        public IVariable Variable
        {
            get
            {
                // Lazy loading so that things work both in the editor and at runtime
                RefreshOwner();
                RefreshVar();
                return variable;
            }
            set
            {
                if (value == null)
                {
                    itemId = Muscariable.InvalidID;
                }
                else
                {
                    itemId = value.ItemId;
                    VarOwner = value.Owner;
                }
                variable = value;
            }
        }

        private IVariable variable;

        public virtual void Refresh()
        {
            RefreshOwner();
            RefreshVar();
        }

        private void RefreshVar()
        {
            if (varOwner == null)
            {
                return;
            }
            variable = VarOwner.GetVariable(itemId);
            Debug.Log($"Refreshed variable. New variable: {variable?.Key ?? "null"}");
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
                if (!targetType.IsAssignableFrom(contentType))
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

        public void SetValue<T>(T value)
        {
            if (Variable == null)
            {
                Debug.LogError("VariableReference: Variable is null.");
            }
            else
            {
                var contentType = Variable.ContentType;
                var valueType = value?.GetType();
                if (value != null && !contentType.IsAssignableFrom(valueType))
                {
                    Debug.LogError($"VariableReference: Value type {valueType} is not " +
                        $"assignable to variable content type {contentType}.");
                }
                else
                {
                    Variable.BoxedValue = value;
                }
            }
        }
    }
}