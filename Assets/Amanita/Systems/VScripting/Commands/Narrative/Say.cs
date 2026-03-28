using UnityEngine;
using AtMycelia.Amanita.VScripting;
using UnityEngine.Serialization;
using UnityEditor;

namespace AtMycelia.Amanita.DialogueSys.VScripting
{
    /// <summary>
    /// Writes text in a dialog box.
    /// </summary>
    [CommandInfo("Narrative", 
                 "Say", 
                 "Writes text in a dialog box.")]
    [AddComponentMenu("")]
    public class Say : Command, ILocalizable, ISerializationCallbackReceiver
    {
        // Removed this tooltip as users's reported it obscures the text box
        [HyphlowTextArea(5, 10)]
        [SerializeField] protected StringData _storyText = new StringData("");

        [HyphlowTextArea(1, 3)]
        [Tooltip("Notes about this story text for other authors, localization, etc.")]
        [SerializeField] protected StringData _description = new StringData("");

        [Tooltip("Character that is speaking")]
        [SerializeField] protected GameObjectData _character = new GameObjectData();

        [Tooltip("Portrait that represents speaking character")]
        [SerializeField] protected SpriteData _portrait = new SpriteData();

        [Tooltip("Voiceover audio to play when writing the text")]
        [SerializeField] protected AudioClipData _voiceOverClip = new AudioClipData();

        [Tooltip("Always show this Say text when the command is executed multiple times")]
        [SerializeField] protected BooleanData _showAlways = new BooleanData(true);

        [Tooltip("Number of times to show this Say text when the command is executed multiple times")]
        [SerializeField] protected IntegerData _showCount = new IntegerData(1);

        [Tooltip("Type this text in the previous dialog box.")]
        [SerializeField] protected BooleanData _extendPrevious = new BooleanData(false);

        [Tooltip("Fade out the dialog box when writing has finished and not waiting for input.")]
        [SerializeField] protected BooleanData _fadeWhenDone = new BooleanData(true);

        [Tooltip("Wait for player to click before continuing.")]
        [SerializeField] protected BooleanData _waitForClick = new BooleanData(true);

        [Tooltip("Stop playing voiceover when text finishes writing.")]
        [SerializeField] protected BooleanData _stopVoiceover = new BooleanData(true);

        [Tooltip("Wait for the Voice Over to complete before continuing")]
        [SerializeField] protected BooleanData _waitForVO = new BooleanData(false);

        //add wait for vo that overrides stopvo

        [Tooltip("Sets the active Say dialog with a reference to a Say Dialog object in the scene. All story text will now display using this Say Dialog.")]
        
        [SerializeField] protected GameObjectData _setSayDialog;

        protected int executionCount;


        /// <summary>
        /// Character that is speaking.
        /// </summary>
        public virtual Character Character { get { return _character.GetComponent<Character>(); } }
        [SerializeField] [HideInInspector] private Character _characterCached;
        /// <summary>
        /// Portrait that represents speaking character.
        /// </summary>
        public virtual Sprite Portrait { get { return _portrait; } set { _portrait.Value = value; } }

        /// <summary>
        /// Type this text in the previous dialog box.
        /// </summary>
        public virtual bool ExtendPrevious { get { return _extendPrevious; } }

        public override void OnEnter()
        {
            #region Input Validation
            if (_setSayDialog != null && _setSayDialog.Value != null)
            {
                if (!_setSayDialog.TryGetComponent(out SayDialog _))
                {
                    Debug.LogError($"Say Command on {gameObject.name} has invalid Set " +
                        $"Say Dialog input.", this);
                    Continue();
                    return;
                }
            }
            #endregion

            if (!_showAlways && executionCount >= _showCount)
            {
                Continue();
                return;
            }

            executionCount++;
            SayDialog prevMain = SDManager.MainSayDialog;

            OverrideActiveSayDialogAsNeeded();
            void OverrideActiveSayDialogAsNeeded()
            {
                bool shouldGoWithCharacterSetDialog = _characterCached != null && 
                    _characterCached.SetSayDialog != null;
                
                if (shouldGoWithCharacterSetDialog)
                {
                    var charaDialog = _characterCached.SetSayDialog;
                    bool itIsPrefab = charaDialog.gameObject.scene == default;
                    var prevMain = SDManager.MainSayDialog;
                    if (itIsPrefab)
                    {
                        SDManager.MainSayDialog = SDManager.GetOrCreateSD(_characterCached.SetSayDialog);
                    }
                    else
                    {
                        SDManager.MainSayDialog = charaDialog;
                    }

                }

                bool shouldGoWithCommandSetDialog = _setSayDialog != null && _setSayDialog.Value != null;
                // ^Higher priority than the character's set dialog
                if (shouldGoWithCommandSetDialog)
                {
                    var dialogComp = _setSayDialog.GetComponent<SayDialog>();
                    bool itIsPrefab = dialogComp.gameObject.scene == default;
                    if (itIsPrefab)
                    {
                        SDManager.MainSayDialog = SDManager.GetOrCreateSD(dialogComp);
                    }
                    else
                    {
                        SDManager.MainSayDialog = dialogComp;
                    }
                }
            }

            HidePrevMainDialogIfChanged();
            void HidePrevMainDialogIfChanged()
            {
                var currMain = SDManager.MainSayDialog;
                if (prevMain != currMain)
                {
                    prevMain.gameObject.SetActive(false);
                }
            }
            
            var sayDialog = SDManager.MainSayDialog;

            if (sayDialog == null)
            {
                string errorMessage = "No Say Dialog found to display text.";
                Debug.LogError(errorMessage, this);
                Continue();
                return;
            }
    
            var flowchart = GetFlowchart();

            sayDialog.SetActive(true);

            sayDialog.SetCharacter(_characterCached);
            sayDialog.SetCharacterImage(_portrait);

            string displayText = _storyText;

            var activeCustomTags = CustomTag.activeCustomTags;
            for (int i = 0; i < activeCustomTags.Count; i++)
            {
                var ct = activeCustomTags[i];
                displayText = displayText.Replace(ct.TagStartSymbol, ct.ReplaceTagStartWith);
                if (ct.TagEndSymbol != "" && ct.ReplaceTagEndWith != "")
                {
                    displayText = displayText.Replace(ct.TagEndSymbol, ct.ReplaceTagEndWith);
                }
            }

            string subbedText = flowchart.SubstituteVariables(displayText);

            sayDialog.Say(subbedText, !_extendPrevious, _waitForClick, _fadeWhenDone, _stopVoiceover, _waitForVO, _voiceOverClip, delegate {
                Continue();
            });
        }

        protected virtual void DecideSayDialogToUse(out bool success)
        {
            success = false;
        }

        private SayDialogManager SDManager => SayDialogManager.S;

        public override string GetSummary()
        {
            string namePrefix = "";
            if (_characterCached != null) 
            {
                namePrefix = _characterCached.NameText + ": ";
            }
            if (_extendPrevious)
            {
                namePrefix = "EXTEND" + ": ";
            }
            return namePrefix + "\"" + _storyText + "\"";
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Narrative;
        }

        public override void OnReset()
        {
            executionCount = 0;
        }

        public override void OnStopExecuting()
        {
            var sayDialog = SDManager.MainSayDialog;
            if (sayDialog == null)
            {
                return;
            }

            sayDialog.Stop();
        }

        public virtual bool ShowAlways { get { return _showAlways.Value; } }

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("setSayDialog")]
        protected SayDialog oldSetSayDialog;

        #region ILocalizable implementation

        public virtual string GetStandardText()
        {
            return _storyText;
        }

        public virtual void SetStandardText(string standardText)
        {
            _storyText.Value = standardText;
        }

        public virtual string GetDescription()
        {
            return _description;
        }
        
        public virtual string GetStringId()
        {
            // String id for Say commands is SAY.<Localization Id>.<Command id>.[Character Name]
            string stringId = "SAY." + GetFlowchartLocalizationId() + "." + itemId + ".";
            if (_characterCached != null)
            {
                stringId += _characterCached.NameText;
            }

            return stringId;
        }

        #endregion

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_setSayDialog != null && _setSayDialog.Value != null)
            {
                if (!_setSayDialog.TryGetComponent(out SayDialog _))
                {
                    Debug.LogError($"Say Command on {gameObject.name} has invalid Set " +
                        $"Say Dialog input. That input is the GameObject {_setSayDialog.Value.name}");
                }
            }
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            MigrateStuff();
        }
        
        void MigrateStuff()
        {
            if (oldSetSayDialog != null)
            {
                _setSayDialog = new GameObjectData(oldSetSayDialog.gameObject);
                oldSetSayDialog = null;
            }

            if (!string.IsNullOrEmpty(_oldStoryText))
            {
                _storyText.Value = _oldStoryText;
                _oldStoryText = "";
            }

            if (!string.IsNullOrEmpty(_oldDescription))
            {
                _description.Value = _oldDescription;
                _oldDescription = "";
            }

            if (_oldCharacter != null)
            {
                _character.Value = _oldCharacter.gameObject;
                _characterCached = _oldCharacter;
                _oldCharacter = null;
            }

            if (_oldPortrait != null)
            {
                _portrait.Value = _oldPortrait;
                _oldPortrait = null;
            }

            if (_oldVoiceOverClip != null)
            {
                _voiceOverClip.Value = _oldVoiceOverClip;
                _oldVoiceOverClip = null;
            }

            if (_oldShowAlways != true) 
            {
                // The default value is true, meaning that if the old value is false,
                // we want to set it to false. If the old value is true, we can just
                // leave it as is and not overwrite it.
                _showAlways.Value = _oldShowAlways;
                _oldShowAlways = true;
            }

            if (_oldShowCount != 1)
            {
                // The default value is 1, meaning that if the old value is not 1,
                // we want to set it to the old value. If the old value is 1, we can just
                // leave it as is and not overwrite it.
                _showCount.Value = _oldShowCount;
                _oldShowCount = 1;
            }

            if (_oldExtendPrevious != false)
            {
                _extendPrevious.Value = _oldExtendPrevious;
                _oldExtendPrevious = false;
            }

            if (_oldFadeWhenDone != true)
            {
                _fadeWhenDone.Value = _oldFadeWhenDone;
                _oldFadeWhenDone = true;
            }

            if (_oldWaitForClick != true)
            {
                _waitForClick.Value = _oldWaitForClick;
                _oldWaitForClick = true;
            }

            if (_oldStopVoiceover != true)
            {
                _stopVoiceover.Value = _oldStopVoiceover;
                _oldStopVoiceover = true;
            }

            if (_oldWaitForVO != false)
            {
                _waitForVO.Value = _oldWaitForVO;
                _oldWaitForVO = false;
            }

        }

        [SerializeField]
        [HideInInspector] protected bool _migrated;
        [FormerlySerializedAs("storyText")]
        [SerializeField] protected string _oldStoryText = "";
        [Tooltip("Notes about this story text for other authors, localization, etc.")]

        [FormerlySerializedAs("description")]
        [SerializeField] protected string _oldDescription = "";

        [Tooltip("Character that is speaking")]
        [FormerlySerializedAs("character")]
        [SerializeField] protected Character _oldCharacter;

        [Tooltip("Portrait that represents speaking character")]
        [FormerlySerializedAs("portrait")]
        [SerializeField] protected Sprite _oldPortrait;

        [Tooltip("Voiceover audio to play when writing the text")]
        [FormerlySerializedAs("voiceOverClip")]
        [SerializeField] protected AudioClip _oldVoiceOverClip;

        [Tooltip("Always show this Say text when the command is executed multiple times")]
        [FormerlySerializedAs("showAlways")]
        [SerializeField] protected bool _oldShowAlways = true;

        [Tooltip("Number of times to show this Say text when the command is executed multiple times")]
        [FormerlySerializedAs("showCount")]
        [SerializeField] protected int _oldShowCount = 1;

        [Tooltip("Type this text in the previous dialog box.")]
        [FormerlySerializedAs("extendPrevious")]
        [SerializeField] protected bool _oldExtendPrevious = false;

        [Tooltip("Fade out the dialog box when writing has finished and not waiting for input.")]
        [FormerlySerializedAs("fadeWhenDone")]
        [SerializeField] protected bool _oldFadeWhenDone = true;

        [Tooltip("Wait for player to click before continuing.")]
        [FormerlySerializedAs("waitForClick")]
        [SerializeField] protected bool _oldWaitForClick = true;

        [Tooltip("Stop playing voiceover when text finishes writing.")]
        [FormerlySerializedAs("stopVoiceover")]
        [SerializeField] protected bool _oldStopVoiceover = true;

        [Tooltip("Wait for the Voice Over to complete before continuing")]
        [FormerlySerializedAs("waitForVO")]
        [SerializeField] protected bool _oldWaitForVO = false;
    }
}
