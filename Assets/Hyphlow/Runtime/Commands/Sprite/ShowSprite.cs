using UnityEngine;
using UnityEngine.Serialization;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Makes a sprite visible / invisible by setting the color alpha.
    /// </summary>
    [CommandInfo("Sprite", 
                 "Show Sprite", 
                 "Makes a sprite visible / invisible by setting the color alpha.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ShowSprite : Command
    {
        [Tooltip("Sprite object to be made visible / invisible")]
        [SerializeField] protected SpriteRenderer spriteRenderer;

        [Tooltip("Make the sprite visible or invisible")]
        [SerializeField] protected BooleanData _visible = new BooleanData(false);

        [Tooltip("Affect the visibility of child sprites")]
        [SerializeField] protected BooleanData _affectChildren = new BooleanData(true);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_visible);
            _variableDataCache.Add(_affectChildren);
        }

        protected virtual void SetSpriteAlpha(SpriteRenderer renderer, bool visible)
        {
            Color spriteColor = renderer.color;
            spriteColor.a = visible ? 1f : 0f;
            renderer.color = spriteColor;
        }

        #region Public members

        public override void OnEnter()
        {
            if (spriteRenderer != null)
            {
                if (_affectChildren)
                {
                    var spriteRenderers = spriteRenderer.gameObject.GetComponentsInChildren<SpriteRenderer>();
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        var sr = spriteRenderers[i];
                        SetSpriteAlpha(sr, _visible.Value);
                    }
                }
                else
                {
                    SetSpriteAlpha(spriteRenderer, _visible.Value);
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (spriteRenderer == null)
            {
                return "Error: No sprite renderer selected";
            }

            return spriteRenderer.name + " to " + (_visible.Value ? "visible" : "invisible");
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Sprite;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_visible.VarRef, variable) || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("visible")] public bool visibleOLD;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (visibleOLD != default(bool))
            {
                _visible.Value = visibleOLD;
                visibleOLD = default(bool);
            }
        }

        #endregion
    }
}
