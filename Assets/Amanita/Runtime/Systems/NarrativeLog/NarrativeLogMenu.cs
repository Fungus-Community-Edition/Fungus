using UnityEngine;
using UnityEngine.UI;
using AtMycelia.Amanita.DialogueSys;
using AtMycelia.HyphaTween;
using UnityEngine.Serialization;

namespace AtMycelia.Amanita.UI.Legacy
{
    /// <summary>
    /// A singleton game object which displays a simple UI for the Narrative Log.
    /// </summary>
    public class NarrativeLogMenu : MonoBehaviour 
    {
        [Tooltip("Contains the overall aesthetic of each entry.")]
        [FormerlySerializedAs("entryDisplayPrefab")]
        [SerializeField] protected NarrativeLogEntryDisplay _entryDisplayPrefab;

        [Tooltip("Show the Narrative Log Menu")]
        [FormerlySerializedAs("showLog")]
        [SerializeField] protected bool _showLog = true;

        [Tooltip("Show previous lines instead of previous and current")]
        [FormerlySerializedAs("previousLines")]
        [SerializeField] protected bool _previousLines = true;

        [Tooltip("A scrollable text field used for displaying conversation history.")]
        [FormerlySerializedAs("narrativeLogView")]
        [SerializeField] protected ScrollRect _narrativeLogView;

        [Tooltip("Limit characters to be shown in Narrative Log")]
        [FormerlySerializedAs("maxCharacters")]
        [SerializeField] protected int _maxCharacters = 10000;

        protected TextAdapter narLogViewtextAdapter = new TextAdapter();
        
        [Tooltip("The CanvasGroup containing the save menu buttons")]
        [FormerlySerializedAs("narrativeLogMenuGroup")]
        [SerializeField] protected CanvasGroup _narrativeLogMenuGroup;

        protected static bool narrativeLogActive = false;
        
        protected AudioSource clickAudioSource;

        protected static NarrativeLogMenu instance;

        public bool PreviousLines
        {
            get => _previousLines;
            set => _previousLines = value;
        }

        protected virtual void Awake()
        {
            if (_showLog)
            {
                // Only one instance of NarrativeLogMenu may exist
                if (instance != null)
                {
                    Destroy(gameObject);
                    return;
                }

                instance = this;

                GameObject.DontDestroyOnLoad(this);

                clickAudioSource = GetComponent<AudioSource>();
            }
            else
            {
                GameObject logView = GameObject.Find("NarrativeLogView");
                logView.SetActive(false);
                this.enabled = false;
            }

            narLogViewtextAdapter.InitFromGameObject(_narrativeLogView.gameObject, true);
        }

        protected virtual void Start()
        {
            if (!narrativeLogActive)
            {
                _narrativeLogMenuGroup.alpha = 0f;
            }

            //Clear up the lorem ipsum
            UpdateVisuals();
        }

        protected virtual void OnEnable()
        {
            ToggleSubs(true);   
        }
              
        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                WriterSignals.OnWriterState += OnWriterState;
                NarrativeLog.OnNarrativeAdded += OnNarrativeAdded;
            }
            else
            {
                WriterSignals.OnWriterState -= OnWriterState;
                NarrativeLog.OnNarrativeAdded -= OnNarrativeAdded;
            }
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }

        protected virtual void OnNarrativeAdded(NarrativeLogEntry data)
        {
            UpdateVisuals();
        }

        protected virtual void OnWriterState(Writer writer, WriterState writerState)
        {
            if (writerState == WriterState.Start)
            {
                UpdateVisuals();
            }
        }

        protected virtual void OnSaveReset()
        {
            AmanitaManager.S.NarrativeLog.Clear();
            UpdateVisuals();
        }

        public virtual void UpdateVisuals()
        {
            if (_narrativeLogView.enabled)
            {
                var prettyHistory = AmanitaManager.S.NarrativeLog.GetPrettyHistory();

                if (prettyHistory.Length > _maxCharacters)
                {
                    prettyHistory = "... " + prettyHistory.Substring(prettyHistory.Length - _maxCharacters, _maxCharacters);
                }
                narLogViewtextAdapter.Text = prettyHistory;

                Canvas.ForceUpdateCanvases();
                _narrativeLogView.verticalNormalizedPosition = 0f;
                Canvas.ForceUpdateCanvases();
            }
        }

        protected void PlayClickSound()
        {
            if (clickAudioSource != null)
            {
                clickAudioSource.Play();
            }
        }

        #region Public methods
        protected Tween<float> _neoFadeTween;
        public virtual void ToggleNarrativeLogView()
        {
            //if (fadeTween != null)
            //{
            //    LeanTween.cancel(fadeTween.id, true);
            //    fadeTween = null;
            //}

            if (_neoFadeTween != null)
            {
                _neoFadeTween.OnCompleteKill();
                _neoFadeTween = null;
            }

            float targAlpha = narrativeLogActive ? 
                0 :
                100;
            if (narrativeLogActive)
            {
                // Switch menu off
                DefaultTweener.FadeOpacity(_narrativeLogMenuGroup, 0, _logMenuFadeDur);
            }
            else
            {
                // Switch menu on
                DefaultTweener.FadeOpacity(_narrativeLogMenuGroup, 1, _logMenuFadeDur);
            }

            _neoFadeTween = DefaultTweener.TweenBasic(() => _narrativeLogMenuGroup.alpha,
                    (newAlpha) => _narrativeLogMenuGroup.alpha = newAlpha,
                    targAlpha, _logMenuFadeDur)
                    .SetOnComplete(() => _narrativeLogMenuGroup.alpha = targAlpha);

            narrativeLogActive = !narrativeLogActive;
        }

        private static readonly float _logMenuFadeDur = 0.2f;

        private static DefaultTweenAdapter DefaultTweener => TweenManager.S.DefaultAdapter;

        #endregion
    }
}