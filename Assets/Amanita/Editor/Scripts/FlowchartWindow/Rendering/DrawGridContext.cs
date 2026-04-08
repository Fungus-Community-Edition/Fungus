using System;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [System.Serializable]
    public class DrawGridContext : IDisposable
    {
        [SerializeField] private float _gridLineSpacingSize = 20f;
        [SerializeField] private Color _gridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        public virtual void Dispose()
        {
            GridLineSpacingSize = 0;
            GridLineColor = default;
        }

        public virtual float GridLineSpacingSize
        {
            get => _gridLineSpacingSize;
            set => _gridLineSpacingSize = value;
        }
        public virtual Color GridLineColor
        {
            get => _gridLineColor;
            set => _gridLineColor = value;
        }
    }

}