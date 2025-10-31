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

        public virtual object BoxedValue
        {
            get
            {
                if (muscariable == null)
                {
                    return null;
                }

                return muscariable.BoxedValue;
            }
            set
            {
                Ensure(); 
                muscariable.BoxedValue = value; 
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
            set
            {
                Ensure(); 
                muscariable.Scope = value; 
                Dirty();
            }
        }

        public virtual int ItemId
        {
            get
            {
                if (muscariable == null)
                {
                    return Muscariable.InvalidID;
                }

                return muscariable.ItemId;
            }
            set
            { 
                Ensure(); 
                muscariable.ItemId = value; 
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

        public IVariableSource Owner
        {
            get
            {
                if (muscariable == null)
                {
                    return null;
                }
                return muscariable.Owner;
            }
            set
            {
                Ensure(); 
                muscariable.Owner = value; 
                Dirty();
            }
        }

        public bool IsRelationalSupported => ((IVariable)muscariable).IsRelationalSupported;

        public virtual void Init(IVariable variable)
        {
            muscariable = variable as Muscariable;
            muscariable ??= VariableFactory.CreateByContentType(variable?.ContentType, variable);

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

            prop.managedReferenceValue = VariableFactory.CreateByContentType(src.ContentType, src);
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

        public bool IsArithmeticSupported(SetOperator setOperator)
        {
            return ((IVariable)muscariable).IsArithmeticSupported(setOperator);
        }
    }
}