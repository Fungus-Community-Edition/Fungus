using UnityEngine;
using UnityEngine.UI;
using Amanita.VScripting;
using Amanita.DialogueSys;
using Amanita.Tweening;

namespace Amanita.UI.Legacy
{
    /// <summary>
    /// A singleton game object which displays a simple UI for the Narrative Log.
    /// </summary>
    public class NarrativeLogMenu : MonoBehaviour 
    {
        [Tooltip("Contains the overall aesthetic of each entry.")]
        [SerializeField] protected NarrativeLogEntryDisplay entryDisplayPrefab;

        [Tooltip("Show the Narrative Log Menu")]
        [SerializeField] protected bool showLog = true;

        [Tooltip("Show previous lines instead of previous and current")]
        [SerializeField] protected bool previousLines = true;

        [Tooltip("A scrollable text field used for displaying conversation history.")]
        [SerializeField] protected ScrollRect narrativeLogView;

        [Tooltip("Limit characters to be shown in Narrative Log")]
        [SerializeField] protected int maxCharacters = 10000;

        protected TextAdapter narLogViewtextAdapter = new TextAdapter();
        
        [Tooltip("The CanvasGroup containing the save menu buttons")]
        [SerializeField] protected CanvasGroup narrativeLogMenuGroup;

        protected static bool narrativeLogActive = false;
        
        protected AudioSource clickAudioSource;

        protected static NarrativeLogMenu instance;

        protected virtual void Awake()
        {
            if (showLog)
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

            narLogViewtextAdapter.InitFromGameObject(narrativeLogView.gameObject, true);
        }

        protected virtual void Start()
        {
            if (!narrativeLogActive)
            {
                narrativeLogMenuGroup.alpha = 0f;
            }

            //Clear up the lorem ipsum
            UpdateNarrativeLogText();
        }

        protected virtual void OnEnable()
        {
            WriterSignals.OnWriterState += OnWriterState;
            BlockSignals.OnBlockEnd += OnBlockEnd;
            NarrativeLog.OnNarrativeAdded += OnNarrativeAdded;
        }
                
        protected virtual void OnDisable()
        {
            WriterSignals.OnWriterState -= OnWriterState;
            BlockSignals.OnBlockEnd -= OnBlockEnd;
            NarrativeLog.OnNarrativeAdded -= OnNarrativeAdded;
        }

        protected virtual void OnNarrativeAdded(NarrativeLogEntry data)
        {
            UpdateNarrativeLogText();
        }

        protected virtual void OnWriterState(Writer writer, WriterState writerState)
        {
            if (writerState == WriterState.Start)
            {
                UpdateNarrativeLogText();
            }
        }

        protected virtual void OnSavePointLoaded(string savePointKey)
        {
            UpdateNarrativeLogText();
        }

        protected virtual void OnSaveReset()
        {
            AmanitaManager.S.NarrativeLog.Clear();
            UpdateNarrativeLogText();
        }

        protected virtual void OnBlockEnd (Block block)
        {
            // At block end update to get the last line of the block
            bool defaultPreviousLines = previousLines;
            previousLines = false;
            UpdateNarrativeLogText();
            previousLines = defaultPreviousLines;
        }

        protected void UpdateNarrativeLogText()
        {
            if (narrativeLogView.enabled)
            {
                var prettyHistory = AmanitaManager.S.NarrativeLog.GetPrettyHistory();

                if (prettyHistory.Length > maxCharacters)
                {
                    prettyHistory = "... " + prettyHistory.Substring(prettyHistory.Length - maxCharacters, maxCharacters);
                }
                narLogViewtextAdapter.Text = prettyHistory;

                Canvas.ForceUpdateCanvases();
                narrativeLogView.verticalNormalizedPosition = 0f;
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

            float targAlpha, duration = 0.2f;
            if (narrativeLogActive)
            {
                // Switch menu off
                //LeanTween.value(narrativeLogMenuGroup.gameObject, narrativeLogMenuGroup.alpha, 0f, .2f)
                //    .setEase(LeanTweenType.easeOutQuint)
                //    .setOnUpdate((t) => {
                //    narrativeLogMenuGroup.alpha = t;
                //}).setOnComplete(() => {
                //    narrativeLogMenuGroup.alpha = 0f;
                //});
                targAlpha = 0f;
                
            }
            else
            {
                // Switch menu on
                //LeanTween.value(narrativeLogMenuGroup.gameObject, narrativeLogMenuGroup.alpha, 1f, .2f)
                //    .setEase(LeanTweenType.easeOutQuint)
                //    .setOnUpdate((t) => {
                //    narrativeLogMenuGroup.alpha = t;
                //}).setOnComplete(() => {
                //    narrativeLogMenuGroup.alpha = 1f;
                //});

                targAlpha = 1;
            }

            _neoFadeTween = AmanitaManager.DefaultTweener.TweenBasic<float>(() => narrativeLogMenuGroup.alpha,
                    (newAlpha) => narrativeLogMenuGroup.alpha = newAlpha,
                    targAlpha, duration)
                    .SetOnComplete(() => narrativeLogMenuGroup.alpha = targAlpha);

            narrativeLogActive = !narrativeLogActive;
        }
    
        #endregion
    }
}