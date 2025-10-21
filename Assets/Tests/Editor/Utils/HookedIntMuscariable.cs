using Amanita.VScripting;

namespace VScriptingTests.VariableOperations
{
    [VariableInfo("", "", typeof(int), ShowInMenu = false)]
    public class HookedIntMuscariable : Muscariable<int>
    {
        public object LastBasePrev;
        public int LastGenericPrev;
        public int BaseSetCount;
        public int GenericSetCount;


        protected override void OnGenericValueSet(int prev)
        {
            GenericSetCount++;
            LastGenericPrev = prev;
        }
    }
}