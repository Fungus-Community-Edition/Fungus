using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Sets the mouse cursor sprite.
    /// </summary>
    [CommandInfo("Sprite", 
                 "Set Mouse Cursor", 
                 "Sets the mouse cursor sprite.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SetMouseCursor : Command 
    {
        [Tooltip("Texture to use for cursor. Will use default mouse cursor if no sprite is specified")]
        [SerializeField] protected TextureData _cursorTexture;

        [Tooltip("The offset from the top left of the texture to use as the target point")]
        [SerializeField] protected Vector2Data _hotSpot;
        

        // Cached static cursor settings
        protected static Texture2D activeCursorTexture;
        protected static Vector2 activeHotspot;

        public static void ResetMouseCursor()
        {
            // Change mouse cursor back to most recent settings
            Cursor.SetCursor(activeCursorTexture, activeHotspot, CursorMode.Auto);
        }

        public override void OnEnter()
        {
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);

            activeCursorTexture = (Texture2D)_cursorTexture.Value;
            activeHotspot = _hotSpot;

            Continue();
        }

        public override string GetSummary()
        {
            if (_cursorTexture.Value == null)
            {
                return "Error: No cursor sprite selected";
            }

            string result = _cursorTexture.Value.name;
            if (_cursorTexture.RepresentingVar)
            {
                result += $" ({_cursorTexture.VarRef.Key})";
            }

            result += $" w/ hotspot {_hotSpot.Value}";
            if (_hotSpot.RepresentingVar)
            {
                result += $" ({_hotSpot.VarRef.Key})";
            }

            return result;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();

            if (cursorTexture == null && _cursorTexture != null)
            {
                _cursorTexture.Value = cursorTexture;
                cursorTexture = null;
            }

            if (hotSpot != default && _hotSpot != null)
            {
                _hotSpot.Value = hotSpot;
                hotSpot = default;
            }
        }

        [SerializeField] [HideInInspector] protected Texture2D cursorTexture;
        [SerializeField] [HideInInspector] protected Vector2 hotSpot;
    }
}
