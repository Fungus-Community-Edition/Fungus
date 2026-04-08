using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace AtMycelia.Amanita.DialogueSys
{
    /// <summary>
    /// Contains all options to run a portrait command.
    /// </summary>
    public class PortraitOptions
    {
        public Character character;
        public Character replacedCharacter;
        public Sprite portrait;
        public DisplayType display;
        public PositionOffset offset;
        public RectTransform fromPosition;
        public RectTransform toPosition;
        public FacingDirection facing;
        public bool useDefaultSettings;
        public float fadeDuration;
        public float moveDuration;
        public Vector2 shiftOffset;
        public bool move; //sets to position to be the same as from
        public bool shiftIntoPlace;
        public bool waitUntilFinished;
        public System.Action onComplete;

        public PortraitOptions(bool useDefaultSettings = true)
        {
            character = null;
            replacedCharacter = null;
            portrait = null;
            display = DisplayType.None;
            offset = PositionOffset.None;
            fromPosition = null;
            toPosition = null;
            facing = FacingDirection.None;
            shiftOffset = new Vector2(0, 0);
            move = false;
            shiftIntoPlace = false;
            waitUntilFinished = false;
            onComplete = null;

            // Special values that can be overridden
            fadeDuration = 0.5f;
            moveDuration = 1f;
            this.useDefaultSettings = useDefaultSettings;
        }
    }

    /// <summary>
    /// Represents the current state of a character portrait on the stage.
    /// </summary>
    public class PortraitState
    {
        public bool onScreen;
        public bool dimmed;
        public DisplayType display;
        public RectTransform position, holder;
        public FacingDirection facing;
        public Image portraitImage;
        public Sprite portrait { get { return portraitImage != null ? portraitImage.sprite : null; } }
        public List<Image> allPortraits = new List<Image>();

        public void SetPortraitImageBySprite(Sprite portrait)
        {
            portraitImage = allPortraits.Find(x => x.sprite == portrait);
        }
    }

    /// <summary>
    /// Util functions for working with portraits.
    /// </summary>
    public static class PortraitUtil 
    {
        #region Public members

        static public int PortraitCompareTo(Sprite x, Sprite y)
        {
            if (x == y)
                return 0;
            if (y == null)
                return -1;
            if (x == null)
                return 1;

            return x.name.CompareTo(y.name);
        }

        #endregion
    }
}
