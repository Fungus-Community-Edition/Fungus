// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;

namespace Amanita
{
    /// <summary>
    /// Fungus manager singleton. Manages access to all Fungus singletons in a consistent manner.
    /// </summary>
    public sealed class AmanitaManager : MonoBehaviour
    {
        volatile static AmanitaManager instance;  // The keyword "volatile" is friendly to the multi-thread.
        static bool applicationIsQuitting = false;
        readonly static object _lock = new object();  // The keyword "readonly" is friendly to the multi-thread.

        void Awake()
        {
            if (instance == null)
                instance = this;

            CameraManager = GetComponentInChildren<CameraManager>();
            EventDispatcher = GetComponentInChildren<EventDispatcher>();
            GlobalVariables = GetComponentInChildren<GlobalVariables>();
            MainAudioMixer = GetComponentInChildren<MainAudioMixer>();
#if UNITY_5_3_OR_NEWER
            SaveManager = GetComponentInChildren<SaveManager>();
            NarrativeLog = GetComponentInChildren<NarrativeLog>();
#endif
            MainAudioMixer.Init();
        }

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        void OnDestroy () 
        {
            applicationIsQuitting = true;
        }

        #region Public methods

        /// <summary>
        /// Gets the camera manager singleton instance.
        /// </summary>
        public CameraManager CameraManager { get; private set; }

        /// <summary>
        /// Gets the music manager singleton instance.
        /// </summary>
        public MusicManager MusicManager { get; private set; }

        /// <summary>
        /// Gets the event dispatcher singleton instance.
        /// </summary>
        public EventDispatcher EventDispatcher { get; private set; }

        /// <summary>
        /// Gets the global variable singleton instance.
        /// </summary>
        public GlobalVariables GlobalVariables { get; private set; }

        public MainAudioMixer MainAudioMixer { get; private set; }

#if UNITY_5_3_OR_NEWER
        /// <summary>
        /// Gets the save manager singleton instance.
        /// </summary>
        public SaveManager SaveManager { get; private set; }
        
        /// <summary>
        /// Gets the history manager singleton instance.
        /// </summary>
        public NarrativeLog NarrativeLog { get; private set; }
        
        #endif

        /// <summary>
        /// Gets the FungusManager singleton instance.
        /// </summary>
        public static AmanitaManager Instance
        {
            get
            {
                if (applicationIsQuitting) 
                {
                    Debug.LogWarning("FungusManager.Instance() was called while application is quitting. Returning null instead.");
                    return null;
                }

                // Use "double checked locking" algorithm to implement the singleton for this "FungusManager" class, which can improve performance.
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            AmanitaManager prefab = Resources.Load<AmanitaManager>(AmanitaConstants.PathToAmanitaManagerPrefab);
                            instance = Instantiate(prefab);
                            instance.gameObject.name = prefab.name; // We don't want "Clone" in the name.
                            DontDestroyOnLoad(instance.gameObject);
                        }

                    }
                }
                
                return instance;
            }
        }

        #endregion
    }
}