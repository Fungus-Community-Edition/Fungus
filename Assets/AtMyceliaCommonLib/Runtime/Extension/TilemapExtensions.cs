using UnityEngine;
using UnityEngine.Tilemaps;

namespace AtMycelia.Graphics
{
    public static class TilemapExtensions
    {
        public static void SetAlpha(this Tilemap tilemap, float newAlpha)
        {
            Color col = tilemap.color;
            col.a = newAlpha;
            tilemap.color = col;
        }
    }
}