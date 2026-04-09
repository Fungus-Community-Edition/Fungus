using UnityEngine;
using UnityEngine.Serialization;
using UnityObj = UnityEngine.Object;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// A reference to a variable belonging to a variable source (Flowchart or VariableSourceAsset).
    /// If you want this to work with a source that is not derived from either of those,
    /// you will need to subclass this.
    /// </summary>
    [System.Serializable]
[MovedFrom("AtMycelia.Amanita.VScripting")]
    public class VariableReference
    {
        // What we do is store the id of the var, and then return the var itself based on
        // what source we're asked to work with. This minimizes the amount of data we need to serialize.
        [SerializeField] private byte itemId;
        [SerializeField] private UnityObj owningSource;

        [FormerlySerializedAs("owningFc")]
        [SerializeField] [HideInInspector] private Flowchart legacyOwningFc;

        [FormerlySerializedAs("owningVsa")]
        [SerializeField] [HideInInspector] private VariableSourceAsset legacyOwningVsa;

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
                if (itemId == Muscariable.InvalidID)
                {
                    //Debug.LogWarning($"VariableReference: Variable is null. Owner is {VarOwner}");
                    return null;
                }

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

        /// <summary>
        /// Refreshes the owner variable source. Subclasses should override this if they
        /// want to support variable sources other than Flowchart or VariableSourceAsset.
        /// </summary>
        protected virtual void RefreshOwner()
        {
            varOwner = null;

            if (IsUnityObjectNull(owningSource))
            {
                if (!IsUnityObjectNull(legacyOwningFc))
                {
                    owningSource = legacyOwningFc;
                    legacyOwningFc = null;
                    legacyOwningVsa = null;
                }
                else if (!IsUnityObjectNull(legacyOwningVsa))
                {
                    owningSource = legacyOwningVsa;
                    legacyOwningVsa = null;
                }
            }

            varOwner ??= owningSource as Flowchart;
            varOwner ??= owningSource as VariableSourceAsset;
        }

        private static bool IsUnityObjectNull(UnityObj unityObj)
        {
            if (ReferenceEquals(unityObj, null))
            {
                return true;
            }

            try
            {
                return unityObj == null;
            }
            catch (System.InvalidOperationException)
            {
                return false;
            }
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
                owningSource = value as UnityObj;
                legacyOwningFc = null;
                legacyOwningVsa = null;
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
            var targetType = typeof(T);
            if (varToFetchFrom == null)
            {
                Debug.LogError($"VariableReference: Variable is null. Returning default " +
                    $"value of type {targetType}.");
            }
            else
            {
                var contentType = varToFetchFrom.ContentType;
                bool typesAreCompatible = TypeUtils.TypesCompatible(targetType, contentType);
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
                Debug.LogError("VariableReference: Variable is null. Cannot set value.");
            }
            else
            {
                var ourContentType = ourVar.ContentType;
                var valueType = val?.GetType();
                bool typesAreCompatible = ourContentType.IsAssignableFrom(valueType) || 
                    valueType.IsAssignableFrom(ourContentType);
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