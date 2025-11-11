using System;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    // To reduce the boilerplate in IVariableData implementors such as AnimatorData and FloatData

    public abstract class VariableData : IVariableData
    {
        public abstract Type ContentType { get; }
        public abstract object BoxedValue
        {
            get;
            set;
        }

        public abstract IVariable VarRef { get; set; }

        public abstract string GetDescription();

        public virtual IVariableData GetCopy()
        {
            Type thisType = GetType();

            IVariableData theCopy = (IVariableData)Activator.CreateInstance(thisType);
            theCopy.SetContentsTo(this);
            return theCopy;
        }

        public virtual void Refresh() { }

        public virtual void SetContentsTo(IVariableData otherVarData)
        {
            this.VarRef = otherVarData.VarRef;
        }

        protected virtual bool CanHoldAsValue(object obj)
        {
            bool result;

            if (ReferenceEquals(obj, null))
            {
                result = ContentType.IsClass;
            }
            else
            {
                result = ContentType.IsAssignableFrom(obj.GetType());
            }

            return result;
        }
    }

    public interface IVariableData
    {
        Type ContentType { get; }

        object BoxedValue { get; set; }

        /// <summary>
        /// Returns a human-readable description for UI/debug.
        /// </summary>
        string GetDescription();

        IVariable VarRef { get; set; } // To be a more generic way to access stuff like animatorRef, floatRef, etc
        void SetContentsTo(IVariableData otherVarData);

        IVariableData GetCopy();

    }

    public abstract class VariableData<TValue> : VariableData
    {
        [SerializeReference]
        protected IVariable varRef; // Intended target reference (may be lost across reloads)

        protected virtual Variable LegacyVarRef { get; set; } // For backward compatibility

        // Persistent lookup data (survives when varRef does not)
        [SerializeField] private int storedItemId = 0;
        [SerializeField] private string storedOwnerUid = "";
        [SerializeField] private string storedNamespacedKey = ""; // Format used by the drawer (FlowchartName/VarKey) or ~GlobalSource~/VarKey

        public static implicit operator TValue(VariableData<TValue> someData)
        {
            someData.Refresh();
            return someData.Value;
        }

        public VariableData()
        {
            value = default;
            VarRef = null;
        }

        public VariableData(TValue startVal = default)
        {
            value = startVal;
            VarRef = null;
        }

        public override Type ContentType => typeof(TValue);

        public virtual TValue Value
        {
            get
            {
                if (LegacyVarRef != null)
                {
                    return (TValue)LegacyVarRef.BoxedValue;
                }
                else if (VarRef != null)
                {
                    return (TValue)VarRef.BoxedValue;
                }
                else
                {
                    return value;
                }
            }
            set
            {
                if (LegacyVarRef != null)
                {
                    LegacyVarRef.BoxedValue = value;
                }
                else if (VarRef != null)
                {
                    VarRef.BoxedValue = value;
                }
                else
                {
                    this.value = value;
                }
            }
        }

        public override object BoxedValue
        {
            get
            {
                if (LegacyVarRef != null)
                {
                    return LegacyVarRef.BoxedValue;
                }
                else if (VarRef != null)
                {
                    return VarRef.BoxedValue;
                }
                else
                {
                    return value;
                }
            }
            set
            {
                object whatToAssign = null;
                try
                {
                    whatToAssign = (TValue)value;
                }
                catch
                {
                    Debug.LogWarning($"VariableData of value type {typeof(TValue).Name} could not box value " +
                        $"of type {value.GetType().Name} to type {typeof(TValue).Name}");
                }

                if (LegacyVarRef != null)
                {
                    LegacyVarRef.BoxedValue = whatToAssign;
                }
                else if (VarRef != null)
                {
                    VarRef.BoxedValue = whatToAssign;
                }
                else
                {
                    this.value = (TValue)whatToAssign;
                }
            }
        }

        [SerializeReference, SerializeField] protected TValue value = default;

        public override string GetDescription()
        {
            string result = "null"; // <- This is valid for reference types

            if (VarRef == null && value != null)
            {
                result = value.ToString();
            }
            else if (VarRef != null)
            {
                result = VarRef.Key;
            }

            return result;
        }

        public override void SetContentsTo(IVariableData otherVarData)
        {
            var ourType = this.GetType();
            var theirType = otherVarData.GetType();
            if (ourType.Equals(theirType))
            {
                SetContentsTo(otherVarData as VariableData<TValue>);
            }
        }

        public virtual void SetContentsTo(VariableData<TValue> otherVarData)
        {
            this.VarRef = otherVarData.VarRef;
            this.value = otherVarData.value;
        }

        public override IVariable VarRef
        {
            get
            {
                if (LegacyVarRef != null)
                {
                    return LegacyVarRef;
                }

                // If our var ref is for a non-UnityObj type and the owner is missing, that means
                // that the var ref we have points to a copy of the actual var. Thus, we need to try
                // to resolve it again.
                if (varRef != null && !string.IsNullOrEmpty(varRef.OwnerId) && varRef.Owner == null)
                {
                    Debug.Log($"VariableData<{typeof(TValue).Name}> detected that its varRef's owner is null. " +
                        $"Attempting to re-resolve variable reference.");

                    Refresh();
                }
                return varRef;
            }
            set
            {
                bool alreadyAssigned = ReferenceEquals(value, varRef) || ReferenceEquals(value, LegacyVarRef);
                if (alreadyAssigned)
                {
                    return;
                }
                // Capture persistent identity info first
                CaptureMuscariIdentityInfo();
                void CaptureMuscariIdentityInfo()
                {
                    Muscariable mus = value as Muscariable;
                    if (mus == null || mus.ItemId <= Muscariable.InvalidID)
                    {
                        return;
                    }

                    storedItemId = mus.ItemId;
                    // Owner may be a Flowchart or a VariableSourceAsset.
                    if (mus.Owner is Flowchart fChart)
                    {
                        storedOwnerUid = fChart.UniqueId;
                        storedNamespacedKey = $"{fChart.gameObject.name}/{mus.Key}";
                    }
                    else if (mus.Owner is VariableSourceAsset vSourceAsset)
                    {
                        storedOwnerUid = vSourceAsset.UniqueId;
                        storedNamespacedKey = $"~{vSourceAsset.name}~/{mus.Key}";
                    }
                    
                }
                
                CaptureLegacyIdentityInfo();
                void CaptureLegacyIdentityInfo()
                {
                    Variable legacy = value as Variable;
                    if (legacy == null || legacy.ItemId <= Variable.InvalidID)
                    {
                        return;
                    }

                    storedItemId = legacy.ItemId;
                    if (legacy.Owner is Flowchart legacyFc)
                    {
                        storedOwnerUid = legacyFc.UniqueId;
                        storedNamespacedKey = $"{legacyFc.gameObject.name}/{legacy.Key}";
                    }
                }

                if (value == null)
                {
                    storedItemId = 0;
                    // Keep namespaced key so we can attempt re-resolution later if possible
                    varRef = null;
                    LegacyVarRef = null;
                    return;
                }

                if (this.ContentType.IsAssignableFrom(value.ContentType)) // We want to allow polymorphism
                {
                    if (value is UnityObj)
                    {
                        LegacyVarRef = (Variable)value;
                        varRef = null;
                    }
                    else
                    {
                        LegacyVarRef = null;
                        try
                        {
                            varRef = (IVariable<TValue>)value;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed to cast variable of type {value.GetType().Name} to IVariable<{typeof(TValue).Name}>. Exception: {e}");
                            varRef = null;
                        }
                    }
                }
                else
                {
                    string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
                    throw new InvalidCastException(errorMessage);
                }
            }
        }

        public override void Refresh()
        {
            // We might have a live reference to a var, but it has no idea who the owner is. In such a case,
            // that means our reference is a copy, not a pointer to the actual variable.
            bool weHaveLiveRef = varRef != null || LegacyVarRef != null;
            bool weKnowTheOwner = varRef != null && varRef.Owner != null &&
                varRef.OwnerId == storedOwnerUid;
            if (weHaveLiveRef && weKnowTheOwner)
            {
                // The reason we don't worry about the owner for legacy vars is that those are MonoBehaviours,
                // which Unity will automatically re-link on domain reload. Since Muscariables are plain
                // C# objects, we have to do the re-linking ourselves.
                return;
            }

            bool shouldFetchInfo = varRef != null && string.IsNullOrEmpty(storedOwnerUid);
            if (shouldFetchInfo)
            {
                storedOwnerUid = varRef.OwnerId;
                storedItemId = varRef.ItemId;
            }

            bool shouldLookForRef = !string.IsNullOrEmpty(storedOwnerUid) && storedItemId > 0;
            if (shouldLookForRef)
            {
                FindVarRefFromOwner();
            }

        }

        protected virtual void FindVarRefFromOwner()
        {
            if (string.IsNullOrEmpty(storedOwnerUid))
            {
                return;
            }
            IVariableSource owner = FindOwnerWithID(storedOwnerUid);
            if (owner != null)
            {
                // Need to reference the exact variable instance from the owner. Us getting to this point
                // in the code suggests that the varRef we do have is a copy, not a pointer
                // to the actual variable.
                var foundVar = owner.Variables.FirstOrDefault(ownedVar => ownedVar.ItemId == storedItemId);
                if (foundVar != null && ContentType.IsAssignableFrom(foundVar.ContentType))
                {
                    varRef = (IVariable<TValue>)foundVar;
                }
            }
        }

        IVariableSource FindOwnerWithID(string id)
        {
            var owningFlowchart = Flowchart.CachedFlowcharts.FirstOrDefault(fc => fc.UniqueId == id);
            if (owningFlowchart != null)
            {
                var foundVar = owningFlowchart.GetVariableById(varRef.ItemId);
                if (foundVar != null && ContentType.IsAssignableFrom(foundVar.ContentType))
                {
                    varRef = (IVariable<TValue>)foundVar;
                    return owningFlowchart;
                }
            }

            AmanitaManager ammieManager = AmanitaManager.S;
            if (ammieManager != null)
            {
                var owningSource = ammieManager.GlobalVariableSources
                    .FirstOrDefault(vSource => vSource.UniqueId == id);
                if (owningSource != null)
                {
                    var foundVar = owningSource.Variables
                        .FirstOrDefault(ownedVar => ownedVar.ItemId == varRef.ItemId);
                    if (foundVar != null && ContentType.IsAssignableFrom(foundVar.ContentType))
                    {
                        varRef = (IVariable<TValue>)foundVar;
                        return owningSource;
                    }
                }
            }
            return null;
        }

        protected virtual IVariable TryResolve()
        {
            
            // Fallback: brute-force search by ItemId across all flowcharts
            if (storedItemId > 0)
            {
                foreach (var fc in Flowchart.CachedFlowcharts)
                {
                    var candidate = fc.Variables.FirstOrDefault(fcVar => fcVar.ItemId == storedItemId &&
                    ContentType.IsAssignableFrom(fcVar.ContentType));
                    if (candidate != null)
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }
    }

}