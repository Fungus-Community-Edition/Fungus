using System;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    public interface IVariablePointer: IVariable
    {
        UnityObj Component { get; set; }
        bool Equals(IVariable other);
    }

    public class VariablePointer<T> : IVariable<T>, IVariablePointer
    {
        [SerializeField] private UnityObj _component; // MonoBehaviour or ScriptableObject
        public VariablePointer() { }

        public VariablePointer(UnityObj component)
        {
            _component = component;
        }

        // IHasKey / IVariable shared members
        public string Key
        {
            get => (_component as IVariable)?.Key ?? string.Empty;
            set { if (_component is IVariable iv) iv.Key = value; }
        }

        public VariableScope Scope
        {
            get => (_component as IVariable)?.Scope ?? VariableScope.Private;
            set { if (_component is IVariable iv) iv.Scope = value; }
        }

        public int ItemID
        {
            get => (_component as IVariable)?.ItemID ?? 0;
            set { if (_component is IVariable iv) iv.ItemID = value; }
        }

        public Type ContentType =>
            (_component as IVariable)?.ContentType ?? typeof(T);

        public void Init()
        {
            if (_component is IVariable iv) iv.Init();
        }

        public bool IsComparisonSupported()
        {
            return (_component as IVariable)?.IsComparisonSupported() ?? false;
        }

        public bool Evaluate(CompareOperator compareOperator, object value)
        {
            return (_component as IVariable)?.Evaluate(compareOperator, value) ?? false;
        }

        public void Apply(SetOperator setOperator, object value)
        {
            if (_component is IVariable iv) iv.Apply(setOperator, value);
        }

        // IVariable<T>
        public T Value
        {
            get => _component is IVariable<T> iv ? iv.Value : default;
            set { if (_component is IVariable<T> iv) iv.Value = value; }
        }

        public void Apply(SetOperator setOperator, T value)
        {
            if (_component is IVariable<T> iv) iv.Apply(setOperator, value);
        }

        bool IVariablePointer.Equals(IVariable other)
        {
            bool result = false;
            IVariable ourVar = _component as IVariable;
            if (ourVar != null)
            {
                result = ourVar.Equals(other);
            }
            return result;
        }

        public bool Equals(T other)
        {
            if (_component is IVariable<T> iv) return iv.Equals(other);
            return false;
        }

        // Explicit IVariable.Value (object) to avoid the name clash
        object IVariable.Value
        {
            get => Value;
            set
            {
                if (_component is IVariable<T> iv)
                {
                    // Try exact cast, then Convert.ChangeType for primitives if needed
                    if (value is T tv)
                        iv.Value = tv;
                    else
                        iv.Value = (T)Convert.ChangeType(value, typeof(T));
                }
            }
        }

        // Convenience
        public UnityObj Component { get => _component; set => _component = value; }

        public virtual IVariableSource Owner
        {
            get => (_component as IVariable)?.Owner;
            set { if (_component is IVariable iv) iv.Owner = value; }
        }
    }
}