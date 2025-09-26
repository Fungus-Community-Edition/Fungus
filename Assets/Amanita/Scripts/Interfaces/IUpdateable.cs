namespace Amanita
{
    /// <summary>
    /// Interface for Flowchart components which can be updated when the 
    /// scene loads in the editor. This is used to maintain backwards 
    /// compatibility with earlier versions of Amanita.
    /// </summary>
    interface IUpdateable
    {
        void UpdateToVersion(int oldVersion, int newVersion);
    }
}