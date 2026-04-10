using UnityEngine;

namespace AtMycelia.Physics
{
    public static class Collider2DExtensions 
    {

        public static void SetSizeX(this CapsuleCollider2D collider2D, float newSizeX)
        {
            Vector2 newSize = collider2D.size;
            newSize.x = newSizeX;
            collider2D.size = newSize;
        }

        public static void SetSizeY(this CapsuleCollider2D collider2D, float newSizeY)
        {
            Vector2 newSize = collider2D.size;
            newSize.y = newSizeY;
            collider2D.size = newSize;
        }

        public static void ShiftSizeBy(this CapsuleCollider2D collider2D, Vector2 howMuchToShift)
        {
            collider2D.ShiftSizeBy(howMuchToShift.x, howMuchToShift.y);
        }

        public static void ShiftSizeBy(this CapsuleCollider2D collider2D, float xShift, float yShift)
        {
            Vector2 newSize = collider2D.size;
            newSize.x += xShift;
            newSize.y += yShift;
            collider2D.size = newSize;
        }

        /// <summary>
        /// Adds (or in the case of a negative value, subtracts) from this CapsuleCollider2D's size
        /// </summary>
        public static void ShiftSizeXBy(this CapsuleCollider2D collider2D, float howMuch)
        {
            ShiftSizeBy(collider2D, howMuch, 0);
        }

        public static void ShiftSizeYBy(this CapsuleCollider2D collider2D, float howMuch)
        {
            ShiftSizeBy(collider2D, 0, howMuch);
        }

        //////////////////////

        public static void SetOffsetX(this CapsuleCollider2D collider2D, float newOffsetX)
        {
            Vector2 newOffset = collider2D.offset;
            newOffset.x = newOffsetX;
            collider2D.offset = newOffset;
        }

        public static void SetOffsetY(this CapsuleCollider2D collider2D, float newOffsetY)
        {
            Vector2 newOffset = collider2D.offset;
            newOffset.y = newOffsetY;
            collider2D.offset = newOffset;
        }

        public static void ShiftOffsetBy(this CapsuleCollider2D collider2D, Vector2 howMuchToShift)
        {
            collider2D.ShiftOffsetBy(howMuchToShift.x, howMuchToShift.y);
        }

        public static void ShiftOffsetBy(this CapsuleCollider2D collider2D, float xShift, float yShift)
        {
            Vector2 newOffset = collider2D.offset;
            newOffset.x += xShift;
            newOffset.y += yShift;
            collider2D.offset = newOffset;
        }

        /// <summary>
        /// Adds (or in the case of a negative value, subtracts) from this CapsuleCollider2D's offset
        /// </summary>
        public static void ShiftOffsetXBy(this CapsuleCollider2D collider2D, float howMuch)
        {
            ShiftOffsetBy(collider2D, howMuch, 0);
        }

        public static void ShiftOffsetYBy(this CapsuleCollider2D collider2D, float howMuch)
        {
            ShiftOffsetBy(collider2D, 0, howMuch);
        }
    }
}