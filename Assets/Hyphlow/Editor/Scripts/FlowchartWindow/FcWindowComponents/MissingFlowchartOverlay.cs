using System;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace AtMycelia.Hyphlow.EditorUtils
{
    sealed class MissingFlowchartOverlay : IDisposable
    {
        private readonly Action refreshHandler;
        private UitkLabel errorLabel;
        private Button refreshButton;

        public MissingFlowchartOverlay(Action refreshHandler)
        {
            this.refreshHandler = refreshHandler ?? throw new ArgumentNullException(nameof(refreshHandler));
        }

        public void Show(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            EnsureErrorLabel();
            EnsureRefreshButton();

            if (errorLabel.parent == null)
            {
                root.Add(errorLabel);
            }

            if (refreshButton.parent == null)
            {
                root.Add(refreshButton);
            }
        }

        public void Hide()
        {
            errorLabel?.RemoveFromHierarchy();
            refreshButton?.RemoveFromHierarchy();
        }

        public void Dispose()
        {
            Hide();
            errorLabel = null;
            refreshButton = null;
        }

        private void EnsureErrorLabel()
        {
            if (errorLabel != null)
            {
                return;
            }

            errorLabel = new UitkLabel("No Flowcharts found in the scene. ");
            errorLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            errorLabel.style.fontSize = 48;
            errorLabel.style.color = Color.yellow;
        }

        private void EnsureRefreshButton()
        {
            if (refreshButton != null)
            {
                return;
            }

            refreshButton = new Button(refreshHandler);
            refreshButton.text = "Refresh";
            refreshButton.style.alignSelf = Align.Center;
            Vector2 buttonSize = new Vector2(200, 50);
            refreshButton.style.width = buttonSize.x;
            refreshButton.style.height = buttonSize.y;
            refreshButton.style.fontSize = 24;
        }
    }

}