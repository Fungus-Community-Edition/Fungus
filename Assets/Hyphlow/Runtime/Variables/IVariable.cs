using System;

namespace AtMycelia.Hyphlow
{
    public interface IVariable : IHasKey, IHasItemID
    {
        void Init(object startValue = default);
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

        /// <summary>
        /// For returning a variable to its initial value
        /// </summary>
        void OnReset();
        // ^We add the "On" for the sake of compatibility with legacy vars

    }

    public interface IVariable<T> : IVariable, IEquatable<T>
    {
        void Init(T startValue = default);
        T Value { get; set; }
        void Apply(SetOperator setOperator, T value);
    }


}