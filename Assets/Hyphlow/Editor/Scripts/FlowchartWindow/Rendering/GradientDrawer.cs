using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Provides utility methods for drawing gradients on UITK elements in the Unity Editor.
    /// </summary>
    public static class GradientDrawer
    {
        public static void AttachVerticalGradient(VisualElement element, Color top, Color bottom)
        {
            if (element == null)
            {
                return;
            }

            element.generateVisualContent += context => DrawVerticalGradient(context, element, top, bottom);
        }

        public static void DrawVerticalGradient(MeshGenerationContext context, VisualElement element, Color top, Color bottom)
        {
            if (element == null)
            {
                return;
            }

            float width = element.layout.width;
            float height = element.layout.height;
            if (width <= 0f || height <= 0f)
            {
                return;
            }

            var mesh = context.Allocate(4, 6);

            var topLeft = new Vector3(0f, 0f, Vertex.nearZ);
            var topRight = new Vector3(width, 0f, Vertex.nearZ);
            var bottomRight = new Vector3(width, height, Vertex.nearZ);
            var bottomLeft = new Vector3(0f, height, Vertex.nearZ);

            mesh.SetNextVertex(new Vertex { position = topLeft, tint = top });
            mesh.SetNextVertex(new Vertex { position = topRight, tint = top });
            mesh.SetNextVertex(new Vertex { position = bottomRight, tint = bottom });
            mesh.SetNextVertex(new Vertex { position = bottomLeft, tint = bottom });

            mesh.SetNextIndex(0);
            mesh.SetNextIndex(1);
            mesh.SetNextIndex(2);
            mesh.SetNextIndex(2);
            mesh.SetNextIndex(3);
            mesh.SetNextIndex(0);
        }
    }
}