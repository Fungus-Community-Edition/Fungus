using AtMycelia.Collections;
using System.Collections.Generic;
using AtMycelia.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AtMycelia.Amanita.DialogueSys
{     
    /// <summary>
    /// Manages Say Dialogs within the dialogue system.
    /// </summary>
    [AddComponentMenu("Amanita/Dialogue System/Say Dialog Manager")]
    public class SayDialogManager : MonoBehaviour, IAmanitaManagerSubmodule
    {
        [SerializeField] private int orderIndex = 0;
        [SerializeField] private Transform holdsSayDialogs;
        public int OrderIndex => orderIndex;
        
        public void Init()
        {
            #region Validation
            if (IsFullyInitted || !Application.isPlaying)
            {
                return;
            }

            if (s != null && s != this)
            {
                Destroy(this.gameObject);
                return;
            }

            s = this;

            if (holdsSayDialogs == null)
            {
                GameObject holderGo = new GameObject("Say Dialogs");
                holderGo.transform.SetParent(this.transform, false);
                holdsSayDialogs = holderGo.transform;
            }
            #endregion

            RegisterSayDialogs();
            void RegisterSayDialogs()
            {
                sayDialogsInScene.Clear();
                activeSayDialogs.Clear();

                var foundDialogs = FindObjectsByType<SayDialog>(FindObjectsSortMode.None).ToList();
                sayDialogsInScene.AddRange(foundDialogs);

                foreach (var found in foundDialogs)
                {
                    if (found.gameObject.activeInHierarchy)
                    {
                        activeSayDialogs.Add(found);
                    }
                }
            }

            GatherSayDialogs();
            void GatherSayDialogs()
            {
                for (int i = sayDialogsInScene.Count - 1; i >= 0; i--)
                {
                    var sd = sayDialogsInScene.ElementAt(i);
                    if (sd == null)
                    {
                        sayDialogsInScene.Remove(sd);
                    }

                    sd.transform.SetParent(holdsSayDialogs, false);
                }
            }

            activeSayDialogsRO = new ReadOnlyHashSet<SayDialog>(activeSayDialogs);
            sayDialogsInSceneRO = new ReadOnlyHashSet<SayDialog>(sayDialogsInScene);
            IsFullyInitted = true;
        }

        public virtual bool IsFullyInitted { get; private set; } = false;

        public static SayDialogManager S
        {
            get => s;
            set => s = value;
        }
        private static SayDialogManager s;

        // We're using HashSets instead of lists to:
        // 1. Prevent duplicates automatically.
        // 2. Speed up lookups if needed.
        private readonly HashSet<SayDialog> sayDialogsInScene = new HashSet<SayDialog>();
        private readonly HashSet<SayDialog> activeSayDialogs = new HashSet<SayDialog>();
        
        // ^Might want to have multiple active say dialogs in future for split-screen
        // or multiple characters talking simultaneously. Think
        // Paper Mario: The Thousand-Year Door.
        private IReadOnlyHashSet<SayDialog> activeSayDialogsRO;

        public IReadOnlyHashSet<SayDialog> SayDialogsInScene
        {
            get => sayDialogsInSceneRO;
        }
        
        private IReadOnlyHashSet<SayDialog> sayDialogsInSceneRO;

        /// <summary>
        /// Returns an existing instance of the specified SayDialog prefab if one exists; otherwise, creates a new
        /// instance and returns it.
        /// </summary>
        /// <remarks>If a new SayDialog instance is created, it is added to the internal collection and an
        /// event is raised to signal its creation. Subsequent calls with the same prefab will return the same
        /// instance.</remarks>
        /// <param name="prefab">The SayDialog prefab to retrieve or instantiate. Cannot be null.</param>
        public SayDialog GetOrCreateSD(SayDialog prefab)
        {
            if (prefabToInstanceMap.TryGetValue(prefab, out SayDialog existingInstance))
            {
                return existingInstance;
            }

            return CreateSD(prefab);
        }

        public SayDialog CreateSD(SayDialog prefab)
        {
            SayDialog newSD = Instantiate(prefab, holdsSayDialogs);
            newSD.name = prefab.name;
            sayDialogsInScene.Add(newSD);
            DialogueSysSignals.SayDialogMadeFromPrefab.Invoke(prefab, newSD); // This should add to the map
            return newSD;
        }


        protected virtual void OnEnable()
        {
            ToggleSubs(true);
        }

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                DialogueSysSignals.SayDialogEnabled += OnSayDialogEnabled;
                DialogueSysSignals.SayDialogDisabled += OnSayDialogDisabled;
                DialogueSysSignals.SayDialogMadeFromPrefab += OnSayDialogMadeFromPrefab;
            }
            else
            {
                DialogueSysSignals.SayDialogEnabled -= OnSayDialogEnabled;
                DialogueSysSignals.SayDialogDisabled -= OnSayDialogDisabled;
                DialogueSysSignals.SayDialogMadeFromPrefab -= OnSayDialogMadeFromPrefab;
            }
        }

        private void OnSayDialogEnabled(SayDialog dialog)
        {
            activeSayDialogs.Add(dialog);

            // Make sure it is parented to the holder
            if (dialog.transform.parent != holdsSayDialogs)
            {
                dialog.transform.SetParent(holdsSayDialogs, false);
            }
        }

        private void OnSayDialogDisabled(SayDialog dialog)
        {
            activeSayDialogs.Remove(dialog);
        }

        private void OnSayDialogMadeFromPrefab(SayDialog prefab, SayDialog instance)
        {
            prefabToInstanceMap[prefab] = instance;
        }

        private readonly IDictionary<SayDialog, SayDialog> prefabToInstanceMap = new Dictionary<SayDialog, SayDialog>();

        /// <summary>
        /// These are the Say Dialogs that are currently enabled in the scene.
        /// </summary>
        public IReadOnlyHashSet<SayDialog> ActiveSayDialogs
        {
            get => activeSayDialogsRO;
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }

        /// <summary>
        /// The one used to display main dialogue lines (as opposed to background chatter like in
        /// Paper Mario: The Thousand-Year Door). If none exists at the time of getting, this will
        /// either search for one in the scene or create one based on the default prefab. 
        /// </summary>
        public SayDialog MainSayDialog
        {
            get
            {
                if (mainSayDialog != null)
                {
                    return mainSayDialog;
                }

                if (sayDialogsInScene.Count > 0)
                {
                    mainSayDialog = sayDialogsInScene.First();
                    return mainSayDialog;
                }

                Debug.Log("No Say Dialogs found in scene. Creating one from default prefab.");
                SayDialog prefab = Resources.Load<SayDialog>("Prefabs/SayDialog");
                if (prefab != null)
                {
                    mainSayDialog = Instantiate(prefab);
                    mainSayDialog.name = prefab.name;
                    mainSayDialog.transform.SetParent(holdsSayDialogs, false);
                }

                return mainSayDialog;
            }
            set
            {
                if (value == null)
                {
                    Debug.LogError("Cannot set Main Say Dialog to null.");
                    return;
                }

                bool isInScene = value.gameObject.scene != default;
                if (isInScene)
                {
                    mainSayDialog = value;
                }
                else
                {
                    mainSayDialog = CreateSD(value);
                }
            }
        }

        private SayDialog mainSayDialog;

    }


}

