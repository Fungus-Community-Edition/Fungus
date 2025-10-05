using UnityEngine;
using Amanita.Tweening;

namespace Amanita.VScripting
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
        [SerializeField] protected float duration = 1f;

        [Tooltip("Current target alpha transparency value. The fade gradually adjusts the alpha to approach this target value.")]
        [SerializeField] protected float targetAlpha = 1f;

        [Tooltip("Wait until the fade has finished before executing next command")]
        [SerializeField] protected bool waitUntilFinished = true;

        [Tooltip("Color to render fullscreen fade texture with when screen is obscured.")]
        [SerializeField] protected Color fadeColor = Color.black;

        [Tooltip("Optional texture to use when rendering the fullscreen fade effect.")]
        [SerializeField] protected Texture2D fadeTexture;

        [SerializeField] protected ScriptableObject fadeTweener;

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
                    result = CameraManager.CreateColorTexture(fadeColor, 32, 32);
                }

                return result;
            }

            cameraManager.Fade(targetAlpha, duration, OnFadeDone, DoFadeTween);
            void OnFadeDone()
            {
                if (waitUntilFinished)
                {
                    Continue();
                }
            }

            if (!waitUntilFinished)
            {
                Continue();
            }
        }
        
        public override string GetSummary()
        {
            return "Fade to " + targetAlpha + " over " + duration + " seconds";
        }
        
        public override Color GetButtonColor()
        {
            return new Color32(216, 228, 170, 255);
        }

        #endregion

        public override void OnValidate()
        {
            base.OnValidate();
            ValidateTweeners();
        }

        protected IGeneralTweenAdapter<float> DoFadeTween => doFade as IGeneralTweenAdapter<float>;

    }    
}
