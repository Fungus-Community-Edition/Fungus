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

        /// <summary>
        /// The owner of the var this is meant to reference. Changing this will
        /// change the context in which the variable is looked up. It is
        /// automatically changed when setting the Variable property.
        /// </summary>
        public IVariableSource VarOwner { get; set; }

        public IVariable Variable
        {
            get
            {
                if (VarOwner == null)
                {
                    return null;
                }
                return VarOwner.GetVariable(itemId);
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
            }
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