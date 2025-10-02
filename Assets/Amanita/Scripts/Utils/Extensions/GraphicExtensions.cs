using UnityEngine;
namespace Amanita
{
    public static class GraphicExtensions 
    {
        public static void SetAlpha(this UnityEngine.UI.Graphic graphic, float alpha)
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

    }
}