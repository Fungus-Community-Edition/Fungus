using AtMycelia.Hyphlow;

namespace VScriptingTests.VariableOperations
{
    [VariableInfo("", "", typeof(int), ShowInMenu = false, IsTest = true)]
    public class HookedIntMuscariable : IntMuscariable
    {
        public object LastBasePrev;
        public int LastGenericPrev;
        public int BaseSetCount;
        public int GenericSetCount;


    }
}