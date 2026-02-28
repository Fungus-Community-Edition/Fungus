using System;
using System.Collections.Generic;

namespace Amanita.VScripting
{
    public interface IVariableSource : IHasUniqueID
    {
        event Action<IVariable> VariableAdded;
        event Action<IVariable> VariableRemoved;
        IReadOnlyList<IVariable> Variables { get; }

        IVariable AddVariable(IVariable toAdd);
        void RemoveVariable(IVariable toRemove);

        IVariable GetVariable(byte itemId);

        /// <summary>
        /// Returns the first variable of the given type, or null if there are no variables of that type.
        /// </summary>
        T GetVariableOfType<T>() where T : class, IVariable;

        /// <summary>
        /// Returns the first variable with the given name, or null if there are no 
        /// variables with that name. Uses the provided StringComparison for the name comparison.
        /// </summary>
        IVariable GetVariableByName(string name, StringComparison strCompare = StringComparison.Ordinal);

        /// <summary>
        /// Returns the first variable of the given type with the given name, or null if there are no 
        /// variables with that name and type. Uses the provided StringComparison for the name comparison.
        /// This is good for when you expect a variable to be of a certain type and want to avoid 
        /// having to cast it after retrieval.
        /// </summary>
        T GetVariableOfTypeByName<T>(string name, StringComparison strCompare = StringComparison.Ordinal)
            where T : class, IVariable;

        /// <summary>
        /// Returns the first variable of the given type with the given name, or null if there are none
        /// in this source. Uses the provided StringComparison for the name comparison.
        /// </summary>
        IVariable GetVariableOfTypeByName(Type type, string name, StringComparison strCompare = StringComparison.Ordinal);

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