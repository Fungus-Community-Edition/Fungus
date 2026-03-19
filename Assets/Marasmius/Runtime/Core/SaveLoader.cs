using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AtMycelia.SaveSys
{
    /// <summary>
    /// Top-level module for restoring game state.
    /// </summary>
    public class SaveLoader : IMainSaveLoader
    {
        public SaveLoader(IList<IMainSaveCodec> codecList)
        {
            if (codecList == null)
            {
                string errorMessage = "Cannot initialize a SaveLoader with a null codec list.";
                throw new ArgumentNullException(errorMessage);
            }

            // Codecs no longer used for load; retained for constructor compatibility.
        }

        public virtual async Task LoadMain(CompositeSaveData mainData,
            Scene sceneToLoad,
            CancellationToken token = default)
        {
            if (mainData == null)
            {
                throw new ArgumentNullException(nameof(mainData), "Main Save data cannot be null.");
            }

            Debug.Log($"About to load scene named: {sceneToLoad.name}");
            await HandleSceneLoading(sceneToLoad);
            async Task HandleSceneLoading(Scene scene)
            {
                bool shouldLoadScene = scene.IsValid() && !scene.Equals(DoNotLoad);

                if (shouldLoadScene)
                {
                    AsyncOperation loadOperation = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Single);
                    await loadOperation;
                    UnityThreadUtil.RunOnMainThread(() => AnnounceSceneLoad(scene));
                }
                else
                {
                    await Task.CompletedTask;
                }
            }

            // Option 2: Items are already concrete SaveData; no codec routing/decoding.
            IReadOnlyList<SaveData> itemsToApply = mainData.Items;

            await ApplyItemsToScene(itemsToApply);
            async Task ApplyItemsToScene(IReadOnlyList<SaveData> items)
            {
                var appliers = SaveSystem.SaveDataAppliers;

                foreach (ISaveDataApplier applierEl in appliers)
                {
                    IList<SaveData> compatible = (from elem in items
                                                  where applierEl.CanApply(elem)
                                                  select elem).ToList();
                    if (compatible.Count == 0)
                    {
                        continue;
                    }

                    bool doneApplying = false;
                    applierEl.ApplyRange(compatible, () => doneApplying = true);

                    while (!doneApplying)
                    {
                        await Task.Yield();
                    }
                }
            }

            SaveSysSignals.SaveLoaded(mainData);
        }

        private void AnnounceSceneLoad(Scene scene)
        {
            SaveSysSignals.SceneLoaded?.Invoke(scene);
        }
        protected static Scene DoNotLoad { get { return SaveSysConstants.DoNotLoad; } }
    }

    public interface IMainSaveLoader
    {
        Task LoadMain(CompositeSaveData mainData,
            Scene sceneToLoad,
            CancellationToken token = default);
    }
}