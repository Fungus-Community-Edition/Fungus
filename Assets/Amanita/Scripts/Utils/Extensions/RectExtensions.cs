using UnityEngine;

namespace Amanita
{
    public static class RectExtensions
    {
        public static Rect Shift(this Rect r, float x, float y, float w, float h)
        {
            return new Rect(r.x + x, r.y + y, w, h);
        }

        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }

        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }

        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale.x;
            result.xMax *= scale.x;
            result.yMin *= scale.y;
            result.yMax *= scale.y;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }

        public static Rect SnapPosition(this Rect rect, float snapInterval)
        {
            var tmp = rect;
            var x = tmp.position.x;
            var y = tmp.position.y;
            tmp.position = new Vector2(Mathf.RoundToInt(x / snapInterval) * snapInterval, Mathf.RoundToInt(y / snapInterval) * snapInterval);
            return tmp;
        }

        public static Rect SnapWidth(this Rect rect, float snapInterval)
        {
            var tmp = rect;
            tmp.width = Mathf.RoundToInt(tmp.width / snapInterval) * snapInterval;
            return tmp;
        }
    }

}