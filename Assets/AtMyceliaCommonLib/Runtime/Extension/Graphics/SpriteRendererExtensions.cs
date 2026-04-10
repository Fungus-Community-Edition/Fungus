using UnityEngine;

namespace AtMycelia.Graphics
{
    public static class SpriteRendererExtensions 
    {
        public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha)
        {
            Color origColor = spriteRenderer.color;
            Color withNewAlpha = new Color(origColor.r, origColor.g, origColor.b, alpha);
            spriteRenderer.color = withNewAlpha;
        }

        public static void SetSizeX(this SpriteRenderer spriteRenderer, float newSizeX)
        {
            Vector2 newSize = spriteRenderer.size;
            newSize.x = newSizeX;
            spriteRenderer.size = newSize;
        }

        public static void SetSizeY(this SpriteRenderer spriteRenderer, float newSizeY)
        {
            Vector2 newSize = spriteRenderer.size;
            newSize.y = newSizeY;
            spriteRenderer.size = newSize;
        }

        public static void ShiftSizeBy(this SpriteRenderer spriteRenderer, Vector2 howMuchToShift)
        {
            spriteRenderer.ShiftSizeBy(howMuchToShift.x, howMuchToShift.y);
        }

        public static void ShiftSizeBy(this SpriteRenderer spriteRenderer, float xShift, float yShift)
        {
            Vector2 newSize = spriteRenderer.size;
            newSize.x += xShift;
            newSize.y += yShift;
            spriteRenderer.size = newSize;
        }

        /// <summary>
        /// Adds (or in the case of a negative value, subtracts) from this SpriteRenderer's size
        /// </summary>
        public static void ShiftSizeXBy(this SpriteRenderer spriteRenderer, float howMuch)
        {
            ShiftSizeBy(spriteRenderer, howMuch, 0);
        }

        public static void ShiftSizeYBy(this SpriteRenderer spriteRenderer, float howMuch)
        {
            ShiftSizeBy(spriteRenderer, 0, howMuch);
        }
    }
}