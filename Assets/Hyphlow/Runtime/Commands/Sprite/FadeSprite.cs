using AtMycelia.AmaniTween;
using UnityEngine;
using UnityEngine.Serialization;
using AtMycelia.Hyphlow;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Fades a sprite to a target color over a period of time.
    /// </summary>
    [CommandInfo("Sprite", 
                 "Fade Sprite", 
                 "Fades a sprite to a target color over a period of time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class FadeSprite : Command
    {
        [Tooltip("Sprite object to be faded")]
        [FormerlySerializedAs("spriteRenderer")]
        [SerializeField] protected SpriteRenderer _spriteRenderer;

        [Tooltip("Length of time to perform the fade")]
        [FormerlySerializedAs("duration")]
        [SerializeField] protected FloatData _duration = new FloatData(1f);

        [Tooltip("Target color to fade to. To only fade transparency level, set the color to white and " +
                 "set the alpha to required transparency.")]
        [FormerlySerializedAs("targetColor")]
        [SerializeField] protected ColorData _targetColor = new ColorData(Color.white);

        [Tooltip("Wait until the fade has finished before executing the next command")]
        [FormerlySerializedAs("waitUntilFinished")]
        [SerializeField] protected BooleanData _waitUntilFinished = new BooleanData(true);

        [FormerlySerializedAs("fadeTweener")]
        [SerializeField] protected ScriptableObject _fadeTweener;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_duration);
            _variableDataCache.Add(_targetColor);
            _variableDataCache.Add(_waitUntilFinished);
        }

        protected virtual void Awake()
        {
            ValidateTweeners(false);
        }

        #region Public members

        public override void OnEnter()
        {
            if (_spriteRenderer == null)
            {
                Continue();
                return;
            }

            SpriteFader.FadeSprite(_spriteRenderer, _targetColor.Value, _duration.Value,
                Vector2.zero, doFadeTween, ContinueAfterWait);

            if (!_waitUntilFinished)
            {
                Continue();
            }
        }

        protected virtual void ContinueAfterWait()
        {
            if (_waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (_spriteRenderer == null)
            {
                return "Error: No sprite renderer selected";
            }

            return _spriteRenderer.name + " to " + _targetColor.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Sprite;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_duration.VarRef, variable) || 
                ReferenceEquals(_targetColor.VarRef, variable) ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [SerializeField] [HideInInspector] [FormerlySerializedAs("duration")] 
        public float durationOLD;

        [SerializeField] [HideInInspector] [FormerlySerializedAs("targetColor")] 
        public Color targetColorOLD;

        [SerializeField] [HideInInspector] [FormerlySerializedAs("waitUntilFinished")] 
        protected bool waitUntilFinishedOLD;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (durationOLD != default)
            {
                _duration.Value = durationOLD;
                durationOLD = default;
            }
            if (targetColorOLD != default)
            {
                _targetColor.Value = targetColorOLD;
                targetColorOLD = default;
            }
            if (waitUntilFinishedOLD != default)
            {
                _waitUntilFinished.Value = waitUntilFinishedOLD;
                waitUntilFinishedOLD = default;
            }
        }

        #endregion

        protected override void OnValidate()
        {
            base.OnValidate();
            ValidateTweeners();
        }

        protected virtual void ValidateTweeners(bool logMessages = true)
        {
            TweenUtils.EnsureValidTweener(ref _fadeTweener,
                typeof(IGraphicTweenAdapter),
                "sprite-fading", logMessages);
            doFadeTween = _fadeTweener as IGraphicTweenAdapter;
        }

        protected IGraphicTweenAdapter doFadeTween;
    }
}
