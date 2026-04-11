// Original code by Martin Ecker (http://martinecker.com)

using UnityEngine;
using AtMycelia.Graphics;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class EditorZoomArea
    {
        private static Matrix4x4 _prevGuiMatrix;
        private static Vector2 offset = new Vector2(2.0f, 19.0f);
        public static Vector2 Offset { get { return offset; } set { offset = value; } }
        
        public static Rect Begin(float zoomScale, Rect screenCoordsArea)
        {
            GUI.EndGroup();        // End the group Unity begins automatically for an EditorWindow to clip out the window tab. This allows us to draw outside of the size of the EditorWindow.
            
            Rect clippedArea = screenCoordsArea.ScaleSizeBy(1.0f / zoomScale, screenCoordsArea.TopLeft());
            clippedArea.position += offset;
            GUI.BeginGroup(clippedArea);
            
            _prevGuiMatrix = GUI.matrix;

            Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
            GUI.matrix = translation * scale * translation.inverse * GUI.matrix;
            

            return clippedArea;
        }
        
        public static void End()
        {
            GUI.matrix = _prevGuiMatrix;
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(offset.x, offset.y, Screen.width, Screen.height));
        }
    }
}