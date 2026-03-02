namespace AtMycelia
{
    /// <summary>
    /// Implement this interface on any class that needs to respond to TearDown calls from test suites.
    /// </summary>
    public interface ITearDownResponder
    {
        /// <summary>
        /// Method called during TearDown phase of test suites.
        /// </summary>
        void OnTearDown();
    }
}