using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Handles restoring game state.
    /// </summary>
    public class SaveLoader : ISaveLoader
    {
        public SaveLoader(IList<IMainSaveCodec> codecList)
        {
            if (codecList == null)
            {
                string errorMessage = "Cannot initialize a SaveLoader with a null codec list.";
                throw new ArgumentNullException(errorMessage);
            }

            this.mainCodecs = codecList;
        }

        public virtual void AddRange(IList<IMainSaveCodec> codecs)
        {
            for (int i = 0; i < codecs.Count; i++)
            {
                Add(codecs[i]);
            }
        }

        public virtual void Add(IMainSaveCodec codec)
        {
            if (!mainCodecs.Contains(codec))
            {
                mainCodecs.Add(codec);
            }
        }

        protected IList<IMainSaveCodec> mainCodecs;

        public virtual async Task LoadMain(CompositeSaveData mainData,
            Scene sceneToLoad,
            CancellationToken token = default)
        {
            if (mainData == null)
            {
                throw new ArgumentNullException(nameof(mainData), "Main Save data cannot be null.");
            }

            await HandleSceneLoading(sceneToLoad);
            async Task HandleSceneLoading(Scene scene)
            {
                bool shouldLoadScene = scene.IsValid() && !scene.Equals(DoNotLoad);

                if (shouldLoadScene)
                {
                    AsyncOperation loadOperation = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Single);
                    await loadOperation;
                }
                else
                {
                    await Task.CompletedTask;
                }
            }
            
            IList<SaveData> decodedUnits = GetDecodedUnits();
            IList<SaveData> GetDecodedUnits()
            {
                IList<SaveData> result = new List<SaveData>();
                foreach (SaveDataUnit unitEl in mainData.Units)
                {
                    string typeName = unitEl.DataTypeName;
                    IMainSaveCodec codecForThisUnit = mainCodecs.FirstOrDefault(codec => codec.CanHandle(typeName));

                    if (codecForThisUnit != null)
                    {
                        SaveData decodedSave = codecForThisUnit.DecodeFrom(unitEl);
                        result.Add(decodedSave);
                    }
                }
                return result;
            }

            await ApplyUnitsToScene(decodedUnits);
            async Task ApplyUnitsToScene(IList<SaveData> unitsToApply)
            {
                var appliers = SaveSystem.S.SaveDataAppliers;

                foreach (ISaveDataApplier applierEl in appliers)
                {
                    IList<SaveData> unitsItCanWorkWith = (from elem in decodedUnits
                                                          where applierEl.CanApply(elem)
                                                          select elem).ToList();
                    if (unitsItCanWorkWith.Count == 0)
                    {
                        continue;
                    }

                    await applierEl.ApplyRange(unitsItCanWorkWith);
                }

            }

        }

        protected static Scene DoNotLoad { get { return SaveSysConstants.DoNotLoad; } }
    }

    public interface ISaveLoader
    {
        Task LoadMain(CompositeSaveData mainData,
            Scene sceneToLoad,
            CancellationToken token = default);
    }
}