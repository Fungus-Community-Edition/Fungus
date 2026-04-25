using AtMycelia.AmaniTween;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow.Legacy
{
    /// <summary>
    /// Select which type of fade will be applied.
    /// </summary>
    [MovedFrom("AtMycelia.Amanita.VScripting.Legacy")]
    public enum FadeMode
    {
        /// <summary> Fade the alpha color component only. </summary>
        Alpha,
        /// <summary> Fade all color components (RGBA). </summary>
        Color
    }

    /// <summary>
    /// Fades a UI object.
    /// </summary>
    [CommandInfo("UI",
                 "Fade UI",
                 "Fades a UI object")]
    [MovedFrom("AtMycelia.Amanita.VScripting.Legacy")]
    public class FadeUI : TweenUI 
    {
        [SerializeField] protected FadeMode fadeMode = FadeMode.Alpha;
        [SerializeField] protected ColorData targetColor = new ColorData(Color.white);
        [SerializeField] protected FloatData targetAlpha = new FloatData(1f);
        [SerializeField] protected ScriptableObject fadeTweener;

        protected override void ValidateTweeners()
        {
            TweenUtils.EnsureValidTweener(ref fadeTweener, typeof(IGraphicTweenAdapter), "fading graphics");
        }

        protected IGraphicTweenAdapter DoesFading => fadeTweener as IGraphicTweenAdapter;
        protected override void ApplyTweenToSingle(GameObject go)
        {
            // Images, legacy UI Texts and TMP Texts are below Graphic in the family tree, thus we can 
            // put them all in the same list and treat them the same
            IList<Graphic> graphics = go.GetComponentsInChildren<Graphic>();
            ApplyToGraphics();
            void ApplyToGraphics()
            {
                for (int i = 0; i < graphics.Count; i++)
                {
                    var graphicEl = graphics[i];
                    if (graphicEl == null)
                    {
                        Debug.LogWarning($"{this.gameObject.name}: Null graphic found when trying to fade UI " +
                            $"element at index {i}");
                        continue;
                    }

                    // We assume that the tweeners know what to do when the duration is zero, and
                    // thus we won't check for that here
                    switch (fadeMode)
                    {
                        case FadeMode.Alpha:
                            DoesFading.FadeOpacity(graphicEl, targetAlpha, duration);
                            break;
                        case FadeMode.Color:
                            DoesFading.FadeColor(graphicEl, targetColor, duration);
                            break;
                        default:
                            Debug.LogWarning($"{this.gameObject.name}: Unsupported fade mode {fadeMode} when " +
                                $"trying to fade UI element at index {i}");
                            break;
                    }

                }
            }

            ApplyToCanvasGroups();
            void ApplyToCanvasGroups()
            {
                // Canvas groups don't support color, but we can fade their alpha based on the
                // target color's alpha if needed
                var canvasGroups = go.GetComponentsInChildren<CanvasGroup>();
                for (int i = 0; i < canvasGroups.Length; i++)
                {
                    var canvasGroupEl = canvasGroups[i];
                    switch (fadeMode)
                    {
                        case FadeMode.Alpha:
                            DoesFading.FadeOpacity(canvasGroupEl, targetAlpha, duration); break;
                        case FadeMode.Color:
                            DoesFading.FadeOpacity(canvasGroupEl, targetColor.Value.a, duration); break;
                        default:
                            Debug.LogWarning($"{this.gameObject.name}: Unsupported fade mode {fadeMode} when " +
                                $"trying to fade CanvasGroup at index {i}");
                            break;
                    }
                }
            }
        }

        protected override string GetSummaryValue()
        {
            string result = "";
            if (fadeMode == FadeMode.Alpha)
            {
                result = targetAlpha.Value.ToString() + " alpha";
            }
            else if (fadeMode == FadeMode.Color)
            {
                result = targetColor.Value.ToString()  + " color";
            }

            return result;
        }

        #region Public members

        public override bool IsPropertyVisible(string propertyName)
        {
            if (fadeMode == FadeMode.Alpha &&
                propertyName == "targetColor")
            {
                return false;
            }

            if (fadeMode == FadeMode.Color &&
                propertyName == "targetAlpha")
            {
                return false;
            }

            return true;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(targetColor.VarRef, variable) || 
                ReferenceEquals(targetAlpha.VarRef, variable) ||
                base.HasReference(variable);
        }

        #endregion
    }
}
