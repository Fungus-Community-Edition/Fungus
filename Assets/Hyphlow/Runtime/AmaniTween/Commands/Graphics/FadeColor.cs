using AtMycelia.AmaniTween.VScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween
{
    [CommandInfo("BI Tween/Graphics",
                 "Fade Color",
                 "Fades the color of a component to a target color over a duration. Fading only the " +
                 "opacity/transparency/alpha is also an option.")]
    public class FadeColor : BaseSimpleTweenCommand
    {
        [ContentTypeConstraint(typeof(Graphic), typeof(GameObject), typeof(SpriteRenderer))]
        [SerializeField] protected VariableReference _target = new VariableReference();
        [SerializeField] protected ColorData _targetColor = new ColorData(Color.white);

        [Tooltip("Goes by a scale of 0 for totally transparent and 100 for totally opaque. " +
            "Only affects the alpha channel of the color. This value overrides the alpha in " +
            "the Target Color, even when Fade Only Alpha is false.")]
        [SerializeField] protected FloatData _targetAlpha = new FloatData(100f);

        [Tooltip("If true, the tween will only affect the alpha channel of the color. " +
            "The RGB channels will be ignored and remain unchanged.")]
        [SerializeField] protected BooleanData _fadeOnlyAlpha = new BooleanData(false);

        [Tooltip("Affect the colors of child Graphics and SpriteRenderers.")]
        [SerializeField] protected BooleanData _affectChildren = new BooleanData(false);

        protected override void RegisterAllTargets()
        {
            _spriteRenderer = null;
            _normalGraphic = _target.Variable.BoxedValue as Graphic; // Prioritizing Graphic since it's the more general class
            _spriteRenderer = _target.Variable.BoxedValue as SpriteRenderer;

            GameObject go = _target.Variable.BoxedValue as GameObject;
            if (_normalGraphic == null && _spriteRenderer == null && go != null)
            {
                _normalGraphic = go.GetComponent<Graphic>();
                if (_normalGraphic == null)
                {
                    _spriteRenderer = go.GetComponent<SpriteRenderer>();
                }
            }

            if (_affectChildren.Value)
            {
                if (go == null)
                {
                    if (_normalGraphic != null)
                    {
                        go = _normalGraphic.gameObject;
                    }
                    else if (_spriteRenderer != null)
                    {
                        go = _spriteRenderer.gameObject;
                    }
                }

                AddTargetsFromGameObject(go);
                return;
            }

            if (_normalGraphic != null)
            {
                AddTarget(_normalGraphic);
            }
            else if (_spriteRenderer != null)
            {
                AddTarget(_spriteRenderer);
            }
        }

        protected Graphic _normalGraphic;
        protected SpriteRenderer _spriteRenderer;

        private void AddTargetsFromGameObject(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            Graphic[] graphics = go.GetComponentsInChildren<Graphic>();
            for (int i = 0; i < graphics.Length; i++)
            {
                AddTarget(graphics[i]);
            }

            SpriteRenderer[] spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                AddTarget(spriteRenderers[i]);
            }
        }

        private void AddTarget(object target)
        {
            if (target == null)
            {
                return;
            }

            if (_allTargets.Contains(target))
            {
                return;
            }

            _allTargets.Add(target);
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_targetColor);
            _variableDataCache.Add(_targetAlpha);
            _variableDataCache.Add(_fadeOnlyAlpha);
            _variableDataCache.Add(_affectChildren);
        }

        protected override bool AreTargetsValid()
        {
            return _allTargets.Count > 0;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();
            List<ITweenHandle> tweens = new List<ITweenHandle>(_allTargets.Count);
            for (int i = 0; i < _allTargets.Count; i++)
            {
                ITweenHandle tween = TweenTarget(_allTargets[i]);
                if (tween != null)
                {
                    tweens.Add(tween);
                }
            }

            if (tweens.Count == 0)
            {
                return null;
            }

            if (tweens.Count == 1)
            {
                _ourTween = tweens[0];
                return _ourTween;
            }

            _ourTween = new MultiTweenHandle(tweens);
            return _ourTween;
        }

        private ITweenHandle TweenTarget(object target)
        {
            if (target is Graphic graphic)
            {
                Color startColor = DecideStartColor(graphic);
                Color endColor = DecideEndColor(ref startColor);
                return _tweener.FadeColor(graphic, endColor, _duration);
            }

            if (target is SpriteRenderer spriteRenderer)
            {
                Color startColor = DecideStartColor(spriteRenderer);
                Color endColor = DecideEndColor(ref startColor);
                return _tweener.FadeColor(spriteRenderer, endColor, _duration);
            }

            return null;
        }

        protected virtual Color DecideStartColor(Graphic graphic)
        {
            return graphic.color;
        }

        protected virtual Color DecideStartColor(SpriteRenderer spriteRenderer)
        {
            return spriteRenderer.color;
        }

        protected virtual Color DecideEndColor(ref Color startColor)
        {
            Color result;
            if (_fadeOnlyAlpha.Value)
            {
                result = startColor;
            }
            else
            {
                result = _targetColor.Value;
            }

            result.a = _targetAlpha.Value / 100f;

            return result;
        }

        protected override void ValidateTweener()
        {
            if (_tweenerSO != null && _tweenerSO is not IGraphicTweenAdapter)
            {
                Debug.LogWarning($"The tweener assigned to {name} is not a transform tween adapter. " +
                    $"Reverting to default one.");
                GoWithDefaultTweener();
            }

            base.ValidateTweener();
            _tweener = _tweenerSO as IGraphicTweenAdapter;
        }

        private IGraphicTweenAdapter _tweener;

        public override Color GetButtonColor()
        {
            return CommandColors.Animation;
        }

        public override string GetSummary()
        {
            string result;
            if (_target.Variable == null)
            {
                result = "Need target.";
            }
            else
            {
                string targName = _target.VarKey;
                string alphaStr = GetAlphaStr();
                string colorStr = GetTargColorString();
                string affectChildrenStr = GetAffectChildrenStr();
                string durationStr = GetDurationString();
                if (_fadeOnlyAlpha.Value)
                {
                    result = $"{targName}'s alpha to {alphaStr} over {durationStr}.{affectChildrenStr}";
                }
                else
                {
                    result = $"{targName} to {colorStr} with alpha {alphaStr} over {durationStr}.{affectChildrenStr}";
                }
            }
            return result;
        }

        private string GetAlphaStr()
        {
            string result;
            if (_targetAlpha.RepresentingVar)
            {
                result = $"{_target.VarKey}%";
            }
            else
            {
                result = $"{Mathf.RoundToInt(_targetAlpha.Value)}%";
            }
            return result;
        }

        private string GetTargColorString()
        {
            string result;
            if (_targetColor.RepresentingVar)
            {
                result = _targetColor.VarRef.Key;
            }
            else
            {
                result = _targetColor.Value.ToString();
            }
            return result;
        }


        private string GetAffectChildrenStr()
        {
            string result = _affectChildren.Value ? 
                " Affecting Children." : 
                "";
            return result;
        }

        private string GetDurationString()
        {
            string result;
            if (_duration.RepresentingVar)
            {
                result = $"{_duration.VarRef.Key}s";
            }
            else
            {
                result = $"{_duration.Value}s";
            }
            return result;
        }

    }
}