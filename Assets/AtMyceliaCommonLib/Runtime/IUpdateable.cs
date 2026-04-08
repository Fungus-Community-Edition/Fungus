namespace AtMycelia
{
    /// <summary>
    /// Interface for components which can be updated when the scene loads in 
    /// the editor. This is used to maintain backwards compatibility with 
    /// earlier versions of whatever systems said Components belong to.
    /// </summary>
    public interface IUpdateable
    {
        void UpdateToVersion(int oldVersion, int newVersion);
    }
}