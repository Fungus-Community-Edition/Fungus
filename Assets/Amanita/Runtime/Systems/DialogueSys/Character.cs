using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Globalization;
using AtMycelia.Amanita.DialogueSys;

namespace AtMycelia.Amanita
{
    /// <summary>
    /// A Character that can be used in dialogue via the Say, Conversation and Portrait commands.
    /// </summary>
    [ExecuteInEditMode]
    public class Character : MonoBehaviour, IComparer<Character>
    {
        [Tooltip("Character name as displayed in Say Dialog.")]
        [FormerlySerializedAs("nameText")]
        [SerializeField] protected string _nameText; 
        // ^We need a separate name as the object name is used for
        // character variations (e.g. "Smurf Happy", "Smurf Sad")

        [Tooltip("Color to display the character name in Say Dialog.")]
        [FormerlySerializedAs("nameColor")]
        [SerializeField] protected Color _nameColor = Color.white;

        [Tooltip("Sound effect to play when this character is speaking.")]
        [FormerlySerializedAs("soundEffect")]
        [SerializeField] protected AudioClip _soundEffect;

        [Tooltip("List of portrait images that can be displayed for this character.")]
        [FormerlySerializedAs("portraits")]
        [SerializeField] protected List<Sprite> _portraits;

        [Tooltip("Direction that portrait sprites face.")]
        [FormerlySerializedAs("portraitsFace")]
        [SerializeField] protected FacingDirection _portraitsFace;

        [Tooltip("Sets the active Say dialog with a reference to a Say Dialog object in " +
            "the scene. This Say Dialog will be used whenever the character speaks.")]
        [FormerlySerializedAs("setSayDialog")]
        [SerializeField] protected SayDialog _setSayDialog;

        [FormerlySerializedAs("notes")]
        [TextArea(5,10)]
        [FormerlySerializedAs("description")]
        [SerializeField] protected string _description;

        [Tooltip("Optional, AudioSource to be used for effects and 'beeps' for " +
            "this Character. (Deprecated)")]
        [FormerlySerializedAs("effectAudioSource")]
        [SerializeField] protected AudioSource _effectAudioSource;

        [Tooltip("Optional, AudioSource to be used for voice over AudioClips for " +
            "this Character. (Deprecated)")]
        [FormerlySerializedAs("voiceAudioSource")]
        [SerializeField] protected AudioSource _voiceAudioSource;

        protected PortraitState _portraitState = new PortraitState();

        protected static List<Character> _activeCharacters = new List<Character>();

        /// <summary>
        /// Currently display profile sprite for this character.
        /// </summary>
        /// <value>The profile sprite.</value>
        public virtual Sprite ProfileSprite { get; set; }

        protected virtual void OnEnable()
        {
            if (!_activeCharacters.Contains(this))
            {
                _activeCharacters.Add(this);
                _activeCharacters.Sort(this);
            }
        }

        protected virtual void OnDisable()
        {
            _activeCharacters.Remove(this);
        }

        #region Public members

        /// <summary>
        /// Gets the list of active characters.
        /// </summary>
        public static List<Character> ActiveCharacters => _activeCharacters;

        /// <summary>
        /// Character name as displayed in Say Dialog.
        /// </summary>
        public virtual string NameText => _nameText;

        /// <summary>
        /// Color to display the character name in Say Dialog.
        /// </summary>
        public virtual Color NameColor 
        { 
            get { return _nameColor; } 
            set { _nameColor = value; } 
        }

        /// <summary>
        /// Sound effect to play when this character is speaking.
        /// </summary>
        /// <value>The sound effect.</value>
        public virtual AudioClip SoundEffect 
        { 
            get { return _soundEffect; } 
            set { _soundEffect = value; } 
        }

        /// <summary>
        /// List of portrait images that can be displayed for this character.
        /// </summary>
        public virtual List<Sprite> Portraits => _portraits;

        /// <summary>
        /// Direction that portrait sprites face.
        /// </summary>
        public virtual FacingDirection PortraitsFace => _portraitsFace;

        /// <summary>
        /// Current display state of this character's portrait.
        /// </summary>
        /// <value>The state.</value>
        public virtual PortraitState State => _portraitState;

        /// <summary>
        /// Sets the active Say dialog with a reference to a Say Dialog object or a prefab. 
        /// This Say Dialog will be used whenever the character speaks.
        /// </summary>
        public virtual SayDialog SetSayDialog  => _setSayDialog;

        public virtual AudioSource VoiceAudioSource
        { 
            get { return _voiceAudioSource; } 
            set { _voiceAudioSource = value; } 
        }

        public virtual AudioSource EffectAudioSource 
        { 
            get { return _effectAudioSource; } 
            set { _effectAudioSource = value; } 
        }

        public virtual GameObject SayDialogGameObject
        {
            get
            {
                return _setSayDialog.gameObject;
            }
            set
            {
                if (value.TryGetComponent<SayDialog>(out var sd))
                {
                    _setSayDialog = sd;
                }
            }
        }

        /// <summary>
        /// Returns the name of the game object.
        /// </summary>
        public string GetObjectName() { return gameObject.name; }

        /// <summary>
        /// Returns true if the character name starts with the specified 
        /// string. Case insensitive.
        /// </summary>
        public virtual bool NameStartsWith(string matchString)
        {
#if NETFX_CORE
            return name.StartsWith(matchString, StringComparison.CurrentCultureIgnoreCase)
                || nameText.StartsWith(matchString, StringComparison.CurrentCultureIgnoreCase);
#else
            return name.StartsWith(matchString, true, CultureInfo.CurrentCulture)
                || NameText.StartsWith(matchString, true, CultureInfo.CurrentCulture);
#endif
        }

        /// <summary>
        /// Returns true if the character name is a complete match to the specified 
        /// string. Case insensitive.
        /// </summary>
        public virtual bool NameMatch(string matchString)
        {
            return string.Compare(name, matchString, true, CultureInfo.CurrentCulture) == 0
                || string.Compare(_nameText, matchString, true, CultureInfo.CurrentCulture) == 0;
        }

        public int Compare(Character firstChar, Character secondChar)
        {
            if (firstChar == secondChar)
                return 0;
            if (secondChar == null)
                return 1;
            if (firstChar == null)
                return -1;

            return firstChar.name.CompareTo(secondChar.name);
        }

        /// <summary>
        /// Looks for a portrait by name on a character
        /// If none is found, give a warning and return a blank sprite
        /// </summary>
        public virtual Sprite GetPortrait(string portraitString)
        {
            if (string.IsNullOrEmpty(portraitString))
            {
                return null;
            }

            for (int i = 0; i < _portraits.Count; i++)
            {
                var currentPortrait = _portraits[i];
                if (currentPortrait == null)
                {
                    continue;
                }

                bool matchesName = string.Compare(currentPortrait.name, portraitString, true) == 0;
                if (matchesName)
                {
                    return _portraits[i];
                }
            }
            return null;
        }

        #endregion

        #region ILocalizable implementation

        public virtual string StandardText => NameText;
        
        public virtual void SetStandardText(string standardText)
        {
            _nameText = standardText;
        }

        public virtual string GetDescription => _description;
        

        public virtual string StringId => "CHARACTER." + NameText;
        // String id for character names is CHARACTER.<Character Name>

        #endregion

        protected virtual void OnValidate()
        {
            if (_portraits != null && _portraits.Count > 1)
            {
                _portraits.Sort(PortraitUtil.PortraitCompareTo);
            }
        }
    }
}
