using UnityEngine;

namespace Amanita.SaveSys
{
    public interface IMetaFactory
    {
        /// <summary>
        /// Create a new save metadata object for the given slot.
        /// </summary>
        /// Optional user-supplied name for the save (e.g. “My Hero’s Chapter 2”).
        /// </param>
        ISaveMetaData CreateMeta(int slotNumber, string saveName = "");
    }
}