using UnityEngine;

namespace AtMycelia.Physics
{
    public static class LayerExtensions
    {
        public static bool IsOnLayer(this GameObject gameObject, LayerMask layerMask)
        {
            bool result = (layerMask & 1 << gameObject.layer) > 0;
            return result;
        }

        public static bool IsOnLayer(this Collider collider, LayerMask layerMask)
        {
            return collider.gameObject.IsOnLayer(layerMask);
        }

        public static bool IsOnLayer(this Collider2D collider, LayerMask layerMask)
        {
            return collider.gameObject.IsOnLayer(layerMask);
        }

        public static bool IsOnLayer(this Collision collision, LayerMask layerMask)
        {
            return collision.gameObject.IsOnLayer(layerMask);
        }

        public static bool IsOnLayer(this Collision2D collision, LayerMask layerMask)
        {
            return collision.gameObject.IsOnLayer(layerMask);
        }
    }
}