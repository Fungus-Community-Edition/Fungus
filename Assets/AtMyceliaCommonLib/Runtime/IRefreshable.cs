namespace AtMycelia
{
    public interface IRefreshable
    {
        void Refresh();
    }

    public interface IOnPreCutHandler
    {
        /// <summary>
        /// Meant to be executed right before this instance is to be cut. This is useful for 
        /// performing any necessary cleanup before the instance is removed from the scene.
        /// </summary>
        void OnPreCut();
    }
}