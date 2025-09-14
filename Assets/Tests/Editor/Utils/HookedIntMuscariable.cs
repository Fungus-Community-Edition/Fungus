using Amanita.VScripting;

namespace Amanita.Tests.EditMode
{
    [VariableInfo("", "", typeof(int), ShowInMenu = false)]
    public class HookedIntMuscariable : Muscariable<int>
    {
        public object LastBasePrev;
        public int LastGenericPrev;
        public int BaseSetCount;
        public int GenericSetCount;

        protected override void OnBaseValueSet(object prevValue)
        {
            BaseSetCount++;
            LastBasePrev = prevValue;
            // Keep generic in sync
            valOfType = (int)(this.value);
        }

        protected override void OnGenericValueSet(int prev)
        {
            GenericSetCount++;
            LastGenericPrev = prev;
        }
    }
}