using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow
{
    public interface IVariablePointer: IVariable
    {
        UnityObj Component { get; set; }
        bool Equals(IVariable other);
    }

}