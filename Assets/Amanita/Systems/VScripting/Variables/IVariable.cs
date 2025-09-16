using System;
using UnityEngine;

namespace Amanita.VScripting
{
    public interface IVariable : IHasKey, IHasItemID
    {
        void Init();
        new string Key { get; set; }
        object Value { get; set; }
        VariableScope Scope { get; }

        /// <summary>
        /// The type of the value that this is meant to represent. It's like how Fungus
        /// FloatVariables represent float, Fungus StringVariables represent strings,
        /// so on so forth.
        /// </summary>
        Type ContentType { get; }
        bool IsComparisonSupported();

        /// <summary>
        /// Used by Ifs, While, and the like. Child classes required to declare and implement comparisons.
        /// </summary>
        bool Evaluate(CompareOperator compareOperator, object value);

        void Apply(SetOperator setOperator, object value);
    }

    public interface IVariable<T> : IVariable, IEquatable<T>
    {
        new T Value { get; set; }
        void Apply(SetOperator setOperator, T value);
    }

}