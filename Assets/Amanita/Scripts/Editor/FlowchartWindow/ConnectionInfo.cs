using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public readonly struct ConnectionInfo
    {
        public readonly Rect FromRect;
        public readonly Rect ToRect;
        public readonly bool Highlight;

        public ConnectionInfo(Rect fromRect, Rect toRect, bool highlight)
        {
            FromRect = fromRect;
            ToRect = toRect;
            Highlight = highlight;
        }
    }
}