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
            // If we still have a live reference or a legacy MB reference, nothing to do.
            if (varRef != null || LegacyVarRef != null)
            {
                return;
            }

            if (storedItemId <= 0 && string.IsNullOrEmpty(storedNamespacedKey))
            {
                return;
            }

            TryRehydrate();
            void TryRehydrate()
            {
                IVariable resolved = TryResolve();

                if (resolved != null)
                {
                    // Reassign through VarRef to recapture identifiers (in case they changed)
                    VarRef = resolved;
                }
            }
        }

        protected virtual IVariable TryResolve()
        {
            bool ownerIdRegistered = !string.IsNullOrEmpty(storedOwnerUid);
            if (ownerIdRegistered)
            {
                bool ownerIsGlobalSource = storedNamespacedKey.StartsWith("~");
                if (ownerIsGlobalSource)
                {
                    var manager = AmanitaManager.S;
                    if (manager != null)
                    {
                        foreach (var src in manager.GlobalVariableSources)
                        {
                            if (src.UniqueId == storedOwnerUid && storedItemId > 0)
                            {
                                var varFound = src.Variables.FirstOrDefault(v => v.ItemId == storedItemId);
                                if (varFound != null && ContentType.IsAssignableFrom(varFound.ContentType))
                                {
                                    return varFound;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var flowcharts = Flowchart.CachedFlowcharts;
                    var owningFc = flowcharts.FirstOrDefault(fc => fc.UniqueId == storedOwnerUid);
                    if (owningFc != null && storedItemId > 0)
                    {
                        var varFound = owningFc.GetVariableById(storedItemId);
                        if (varFound != null && ContentType.IsAssignableFrom(varFound.ContentType))
                        {
                            return varFound;
                        }
                    }
                }
            }

            bool checkOtherFlowcharts = !string.IsNullOrEmpty(storedNamespacedKey);
            if (checkOtherFlowcharts)
            {
                int slashIdx = storedNamespacedKey.IndexOf('/');
                if (slashIdx > 0)
                {
                    string fcName = storedNamespacedKey.Substring(0, slashIdx);
                    string varKey = storedNamespacedKey.Substring(slashIdx + 1);

                    var flowcharts = Flowchart.CachedFlowcharts;
                    foreach (var fc in flowcharts)
                    {
                        if (fc.gameObject.name == fcName)
                        {
                            var found = fc.Variables.FirstOrDefault(fcVar => fcVar.Key == varKey &&
                            ContentType.IsAssignableFrom(fcVar.ContentType));
                            if (found != null)
                            {
                                return found;
                            }
                        }
                    }
                }
                
            }

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