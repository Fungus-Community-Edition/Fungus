using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting
{
    public class VarSourceBehaviour : MonoBehaviour, IVariableSource
    {
        public IReadOnlyList<IVariable> Variables => throw new NotImplementedException();

        public event Action<IVariable> VariableAdded;
        public event Action<IVariable> VariableRemoved;
    }
}