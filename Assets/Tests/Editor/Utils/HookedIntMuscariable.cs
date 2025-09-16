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

            // Ensure the generic field is synced with the base object field.
            // Call base implementation which performs: value = (T)base.value;
            base.OnBaseValueSet(prevValue);
        }

        protected override void OnGenericValueSet(int prev)
        {
            GenericSetCount++;
            LastGenericPrev = prev;
        }
    }
}