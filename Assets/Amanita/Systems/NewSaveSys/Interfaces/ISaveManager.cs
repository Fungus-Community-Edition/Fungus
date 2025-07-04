using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Amanita.SaveSys
{
    public interface ISaveManager
    {
        Task SaveTo(int slotNumber, CancellationToken token = default);
        Task<CompositeSaveData> LoadMain(int slotNumber, bool loadScene, CancellationToken token = default);
        void DeleteSave(int slot);

        IList<int> GetOccupiedSlots();
        bool SlotExists(int slot);

        void RegisterMainCodec(IMainSaveCodec codec);
        void RegisterMultiMainCodecs(IList<IMainSaveCodec> codecs);

        // Dependencies
        ISaveRepository SaveRepo { get; set; }
        SaveRegistry Registry { get; set; }
        SaveLoader Loader { get; set; }
        IMetaFactory MetaFactory { get; set; }
        IMainStateFactory MainStateFactory { get; set; }
        SaveDirectoryType SaveDirType { get; set; }
    }
}