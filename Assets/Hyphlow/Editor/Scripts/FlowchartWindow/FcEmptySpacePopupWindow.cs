using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class FcEmptySpacePopupWindow : VisualElement, IDisposable
    {
        [SerializeField]
        private VisualTreeAsset visualTreeAsset = default;

        private static readonly string uxmlPath = "UIToolkitTemplates/ContextMenus/FlowchartEmptySpacePopupMenu";
        // ^Required to be a relative path from the Resources folder, without the file extension

        private static readonly Color GradientTop = new Color(1f, 1f, 1f, 1);
        private static readonly Color GradientBottom = new Color(0f, 0f, 0f, 0.25f);

        public FcEmptySpacePopupWindow()
        {
            visualTreeAsset = Resources.Load<VisualTreeAsset>(uxmlPath);
            if (visualTreeAsset == null)
            {
                Debug.LogError($"Failed to load VisualTreeAsset at path: {uxmlPath}");
                return;
            }
            _isDisposed = false;
            VisualElement root = visualTreeAsset.Instantiate();
            Add(root);
            RegisterControls();
            ToggleSubs(true);
        }

        private bool _isDisposed;

        private void RegisterControls()
        {
            _addButton = this.Q<Button>("AddButton");
            _pasteButton = this.Q<Button>("PasteButton");

            ApplyGradient(_addButton);
            ApplyGradient(_pasteButton);
        }

        private Button _addButton, _pasteButton;

        private static void ApplyGradient(VisualElement element)
        {
            GradientDrawer.AttachVerticalGradient(element, GradientTop, GradientBottom);
        }

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                _addButton.clicked += OnAddButtonClicked;
                _pasteButton.clicked += OnPasteButtonClicked;
            }
            else
            {
                _addButton.clicked -= OnAddButtonClicked;
                _pasteButton.clicked -= OnPasteButtonClicked;
            }
        }

        private void OnAddButtonClicked()
        {
            AddButtonClicked.Invoke();
        }

        public event Action AddButtonClicked = delegate { };

        private void OnPasteButtonClicked()
        {
            PasteButtonClicked.Invoke();
        }

        public event Action PasteButtonClicked = delegate { };

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            ToggleSubs(false);
            this.RemoveFromHierarchy();
        }

        public bool PasteButtonEnabled
        {
            get
            {
                if (_isDisposed || _pasteButton == null)
                {
                    return false;
                }
                return _pasteButton.enabledInHierarchy;
            }
            set
            {
                if (_isDisposed || _pasteButton == null)
                {
                    return;
                }

                _pasteButton.SetEnabled(value);
            }
        }
    }
}