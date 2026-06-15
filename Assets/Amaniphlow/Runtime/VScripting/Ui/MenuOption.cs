using System;
using UnityEngine.UI;
using UnityEngine;

namespace AtMycelia.Amanita.DialogueSys
{
    /// <summary>
    /// Lightweight runtime handle for a MenuDialog option.
    /// Not a MonoBehaviour; not serialized. Holds runtime state and the callback.
    /// </summary>
    public class MenuOption
    {
        public int OptionIndex { get; internal set; }
        public string Id { get; } = Guid.NewGuid().ToString();

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                if (AttachedButton != null) UpdateButtonText();
            }
        }

        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                if (AttachedButton != null) AttachedButton.interactable = _interactable;
            }
        }

        public bool Hidden
        {
            get => _hidden;
            set
            {
                _hidden = value;
                if (AttachedButton != null) AttachedButton.gameObject.SetActive(!_hidden);
            }
        }

        // Button instance assigned by MenuDialog when it creates/uses a button.
        public Button AttachedButton { get; private set; }

        // Action to run when option is selected. Not serialized.
        public Action OnSelected;

        private string _text;
        private bool _interactable = true;
        private bool _hidden = false;

        internal MenuOption(int index, string text, bool interactable, bool hidden, Action onSelected)
        {
            OptionIndex = index;
            _text = text;
            _interactable = interactable;
            _hidden = hidden;
            OnSelected = onSelected;
        }

        internal void Attach(Button button)
        {
            if (AttachedButton != null)
            {
                Detach();
            }
            AttachedButton = button;
            UpdateButtonText();
            AttachedButton.interactable = _interactable;
            AttachedButton.gameObject.SetActive(!_hidden);
            AttachedButton.onClick.AddListener(HandleClick);
        }

        internal void Detach()
        {
            if (AttachedButton == null) return;
            AttachedButton.onClick.RemoveListener(HandleClick);
            AttachedButton = null;
        }

        private void UpdateButtonText()
        {
            var ta = new TextAdapter();
            ta.InitFromGameObject(AttachedButton.gameObject, true);
            if (ta.HasTextObject()) ta.Text = _text;
        }

        private void HandleClick()
        {
            try { OnSelected?.Invoke(); } catch (Exception) { /* swallow to avoid UI crashes */ }
        }
    }
}