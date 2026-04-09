using UnityObj = UnityEngine.Object;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    public interface IVariablePointer: IVariable
    {
        UnityObj Component { get; set; }
        bool Equals(IVariable other);
    }

}