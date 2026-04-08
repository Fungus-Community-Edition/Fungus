using UnityEngine;

namespace AtMycelia.Hyphlow
{ 
    public class HyphlowTextAreaAttribute : PropertyAttribute
    {
        public int MinLines { get; private set; }
        public int MaxLines { get; private set; }
        public HyphlowTextAreaAttribute(int minLines = 3, int maxLines = 10)
        {
            MinLines = minLines;
            MaxLines = maxLines;
        }
    }
}