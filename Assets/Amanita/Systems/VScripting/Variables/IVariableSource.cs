using System;
using System.Collections.Generic;

namespace AtMycelia.Amanita.VScripting
{
    public interface IVariableSource : IHasUniqueID, IHasName
    {
        event Action<IVariable> VariableAdded;
        event Action<IVariable> VariableRemoved;
        IReadOnlyList<IVariable> Variables { get; }

        IVariable AddVariable(IVariable toAdd);
        void RemoveVariable(IVariable toRemove);

        /// <summary>
        /// Returns the variable with the given item ID, or null if there is no variable with 
        /// that item ID in this source.
        IVariable GetVariable(byte itemId);

        /// <summary>
        /// Returns the first variable of the given type, or null if there are no variables of that type.
        /// </summary>
        T GetVariableOfType<T>() where T : class, IVariable;

        /// <summary>
        /// Returns the first variable with the given name, or null if there are no 
        /// variables with that name.
        /// </summary>
        IVariable GetVariable(string name, StringComparison strCompare = StringComparison.Ordinal);

        /// <summary>
        /// Returns the first variable of the given type with the given name, or null if this source
        /// doesn't have such.
        /// 
        /// This is good for when you expect a variable to be of a certain type and want to avoid 
        /// having to cast it after retrieval.
        /// </summary>
        T GetVariableOfType<T>(string name, StringComparison strCompare = StringComparison.Ordinal)
            where T : class, IVariable;

        /// <summary>
        /// Returns the first variable of the given type with the given name, or null if there are none
        /// in this source. Uses the provided StringComparison for the name comparison.
        /// </summary>
        IVariable GetVariableOfType(Type type, string name,
            StringComparison strCompare = StringComparison.Ordinal);

        bool Contains(IVariable var);
    }

    public interface IVariableSource<TVar> : IVariableSource where TVar : IVariable
    {
        new IReadOnlyList<TVar> Variables { get; }
        TVar AddVariable(TVar toAdd);
        void RemoveVariable(TVar toRemove);
    }

    public interface IMuscariableSource : IVariableSource<Muscariable>
    {
        Muscariable AddNewVariableOfContentType(Type contentType, string key);
    }

    public interface IReorderableVariableSource : IVariableSource
    {
        void ReorderVariables(IList<IVariable> newlyOrderedVars);
    }

    public interface IReorderableMuscariableSource : IReorderableVariableSource, IMuscariableSource
    {

    }
}