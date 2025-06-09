using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amanita.SaveSys
{
    public interface ISaveManager
    {
        Task SaveTo(int slotNumber);
        Task<CompositeSaveData> LoadMain(int slotNumber);
        Task DeleteSave(int slot);

        IList<int> GetOccupiedSlots();
        bool SlotExists(int slot);
    }
}