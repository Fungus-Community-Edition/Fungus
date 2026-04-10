using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils
{
    class GenericMenuFactory : IContextMenuFactory
    {
        public IContextMenu Create() => new GenericMenuAdapter();
    }

    class GenericMenuAdapter : IContextMenu
    {
        readonly GenericMenu _menu = new GenericMenu();
        public void AddItem(GUIContent label, bool on, Action onItemClicked = null)
        {
            ContextMenuItem newItem = new ContextMenuItem()
            {
                Content = label,
                Disabled = false,
                Callback = onItemClicked
            };

            items.Add(newItem);
            _menu.AddItem(label, on, () => onItemClicked());
        }
        public void AddDisabledItem(GUIContent label)
        {
            ContextMenuItem newItem = new ContextMenuItem()
            {
                Content = label,
                Disabled = true,
            };

            items.Add(newItem);
            _menu.AddDisabledItem(label);
        }
        public void AddSeparator(string p = "")
        {
            ContextMenuItem newItem = new ContextMenuItem()
            {
                Content = new GUIContent(p),
                Disabled = true
            };
            _menu.AddSeparator(p);
        }
        public void DropDown(Rect r) => _menu.DropDown(r);

        // More just a way to let people see the contents
        public virtual IList<IContextMenuItem> Items => new List<IContextMenuItem>(items);
        protected IList<IContextMenuItem> items = new List<IContextMenuItem>();
    }
}