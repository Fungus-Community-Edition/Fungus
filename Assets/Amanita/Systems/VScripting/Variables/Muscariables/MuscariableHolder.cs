using System;
using UnityEngine;

namespace Amanita.VScripting
{
    public class MuscariableHolder : ScriptableObject, IVariable
    {
        [SerializeReference] protected Muscariable muscariable;

        public virtual string Key
        {
            get
            {
                if (muscariable == null)
                {
                    return string.Empty;
                }

                return muscariable.Key;
            }
            set
            {
                Ensure();
                muscariable.Key = value;
                Dirty();
            }
        }

        public virtual object Value
        {
            get
            {
                if (muscariable == null)
                {
                    return null;
                }

                return muscariable.Value;
            }
            set
            {
                Ensure(); 
                muscariable.Value = value; 
                Dirty(); 
            } 
        }

        public virtual VariableScope Scope
        {
            get
            {
                if (muscariable == null)
                {
                    return VariableScope.Private;
                }

                return muscariable.Scope;
            }
        }

        public virtual int ItemID
        {
            get
            {
                if (muscariable == null)
                {
                    return Muscariable.InvalidID;
                }

                return muscariable.ItemID;
            }
            set
            { 
                Ensure(); 
                muscariable.ItemID = value; 
                Dirty(); 
            } 
        }

        public virtual Type ContentType
        {
            get
            {
                if (muscariable == null)
                {
                    return null;
                }

                return muscariable.ContentType;
            }
        }

        public Muscariable Inner => muscariable; // For Inspectors and such

        public IVariableSource Owner => ((IVariable)muscariable).Owner;

        public virtual void Init(IVariable variable)
        {
            muscariable = variable as Muscariable;
            muscariable ??= VariableFactory.Create(variable?.ContentType, variable);

            Init();
        }

        public virtual void Init()
        {
            Ensure();
            muscariable.Init();
            UpdateName();
        }

        protected virtual void Ensure()
        {
            muscariable ??= VariableFactory.Create(typeof(object));
        }

        protected virtual void UpdateName()
        {
            this.name = $"{muscariable.Key}";
        }

        public virtual void SetFrom(IVariable src)
        {
#if UNITY_EDITOR
            var so = new UnityEditor.SerializedObject(this);
            var prop = so.FindProperty("muscariable");
            UnityEditor.Undo.RecordObject(this, "Set Muscariable");
            // ^We want to be able to revert the change

            prop.managedReferenceValue = VariableFactory.Create(src.ContentType, src);
            so.ApplyModifiedProperties();
#else
            muscariable = MuscariableFactory.Create(src.ContentType, src);
#endif
            Dirty();
        }

        protected virtual void Dirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public virtual bool IsComparisonSupported()
        {
            if (muscariable == null)
            {
                return false;
            }

            return muscariable.IsComparisonSupported();
        }

        public bool Evaluate(CompareOperator compareOperator, object value)
        {
            return ((IVariable)muscariable).Evaluate(compareOperator, value);
        }

        public void Apply(SetOperator setOperator, object value)
        {
            ((IVariable)muscariable).Apply(setOperator, value);
        }

        public virtual void Refresh()
        {
            UpdateName();
        }
    }
}