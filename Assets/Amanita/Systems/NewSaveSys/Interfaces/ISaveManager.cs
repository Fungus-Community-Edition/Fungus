using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Amanita.SaveSys
{
    public interface ISaveManager
    {
        Task Init();
        Task SaveTo(int slotNumber, CancellationToken token = default);
        Task<CompositeSaveData> LoadMain(int slotNumber, bool loadScene, CancellationToken token = default);
        Task<ISaveMetaData> LoadMeta(int slotNumber, CancellationToken token = default);
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