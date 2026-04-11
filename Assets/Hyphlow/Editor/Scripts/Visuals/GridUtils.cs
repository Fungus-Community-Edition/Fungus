// File: GridUtils.cs
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class GridUtils
    {
        /// <summary>
        /// Returns X positions (in window space) of vertical grid lines.
        /// </summary>
        public static IList<float> GetVerticalLinePositions(
            float scrollX, float viewWidth, float spacing)
        {
            var positions = new List<float>();
            // The first line appears at (spacing - (scrollX % spacing)) % spacing
            float offset = (spacing - (scrollX % spacing)) % spacing;
            // Keep adding lines until we exceed viewWidth
            for (float x = offset; x < viewWidth; x += spacing)
            {
                positions.Add(x);
            }
            return positions;
        }

        /// <summary>
        /// Returns Y positions (in window space) of horizontal grid lines.
        /// </summary>
        public static IList<float> GetHorizontalLinePositions(
            float scrollY, float viewHeight, float spacing)
        {
            var positions = new List<float>();
            float offset = (spacing - (scrollY % spacing)) % spacing;
            for (float y = offset; y < viewHeight; y += spacing)
            {
                positions.Add(y);
            }
            return positions;
        }
    }
}