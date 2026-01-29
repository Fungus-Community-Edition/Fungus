using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public class FlowchartWindowConfig : ScriptableObject
    {
        [Header("Window Form Settings")]
        [SerializeField] private string _flowchartWindowTitle = "Flowchart Window Uitk";
        [SerializeField] private Vector2 _windowMinSize = new Vector2(800, 500);

        [Header("Zoom Settings")]
        [SerializeField] private float _minZoom = 0.5f;
        [SerializeField] private float _maxZoom = 1f;
        [SerializeField] private float _zoomStep = 0.1f;
        [SerializeField] private float _defaultZoom = 1f;
        [SerializeField] private bool _snapBlocksToGrid = true;

        [SerializeField] private DrawGridContext _gridDrawConfig = new DrawGridContext()
        {
            GridLineSpacingSize = 50f,
            GridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.2f)
        };

        public string FlowchartWindowTitle => _flowchartWindowTitle;
        public Vector2 WindowMinSize => _windowMinSize;

        public float MinZoom => _minZoom;
        public float MaxZoom => _maxZoom;
        public float ZoomStep => _zoomStep;
        public float DefaultZoom => _defaultZoom;
        public bool SnapBlocksToGrid => _snapBlocksToGrid;
        
        public DrawGridContext GridDrawConfig => _gridDrawConfig;
    }
}