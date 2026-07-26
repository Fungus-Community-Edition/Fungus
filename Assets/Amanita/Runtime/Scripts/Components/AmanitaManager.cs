using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using AtMycelia.Amanita.DialogueSys;
#endif

namespace AtMycelia.Amanita
{
    /// <summary>
    /// Amanita manager singleton. Manages access to all Amanita singletons in a consistent manner.
    /// </summary>
    public sealed class AmanitaManager : MonoBehaviour, ITearDownResponder
    {
        public void Init()
        {
            if (IsFullyInitted ||
                this.gameObject.scene == default ||
                this.gameObject.scene.name == this.name) // <- This can happen when we're in prefab mode
            {
                return;
            }

            bool thisIsDuplicate = S != this && S != null;
            if (thisIsDuplicate)
            {
                Debug.Log("AmanitaManager instance already exists. Destroying the new one.");
                Destroy(this.gameObject);
                return;
            }

            _s = this;
            PrepSubmodules();
        }

        volatile static AmanitaManager _s;  // The keyword "volatile" is friendly to multi-threading.

        public bool IsFullyInitted
        {
            get => (NarrativeLog != null && NarrativeLog.IsFullyInitted);
        }

        private void PrepSubmodules()
        {
            // We assume that these are each on separate GameObjects (for the sake of easier testing)
            FetchSubmodules();
            void FetchSubmodules()
            {
                CameraManager = GetComponentInChildren<CameraManager>();
                EventDispatcher = gameObject.GetOrAddComponent<EventDispatcher>();
                NarrativeLog = GetComponentInChildren<NarrativeLog>();
            }

            List<IAmanitaManagerSubmodule> submodules = GetComponentsInChildren<IAmanitaManagerSubmodule>().ToList();
            // Lower order index, earlier execution
            submodules.Sort((first, second) => first.OrderIndex.CompareTo(second.OrderIndex));
            for (int i = 0; i < submodules.Count; i++)
            {
                var module = submodules[i];
                module.Init();
            }
        }

        private void Awake()
        {
            if (_s != null && _s != this)
            {
                CleanSelfUp();
                void CleanSelfUp()
                {
                    string logMessage = "AmanitaManager instance already exists. Destroying the new one.";
                    Debug.Log(logMessage);
                    if (!Application.isPlaying)
                    {
                        // Since DestroyImmediate doesn't call OnDestroy...
                        OnDestroy();
                        DestroyImmediate(this.gameObject); // Prevents duplicates in edit mode
                    }
                    else
                    {
                        Destroy(this.gameObject);
                    }
                }
                return;
            }
            
            _s = this;
            Init();

            if (Application.IsPlaying(this))
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        #region Public methods

        /// <summary>
        /// Gets the camera manager singleton instance.
        /// </summary>
        public CameraManager CameraManager { get; private set; }

        /// <summary>
        /// Gets the event dispatcher singleton instance.
        /// </summary>
        public EventDispatcher EventDispatcher { get; private set; }

        /// <summary>
        /// Gets the history manager singleton instance.
        /// </summary>
        public NarrativeLog NarrativeLog { get; private set; }
        
        /// <summary>
        /// Gets the FungusManager singleton instance.
        /// </summary>
        public static AmanitaManager S
        {
            get => _s;
            set => _s = value;
        }

        #endregion

        public static void ResetStaticsForTest()
        {
            S = null;
        }

        private void OnDestroy()
        {
            if (_s == this)
            {
                _s = null;
            }
        }

        private void OnValidate()
        {
            // OnValidate gets called on the prefab in response to Resources.Load(), so...
            if (!this.gameObject.scene.IsValid() || (S != null && S != this))
            {
                return;
            }
            S = this;

        }

        private void OnEnable()
        {
            ToggleSubs(true);
        }

        private void ToggleSubs(bool on)
        {
            // No-op for now
        }

#if UNITY_EDITOR
        public void OnTearDown()
        {
            List<ITearDownResponder> responders = new List<ITearDownResponder>();
            foreach (var submodule in GetComponentsInChildren<IAmanitaManagerSubmodule>())
            {
                if (submodule is ITearDownResponder responder)
                {
                    responders.Add(responder);
                }
            }
            foreach (var responder in responders)
            {
                responder.OnTearDown();
            }
        }
#endif

        private void OnDisable()
        {
            ToggleSubs(false);
        }
    }
}