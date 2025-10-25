using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amanita.SaveSys
{
    public class DefaultMainStateFactory : IMainStateFactory
    {
        public DefaultMainStateFactory(IList<ISaveDataApplier> appliers, IList<IMainSaveCodec> mainCodecs)
        {
            Validate(appliers, mainCodecs);
            static void Validate(IList<ISaveDataApplier> appliers, IList<IMainSaveCodec> mainCodecs)
            {
                if (appliers == null)
                {
                    string errorMessage = "Cannot init a DefaultMainStateFactory with a null appliers list.";
                    throw new System.ArgumentNullException(nameof(appliers), errorMessage);
                }

                if (mainCodecs == null)
                {
                    string errorMessage = "Cannot init a DefaultMainStateFactory with a null main codec list.";
                    throw new System.ArgumentNullException(nameof(mainCodecs), errorMessage);
                }
            }

            this.appliers = appliers;
            this.mainCodecs = mainCodecs;
        }

        public void AddRange(IList<ISaveDataApplier> allToAdd)
        {
            for (int i = 0; i < allToAdd.Count; i++)
            {
                var toAdd = allToAdd[i];
                Add(toAdd);
            }
        }

        public void Add(ISaveDataApplier toAdd)
        {
            if (toAdd == null)
            {
                string errorMessage = "Cannot register null applier.";
                throw new System.ArgumentNullException(errorMessage);
            }

            appliers.Add(toAdd);
        }

        protected IList<ISaveDataApplier> appliers;

        public void AddRange(IList<IMainSaveCodec> allToAdd)
        {
            for (int i = 0; i < allToAdd.Count; i++)
            {
                var codec = allToAdd[i];
                Add(codec);
            }
        }

        public void Add(IMainSaveCodec toAdd)
        {
            if (toAdd == null)
            {
                string errorMessage = "Cannot register null codec.";
                throw new System.ArgumentNullException(errorMessage);
            }

            mainCodecs.Add(toAdd);
        }

        protected IList<IMainSaveCodec> mainCodecs = new List<IMainSaveCodec>();

        public virtual async Task<CompositeSaveData> CreateMainState()
        {
            IList<SaveData> itemsNeeded = await GetItemsForGameState();
            async Task<IList<SaveData>> GetItemsForGameState()
            {
                IList<SaveData> items = new List<SaveData>();

                for (int i = 0; i < mainCodecs.Count; i++)
                {
                    IMainSaveCodec currentCodec = mainCodecs[i];

                    await Task.Run(() => currentCodec.FindAndCreateAll(RegisterResults));

                    void RegisterResults(IList<SaveData> results)
                    {
                        for (int j = 0; j < results.Count; j++)
                        {
                            var resultEl = results[j];
                            if (resultEl != null)
                            {
                                items.Add(resultEl);
                            }
                        }
                    }
                    
                }

                return items;
            }

            CompositeSaveData mainState = new CompositeSaveData(itemsNeeded);
            return mainState;
        }

        public void RemoveRange(IList<ISaveDataApplier> allToRemove)
        {
            for (int i = 0; i < allToRemove.Count; i++)
            {
                var toRemove = allToRemove[i];
                Remove(toRemove);
            }
        }

        public void Remove(ISaveDataApplier applier)
        {
            appliers.Remove(applier);
        }

        public void RemoveRange(IList<IMainSaveCodec> codecs)
        {
            for (int i = 0; i < codecs.Count; i++)
            {
                var toRemove = codecs[i];
                Remove(toRemove);
            }
        }

        public void Remove(IMainSaveCodec codec)
        {
            mainCodecs.Remove(codec);
        }

        public void RemoveAllAppliers()
        {
            appliers.Clear();
        }

        public void RemoveAllCodecs()
        {
            mainCodecs.Clear();
        }
    }
}