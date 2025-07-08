using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Responsible for creating save data for the game's main state
    /// (as opposed to the meta).
    /// </summary>
    public interface IMainStateFactory
    {
        Task<CompositeSaveData> CreateMainState();

        // We require the below methods so that client code can still easily add
        // and remove appliers to/from the system
        void AddRange(IList<ISaveDataApplier> appliers);
        void Add(ISaveDataApplier applier);
        void AddRange(IList<IMainSaveCodec> codecs);
        void Add(IMainSaveCodec codec);

        void RemoveRange(IList<ISaveDataApplier> appliers);
        void Remove(ISaveDataApplier applier);
        void RemoveRange(IList<IMainSaveCodec> codecs);
        void Remove(IMainSaveCodec codec);

        void RemoveAllAppliers();
        void RemoveAllCodecs();

    }
}