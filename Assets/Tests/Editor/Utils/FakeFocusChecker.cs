namespace AtMycelia.Hyphlow.EditorUtils
{
    // Fake focus checker you use in all tests
    public class FakeFocusChecker : IFocusChecker
    {
        public bool IsFocused = false;

        public bool CheckFocus(FlowchartContext ctx) => IsFocused;
    }
}