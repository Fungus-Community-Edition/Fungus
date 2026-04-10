using System;
using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public interface IContextMenuFactory
    {
        IContextMenu Create();
    }

    public interface IContextMenu
    {
        IList<IContextMenuItem> Items { get; }
        void AddItem(GUIContent content, bool on, Action onItemClicked);
        void AddDisabledItem(GUIContent content);
        void AddSeparator(string path = "");
        void DropDown(Rect activationRect);
    }

    public interface IContextMenuItem
    {
        bool Disabled { get; }
        GUIContent Content { get; }
        System.Action Callback { get; }
    }
}