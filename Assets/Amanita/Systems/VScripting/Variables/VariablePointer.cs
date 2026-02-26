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

}