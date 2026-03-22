using UnityEngine;
using AtMycelia.Amanita.Tweening;
using UnityEngine.Serialization;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Draws a fullscreen texture over the scene to give a fade effect. Setting Target Alpha to 1 will obscure the screen, alpha 0 will reveal the screen.
    /// If no Fade Texture is provided then a default flat color texture is used.
    /// </summary>
    [CommandInfo("Camera", 
                 "Fade Screen", 
                 "Draws a fullscreen texture over the scene to give a fade effect. Setting Target Alpha to 1 will obscure the screen, alpha 0 will reveal the screen. " +
                 "If no Fade Texture is provided then a default flat color texture is used.")]
    [AddComponentMenu("")]
    public class FadeScreen : Command 
    {
        [Tooltip("Time for fade effect to complete")]
        [SerializeField] protected FloatData duration = new FloatData(1f);

        [Tooltip("Current target alpha transparency value. The fade gradually adjusts the alpha to approach this target value.")]
        [SerializeField] protected FloatData targetAlpha = new FloatData(1f);

        [Tooltip("Wait until the fade has finished before executing next command")]
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(true);

        [Tooltip("Color to render fullscreen fade texture with when screen is obscured.")]
        [SerializeField] protected ColorData fadeColor = new ColorData(Color.black);

        [Tooltip("Optional texture to use when rendering the fullscreen fade effect.")]
        [SerializeField] protected Texture2D fadeTexture;

        [SerializeField] protected ScriptableObject fadeTweener;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(duration);
            variableDataCache.Add(targetAlpha);
            variableDataCache.Add(waitUntilFinished);
            variableDataCache.Add(fadeColor);
        }

        protected virtual void Awake()
        {
            ValidateTweeners();
        }

        protected virtual void ValidateTweeners()
        {
            if (fadeTweener == null)
            {
                doFade = AmanitaManager.DefaultTweener;
                return;
            }

            doFade = fadeTweener as IGeneralTweenAdapter<float>;

            if (doFade == null)
            {
                Debug.LogWarning($"Fade tweener passed to FadeScreen is invalid. It needs to implement IGeneralTweenAdapter<float>. Going back to default.");
                fadeTweener = AmanitaManager.DefaultTweener;
                doFade = AmanitaManager.DefaultTweener;
            }
            
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            BackwardsCompatibility();
        }

        private void BackwardsCompatibility()
        {
            if (!_migrated)
            {
                duration.Value = _oldDuration;
                targetAlpha.Value = _oldTargetAlpha;
                waitUntilFinished.Value = _oldWaitUntilFinished;
                fadeColor.Value = _oldFadeColor;

                _oldDuration = -1;
                _oldTargetAlpha = -1;
                _oldWaitUntilFinished = false;
                _oldFadeColor = Color.clear;
                
                _migrated = true;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        [FormerlySerializedAs("duration")]
        [SerializeField] [HideInInspector] protected float _oldDuration;

        [FormerlySerializedAs("targetAlpha")]
        [SerializeField] [HideInInspector] protected float _oldTargetAlpha;

        [FormerlySerializedAs("waitUntilFinished")]
        [SerializeField] [HideInInspector] protected bool _oldWaitUntilFinished = true;

        [FormerlySerializedAs("fadeColor")]
        [SerializeField] [HideInInspector] protected Color _oldFadeColor = Color.black;

        [SerializeField]
        [HideInInspector] private bool _migrated;

        protected IGeneralTweenAdapter<float> doFade;

        #region Public members

        public override void OnEnter()
        {
            var cameraManager = AmanitaManager.S.CameraManager;

            cameraManager.ScreenFadeTexture = DecideFadeTex();
            Texture2D DecideFadeTex()
            {
                Texture2D result = fadeTexture;
                if (result == null)
                {
                    result = CameraManager.CreateColorTexture(fadeColor.Value, 32, 32);
                }

                return result;
            }

            cameraManager.Fade(targetAlpha.Value, duration.Value, OnFadeDone, DoFadeTween);
            void OnFadeDone()
            {
                if (waitUntilFinished.Value)
                {
                    Debug.Log($"Fade finished, continuing with next command.");
                    Continue();
                }
            }

            if (!waitUntilFinished.Value)
            {
                Continue();
            }
        }
        
        public override string GetSummary()
        {
            string result = $"Fade to {targetAlpha.Value} ";
            if (targetAlpha.RepresentingVar)
            {
                result += $"({targetAlpha.VarRef.Key}) ";
            }

            result += $"over {duration.Value} ";

            if (duration.RepresentingVar)
            {
                result += $"({duration.VarRef.Key}) ";
            }

            result += "seconds";

            return result;
        }
        
        public override Color GetButtonColor()
        {
            return new Color32(216, 228, 170, 255);
        }

        #endregion

        protected override void OnValidate()
        {
            base.OnValidate();
            ValidateTweeners();
        }

        protected IGeneralTweenAdapter<float> DoFadeTween => doFade as IGeneralTweenAdapter<float>;

    }    
}
