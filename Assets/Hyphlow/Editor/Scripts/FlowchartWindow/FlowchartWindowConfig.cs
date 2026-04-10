using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
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

        [Header("Visual Tree Assets")]
        [SerializeField] private VisualTreeAsset _blockUxml;

        [Header("Style Sheets")]
        [SerializeField] private StyleSheet _blockStyleSheet;
        [SerializeField] private StyleSheet _selectedBlockStyleSheet;

        public string FlowchartWindowTitle => _flowchartWindowTitle;
        public Vector2 WindowMinSize => _windowMinSize;

        public float MinZoom => _minZoom;
        public float MaxZoom => _maxZoom;
        public float ZoomStep => _zoomStep;
        public float DefaultZoom => _defaultZoom;
        public bool SnapBlocksToGrid => _snapBlocksToGrid;
        
        public DrawGridContext GridDrawConfig => _gridDrawConfig;

        public VisualTreeAsset BlockUxml => _blockUxml;
        public StyleSheet BlockStyleSheet => _blockStyleSheet;
        public StyleSheet SelectedBlockStyleSheet => _selectedBlockStyleSheet;
    }
}