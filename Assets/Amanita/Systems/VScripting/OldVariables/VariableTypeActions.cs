using System;

namespace Amanita.VScripting
{
    public class VariableTypeActions
    {
        public Func<IVariable, IVariableData, CompareOperator, bool> CompareFunc;
        public Func<IVariableData, string> DescFunc;
        public Action<IVariable, IVariableData, SetOperator> SetFunc;
    }
}