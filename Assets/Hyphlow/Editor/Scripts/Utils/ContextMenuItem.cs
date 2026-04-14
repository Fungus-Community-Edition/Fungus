using System;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class ContextMenuItem : IContextMenuItem
    {
        public virtual GUIContent Content { get; set; }
        public virtual bool Disabled { get; set; }
        public virtual Action Callback { get; set; }
        public virtual string Group { get; set; }
    }
}