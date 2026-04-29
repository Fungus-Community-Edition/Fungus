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
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class VariableReference
    {
        // What we do is store the id of the var, and then return the var itself based on
        // what source we're asked to work with. This minimizes the amount of data we need to serialize.
        [SerializeField]
        [FormerlySerializedAs("itemId")]
        private byte _itemId;
        [SerializeField]
        [FormerlySerializedAs("owningSource")]
        private UnityObj _owningSource;

        [FormerlySerializedAs("owningFc")]
        [SerializeField] [HideInInspector] private Flowchart legacyOwningFc;

        [FormerlySerializedAs("owningVsa")]
        [SerializeField] [HideInInspector] private VariableSourceAsset legacyOwningVsa;

        /// <summary>
        /// The key of the variable this is referencing. This is just for display purposes,
        /// and is not used for lookups or serialization.
        /// </summary>
        public virtual string VarKey
        {
            get
            {
                IVariable var = Variable;
                return var != null ? 
                    var.Key : 
                    "";
            }
        }

        /// <summary>
        /// Itemid assigned to the variable this is referencing (or at least meant to reference). This is what is 
        /// serialized, and is used to look up the variable on the owner.
        /// </summary>
        public virtual byte ItemId
        {
            get { return _itemId; }
        }

        /// <summary>
        /// Setter sets not just the variable, but also the owner and itemId to match.
        /// </summary>
        public IVariable Variable
        {
            get
            {
                if (_itemId == Muscariable.InvalidId)
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
                    result = VarOwner.GetVariable(_itemId);
                }
                return result;
            }
            set
            {
                if (value == null)
                {
                    //_itemId = Muscariable.InvalidID; // Commented out for now. We may want to hold onto the item id
                    VarOwner = null;
                }
                else
                {
                    _itemId = value.ItemId;
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

            if (IsUnityObjectNull(_owningSource))
            {
                if (!IsUnityObjectNull(legacyOwningFc))
                {
                    _owningSource = legacyOwningFc;
                    legacyOwningFc = null;
                    legacyOwningVsa = null;
                }
                else if (!IsUnityObjectNull(legacyOwningVsa))
                {
                    _owningSource = legacyOwningVsa;
                    legacyOwningVsa = null;
                }
            }

            varOwner ??= _owningSource as Flowchart;
            varOwner ??= _owningSource as VariableSourceAsset;
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
                _owningSource = value as UnityObj;
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
                object convertedValue;
                if (TryConvertValue(val, ourVar.ContentType, out convertedValue))
                {
                    ourVar.BoxedValue = convertedValue;
                    return;
                }

                var ourContentType = ourVar.ContentType;
                var valueType = val?.GetType();
                bool typesAreCompatible = TypeUtils.TypesCompatible(valueType, ourContentType);
                bool canBeAssigned = (ourContentType.IsClass && val == null) || typesAreCompatible;
                if (!canBeAssigned)
                {
                    UnityObj ctx = _owningSource is UnityObj ?
                        _owningSource :
                        null;
                    Debug.LogError($"VariableReference: Value type {valueType} is not " +
                        $"assignable to variable content type {ourContentType}.", ctx);
                }
                else
                {
                    ourVar.BoxedValue = val;
                }
            }
        }

        private static bool TryConvertValue(object value, System.Type targetType, out object convertedValue)
        {
            if (value is Vector3 vector3 && targetType == typeof(Vector2))
            {
                convertedValue = (Vector2)vector3;
                return true;
            }

            if (value is Vector2 vector2 && targetType == typeof(Vector3))
            {
                convertedValue = (Vector3)vector2;
                return true;
            }

            convertedValue = null;
            return false;
        }
    }

}