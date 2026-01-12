using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Collections;
using System.Runtime.InteropServices;

namespace Amanita.DialogueSys
{     
    /// <summary>
    /// Manages Say Dialogs within the dialogue system.
    /// </summary>
    [AddComponentMenu("Amanita/Dialogue System/Say Dialog Manager")]
    public class SayDialogManager : MonoBehaviour, IAmanitaManagerSubmodule
    {
        [SerializeField] private int orderIndex = 0;
        public int OrderIndex => orderIndex;

        /// <summary>
        /// Returns (a copy of) the list of currently active Say Dialogs.
        /// </summary>
        public IList<SayDialog> ActiveSayDialogs
        {
            get => new List<SayDialog>(activeSayDialogs);
        }

        private readonly HashSet<SayDialog> activeSayDialogs = new HashSet<SayDialog>();
        // ^Might want to have multiple active say dialogs in future for split-screen
        // or multiple characters talking simultaneously. Think
        // Paper Mario: The Thousand-Year Door.

        public SayDialog MainSayDialog
        {
            get => mainSayDialog;
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

        public void Init()
        {
            if (IsFullyInitted)
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

            RegisterSayDialogs();
            void RegisterSayDialogs()
            {
                sayDialogs.Clear();
                activeSayDialogs.Clear();

                var foundDialogs = FindObjectsByType<SayDialog>(FindObjectsSortMode.None).ToList();
                sayDialogs.AddRange(foundDialogs);

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
                for (int i = sayDialogs.Count - 1; i >= 0; i--)
                {
                    var sd = sayDialogs.ElementAt(i);
                    if (sd == null)
                    {
                        sayDialogs.Remove(sd);
                    }

                    sd.transform.SetParent(holdsSayDialogs, false);
                }
            }

            IsFullyInitted = true;
        }

        public virtual bool IsFullyInitted { get; private set; } = false;

        public static SayDialogManager S
        {
            get => s;
            set => s = value;
        }
        private static SayDialogManager s;
        [SerializeField] private Transform holdsSayDialogs;
        private readonly HashSet<SayDialog> sayDialogs = new HashSet<SayDialog>();

        public SayDialog CreateSD(SayDialog prefab)
        {
            SayDialog newSD = Instantiate(prefab, holdsSayDialogs);
            newSD.name = prefab.name;
            sayDialogs.Add(newSD);
            return newSD;
        }

    }
}

