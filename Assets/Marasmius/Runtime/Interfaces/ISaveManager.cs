using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AtMycelia.SaveSys
{
    public interface ISaveManager
    {
        Task Init();
        Task SaveToSlotAsync(int slotNumber, CancellationToken token = default);
        Task<CompositeSaveData> LoadMainAsync(int slotNumber, bool loadScene, CancellationToken token = default);
        Task<ISaveMetaData> LoadMetaAsync(int slotNumber, CancellationToken token = default);
        void DeleteSave(int slot);

        IList<int> GetOccupiedSlots();
        bool SlotExists(int slot);


        // Dependencies
        ISaveRepository SaveRepo { get; set; }
        SaveRegistry Registry { get; set; }
        SaveLoader Loader { get; set; }
        IMetaFactory MetaFactory { get; set; }
        IMainStateFactory MainStateFactory { get; set; }
        SaveDirectoryType SaveDirType { get; set; }
    }
}