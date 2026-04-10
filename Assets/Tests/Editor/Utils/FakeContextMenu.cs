using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    class FakeContextMenu : IContextMenu
    {
        public IList<IContextMenuItem> Items
        {
            get { return new List<IContextMenuItem>(items); } // We don't want to let callers mutate the list directly
        }

        protected IList<IContextMenuItem> items = new List<IContextMenuItem>();
        public Rect DropDownRect;

        public void AddItem(GUIContent content, bool on, Action callback)
        {
            ContextMenuItem toAdd = new ContextMenuItem
            {
                Content = content,
                Disabled = false,
                Callback = callback
            };
            items.Add(toAdd);
        }

        public void AddDisabledItem(GUIContent content)
        {
            ContextMenuItem disabledItem = new ContextMenuItem
            {
                Content = content,
                Disabled = true,
                Callback = null
            };
            items.Add(disabledItem);
        }

        public void AddSeparator(string path = "")
        {
            ContextMenuItem separator = new ContextMenuItem
            {
                Content = new GUIContent("---"),
                Disabled = false,
                Callback = null
            };
            items.Add(separator);
        }

        public void DropDown(Rect activationRect)
        {
            DropDownRect = activationRect;
        }
    }

    class FakeContextMenuFactory : IContextMenuFactory
    {
        public FakeContextMenu LastMenu;
        public IContextMenu Create()
        {
            LastMenu = new FakeContextMenu();
            return LastMenu;
        }
    }
}