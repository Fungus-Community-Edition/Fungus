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

        public virtual void Init(IVariable variable)
        {
            if (variable is Muscariable)
            {
                muscariable = (Muscariable)variable;
            }
            else
            {
                muscariable = MuscariableFactory.Create(muscariable.ContentType, variable);
            }

            Init();
        }

        public virtual void Init()
        {
            Ensure();
            muscariable.Init();
        }

        protected virtual void Ensure()
        {
            muscariable ??= MuscariableFactory.Create(typeof(object));
        }

        public virtual void SetFrom(IVariable src)
        {
#if UNITY_EDITOR
            var so = new UnityEditor.SerializedObject(this);
            var prop = so.FindProperty("muscariable");
            UnityEditor.Undo.RecordObject(this, "Set Muscariable");
            // ^We want to be able to revert the change

            prop.managedReferenceValue = MuscariableFactory.Create(src.ContentType, src);
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
        
    }
}