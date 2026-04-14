using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Context menu for Flowchart Blocks in the Flowchart Window viewport. Contains buttons for copying, 
    /// cutting, and deleting the selected Block(s).
    /// </summary>
    public class BlockContextMenu : VisualElement, IDisposable
    {
        private static readonly string uxmlPath = "UIToolkitTemplates/ContextMenus/BlockContextMenu";

        public BlockContextMenu()
        {
            var visualTreeAsset = Resources.Load<VisualTreeAsset>(uxmlPath);
            if (visualTreeAsset == null)
            {
                Debug.LogError($"Failed to load VisualTreeAsset at path: {uxmlPath}");
                return;
            }
            VisualElement root = visualTreeAsset.Instantiate();
            Add(root);

            RegisterControls();
            ToggleSubs(true);
        }

        private void RegisterControls()
        {
            _copyButton = this.Q<Button>("CopyButton");
            _cutButton = this.Q<Button>("CutButton");
            _deleteButton = this.Q<Button>("DeleteButton");
        }

        private Button _copyButton, _cutButton, _deleteButton;

        private void ToggleSubs(bool on)
        {
            if (_copyButton == null || _cutButton == null || _deleteButton == null)
            {
                return;
            }

            if (on)
            {
                _copyButton.clicked += OnCopyButtonClicked;
                _cutButton.clicked += OnCutButtonClicked;
                _deleteButton.clicked += OnDeleteButtonClicked;
            }
            else
            {
                _copyButton.clicked -= OnCopyButtonClicked;
                _cutButton.clicked -= OnCutButtonClicked;
                _deleteButton.clicked -= OnDeleteButtonClicked;
            }
        }

        private void OnCopyButtonClicked()
        {
            CopyButtonClicked?.Invoke();
        }

        public event Action CopyButtonClicked = delegate { };

        private void OnCutButtonClicked()
        {
            CutButtonClicked?.Invoke();
        }

        public event Action CutButtonClicked = delegate { };

        private void OnDeleteButtonClicked()
        {
            DeleteButtonClicked?.Invoke();
        }

        public event Action DeleteButtonClicked = delegate { };

        public void Dispose()
        {
            ToggleSubs(false);
            _copyButton = null;
            _cutButton = null;
            _deleteButton = null;
            TargetBlock = null;
            FlowchartContext = null;
        }

        public Block TargetBlock { get; set; }
        public FlowchartContext FlowchartContext { get; set; }

    }
}