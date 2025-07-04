namespace Amanita.SaveSys
{
    public interface IMetaFactory
    {
        /// <summary>
        /// Create a new save metadata object for the given slot.
        /// </summary>
        ISaveMetaData CreateMeta(int slotNumber);
    }
}