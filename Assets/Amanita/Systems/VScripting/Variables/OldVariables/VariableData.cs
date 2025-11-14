using System;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    // To reduce the boilerplate in IVariableData implementors such as AnimatorData and FloatData
    public abstract class VariableData : IVariableData
    {
        // Persistent lookup data (survives when varRef does not)
        [SerializeField] protected int storedOwnerUidIndex = -1;
        // ^Used to find the owner of the variable (Flowchart or VariableSourceAsset)
        [SerializeField] protected byte storedItemId = 0;
        // ^Used to find the variable within its owner

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
                if (varRef != null && varRef.OwnerIdIndex >= 0 && varRef.Owner == null)
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
                // Capture persistent identity info first. Also, we assume that the var already has its owner registered.
                CaptureMuscariIdentityInfo();
                void CaptureMuscariIdentityInfo()
                {
                    Muscariable mus = value as Muscariable;
                    if (mus == null || mus.ItemId <= Muscariable.InvalidID)
                    {
                        return;
                    }

                    storedItemId = mus.ItemId;

                    // We're registering the owner UniqueId as an index into the appropriate GuidRegistry
                    // so that when needed, we can find the owner even if its UniqueId string changes.
                    // And when we find the owner, we can be sure to get the right VarRef.
                    GuidRegistry registry = null;
                    if (mus.Owner is Flowchart fChart)
                    {
                        registry = AmanitaManager.GetOrAddGuidRegistryFor<Flowchart>();
                        storedOwnerUidIndex = registry.GetOrAddNumericId(fChart.UniqueId);
                    }
                    else if (mus.Owner is VariableSourceAsset vSourceAsset)
                    {
                        registry = AmanitaManager.GetOrAddGuidRegistryFor<VariableSourceAsset>();
                        storedOwnerUidIndex = registry.GetOrAddNumericId(vSourceAsset.UniqueId);
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

                    GuidRegistry registry = AmanitaManager.GetOrAddGuidRegistryFor<Flowchart>();
                    storedItemId = legacy.ItemId;
                    if (legacy.Owner is Flowchart legacyFc)
                    {
                        storedOwnerUidIndex = registry.GetOrAddNumericId(legacyFc.UniqueId);
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
                 varRef.OwnerIdIndex == storedOwnerUidIndex;
            if (weHaveLiveRef && weKnowTheOwner)
            {
                // The reason we don't worry about the owner for legacy vars is that those are MonoBehaviours,
                // which Unity will automatically re-link on domain reload. Since Muscariables are plain
                // C# objects, we have to do the re-linking ourselves.
                return;
            }

            bool shouldFetchInfo = varRef != null && storedOwnerUidIndex < 0;
            if (shouldFetchInfo)
            {
                storedOwnerUidIndex = varRef.OwnerIdIndex;
                storedItemId = varRef.ItemId;
            }

            bool shouldLookForRef = storedOwnerUidIndex >= 0 && storedItemId > 0;
            if (shouldLookForRef)
            {
                FindVarRefFromOwner();
            }

        }

        protected virtual void FindVarRefFromOwner()
        {
            if (storedOwnerUidIndex < 0)
            {
                return;
            }

            IVariableSource owner = FindOwnerUsingIndex(storedOwnerUidIndex);
            if (owner != null)
            {
                // Need to reference the exact variable instance from the owner. Us getting to this point
                // in the code suggests that the varRef we do have is a copy, not a pointer
                // to the actual variable.
                var foundVar = owner.GetVariable(storedItemId);
                if (foundVar != null && ContentType.IsAssignableFrom(foundVar.ContentType))
                {
                    varRef = (IVariable<TValue>)foundVar;
                }
            }
        }

        protected virtual IVariableSource FindOwnerUsingIndex(int ownerIndex)
        {
            GuidRegistry flowchartRegistry = AmanitaManager.GetOrAddGuidRegistryFor<Flowchart>();
            var owningFlowchart = Flowchart.CachedFlowcharts.FirstOrDefault(FlowchartTiedToIndex);
            bool FlowchartTiedToIndex(Flowchart fc)
            {
                return flowchartRegistry.GetNumericId(fc.UniqueId) == ownerIndex;
            }

            if (owningFlowchart != null)
            {
                return owningFlowchart;
            }

            AmanitaManager ammieManager = AmanitaManager.S;
            if (ammieManager != null)
            {
                GuidRegistry vSourceRegistry = AmanitaManager.GetOrAddGuidRegistryFor<VariableSourceAsset>();
                var owningSource = ammieManager.GlobalVariableSources.FirstOrDefault(SourceTiedToIndex);
                bool SourceTiedToIndex(VariableSourceAsset source)
                {
                    return vSourceRegistry.GetNumericId(source.UniqueId) == ownerIndex;
                }

                if (owningSource != null)
                {
                    return owningSource;
                }
            }

            return null;
        }

    }

}