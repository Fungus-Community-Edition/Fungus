using Amanita.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Amanita.VScripting
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
        [SerializeField] protected SpriteRenderer spriteRenderer;

        [Tooltip("Length of time to perform the fade")]
        [SerializeField] protected FloatData duration = new FloatData(1f);

        [Tooltip("Target color to fade to. To only fade transparency level, set the color to white and set the alpha to required transparency.")]
        [SerializeField] protected ColorData targetColor = new ColorData(Color.white);

        [Tooltip("Wait until the fade has finished before executing the next command")]
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(true);

        [SerializeField] protected ScriptableObject fadeTweener;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(duration);
            variableDataCache.Add(targetColor);
            variableDataCache.Add(waitUntilFinished);
        }

        protected virtual void Awake()
        {
            ValidateTweeners(false);
        }

        #region Public members

        public override void OnEnter()
        {
            if (spriteRenderer == null)
            {
                Continue();
                return;
            }

            SpriteFader.FadeSprite(spriteRenderer, targetColor.Value, duration.Value,
                Vector2.zero, doFadeTween, ContinueAfterWait);

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        protected virtual void ContinueAfterWait()
        {
            if (waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (spriteRenderer == null)
            {
                return "Error: No sprite renderer selected";
            }

            return spriteRenderer.name + " to " + targetColor.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Sprite;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(duration.VarRef, variable) || 
                ReferenceEquals(targetColor.VarRef, variable) ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("duration")] public float durationOLD;
        [HideInInspector] [FormerlySerializedAs("targetColor")] public Color targetColorOLD;
        [SerializeField][FormerlySerializedAs("waitUntilFinished")] protected bool waitUntilFinishedOLD;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (durationOLD != default)
            {
                duration.Value = durationOLD;
                durationOLD = default;
            }
            if (targetColorOLD != default)
            {
                targetColor.Value = targetColorOLD;
                targetColorOLD = default;
            }
            if (waitUntilFinishedOLD != default)
            {
                waitUntilFinished.Value = waitUntilFinishedOLD;
                waitUntilFinishedOLD = default;
            }
        }

        #endregion

        public override void OnValidate()
        {
            base.OnValidate();
            ValidateTweeners();
        }

        protected virtual void ValidateTweeners(bool logMessages = true)
        {
            TweenUtils.EnsureValidTweener(ref fadeTweener,
                typeof(IGraphicTweenAdapter),
                "sprite-fading", logMessages);
            doFadeTween = fadeTweener as IGraphicTweenAdapter;
        }

        protected IGraphicTweenAdapter doFadeTween;
    }
}
