using AtMycelia.Amanita.VScripting;

namespace VScriptingTests.VariableOperations
{
    [VariableInfo("", "", typeof(int), ShowInMenu = false)]
    public class HookedIntMuscariable : IntMuscariable
    {
        public object LastBasePrev;
        public int LastGenericPrev;
        public int BaseSetCount;
        public int GenericSetCount;


    }
}