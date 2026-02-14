using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace Amanita.VScripting.EditorUtils
{
    public class BlockContextMenuUitk : VisualElement, IDisposable
    {
        private static readonly string uxmlPath = "UIToolkitTemplates/BlockContextMenu";

        public BlockContextMenuUitk()
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
            Debug.Log("Block context menu: Copy button clicked.");
        }

        private void OnCutButtonClicked()
        {
            Debug.Log("Block context menu: Cut button clicked.");
        }

        private void OnDeleteButtonClicked()
        {
            Debug.Log("Block context menu: Delete button clicked.");
        }

        public void Dispose()
        {
            ToggleSubs(false);
            _copyButton = null;
            _cutButton = null;
            _deleteButton = null;
            TargetBlock = null;
        }

        public Block TargetBlock { get; set; }
    }
}