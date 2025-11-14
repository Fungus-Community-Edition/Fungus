using System;

namespace Amanita.VScripting
{
    public interface IVariable : IHasKey, IHasItemID, IHasOwnerIDIndex
    {
        void Init();
        new string Key { get; set; }
        object BoxedValue { get; set; }
        VariableScope Scope { get; set; }

        /// <summary>
        /// The type of the value that this is meant to represent. It's like how Fungus
        /// FloatVariables represent float, Fungus StringVariables represent strings,
        /// so on so forth.
        /// </summary>
        Type ContentType { get; }
        bool IsComparisonSupported();
        bool IsArithmeticSupported(SetOperator setOperator);
        bool IsRelationalSupported { get; }

        /// <summary>
        /// Used by Ifs, While, and the like. Child classes required to declare and implement comparisons.
        /// </summary>
        bool Evaluate(CompareOperator compareOperator, object value);

        void Apply(SetOperator setOperator, object value);
        IVariableSource Owner { get; set; }

        
    }

    public interface IVariable<T> : IVariable, IEquatable<T>
    {
        T Value { get; set; }
        void Apply(SetOperator setOperator, T value);
    }

    public interface IHasOwnerIDIndex
    {
        /// <summary>
        /// Used to find the owner of this variable.
        /// </summary>
        int OwnerIdIndex { get; }
    }

}