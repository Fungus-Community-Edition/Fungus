using Amanita.DialogueSys;
using Amanita.Myceliaudio;
using Amanita.SaveSys;
using Amanita.Tweening;
using UnityEngine;

namespace Amanita
{
    /// <summary>
    /// Amanita manager singleton. Manages access to all Amanita singletons in a consistent manner.
    /// </summary>
    public sealed class AmanitaManager : MonoBehaviour
    {

        public static DefaultTweenAdapter DefaultTweener
        {
            get
            {
                if (_defaultTweener == null)
                {
                    _defaultTweener = Resources.Load<DefaultTweenAdapter>(pathToAdapter);
#if UNITY_EDITOR
                    if (_defaultTweener == null)
                    {
                        Debug.LogWarning($"No TweenAdapter found at Resources/{pathToAdapter}. Creating a new one.");
                        _defaultTweener = TweenAdapterUtility.GetOrCreateDefaultAdapter();
                    }
#else
                    if (_adapter == null)
                    {
                        _adapter = ScriptableObject.CreateInstance<DefaultTweenAdapter>();
                    }
#endif
                }

                return _defaultTweener;
            }
        }

        static DefaultTweenAdapter _defaultTweener;
        static string pathToAdapter = "DefaultTweenAdapter";

        // Best avoid setting up the singleton through the getter
        public static AmanitaManager EnsureExists()
        {
            if (_s == null)
            {
                AmanitaManager existing;
#if UNITY_6000
                existing = FindFirstObjectByType<AmanitaManager>();
#else
                existing = FindObjectOfType<AmanitaManager>();
#endif
                if (existing != null)
                {
                    _s = existing;
                    _s.Init(); 
                    return _s;
                }

                AmanitaManager prefab = Resources.Load<AmanitaManager>(AmanitaConstants.PathToAmanitaManagerPrefab);
                _s = null; // Since the Rasource.Load call triggers this class's Awake method
                _s = Instantiate(prefab);
                _s.gameObject.name = prefab.name; // We don't want "Clone" in the name.
                
                _s.Init();
            }

            return _s;
        }

        public void Init()
        {
            if (IsInitted)
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

            if (S == null)
            {
                Debug.LogError($"AmanitaManager's claim to the S field was ignored.");
            }

            PrepSubmodules();

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(_s.gameObject);
            }
            IsInitted = true;

        }

        private void PrepSubmodules()
        {
            // We assume that these are each on separate GameObjects (for the sake of easier testing)
            CameraManager = GetComponentInChildren<CameraManager>();
            EventDispatcher = GetComponentInChildren<EventDispatcher>();
            GlobalVariables = GetComponentInChildren<GlobalVariables>();
            MainAudioMixer = GetComponentInChildren<MainAudioMixer>();
            NarrativeLog = GetComponentInChildren<NarrativeLog>();
            AudioSystem = GetComponentInChildren<AudioSystem>();
            SaveSysInstaller = GetComponentInChildren<SaveSystemInstaller>();
            TweenManager = GetComponentInChildren<TweenManager>();

            InitAll();
            void InitAll()
            {
                // The order here matters
                TweenManager.Init();
                NarrativeLog.Init();
                MainAudioMixer.Init();
                AudioSystem.Init();
                GlobalVariables.Init();
                SaveSysInstaller.Init();
            }
        }

        private void Awake()
        {
            if (_s == null)
            {
                _s = this;
                if (Application.isPlaying)
                    DontDestroyOnLoad(gameObject);

                Init();
            }
            else if (_s != this)
            {
                if (!Application.isPlaying)
                {
                    // Since DestroyImmediate doesn't call OnDestroy...
                    OnDestroy();
                    if (AudioSystem != null)
                    {
                        AudioSystem?.OnDestroy();
                    }
                    DestroyImmediate(gameObject); // Prevents duplicates in edit mode
                }
                else
                    Destroy(gameObject);
                return;
            }
            else if (IsInitted == false)
            {
                Init();
            }
        }

        public bool IsInitted { get; private set; } = false;

        private SaveSystemInstaller SaveSysInstaller { get; set; }

        private TweenManager TweenManager { get; set; }
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
        public static AmanitaManager S
        {
            get => _s;
            set => _s = value;
        }

        volatile static AmanitaManager _s;  // The keyword "volatile" is friendly to the multi-thread.

        #endregion

        public static void ResetStaticsForTest()
        {
            S = null;
        }

        public AudioSystem AudioSystem { get; private set; }

        private void OnDestroy()
        {
            if (_s == this)
            {
                _s = null;
                IsInitted = false;

                SaveSystem.S = null;
            }
        }
    }
}