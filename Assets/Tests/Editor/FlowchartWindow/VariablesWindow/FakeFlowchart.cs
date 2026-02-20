using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils.Tests
{
    // A tiny stand-in for your Flowchart data model.
    // Rename members to match your real Flowchart API (events + variable accessors).
    internal class FakeFlowchart
    {
        public event Action VariablesChanged;

        public readonly List<string> Variables = new();

        public void AddVariable(string name = null)
        {
            Variables.Add(name ?? $"var_{Variables.Count}");
            VariablesChanged?.Invoke();
        }

        public void RemoveLast()
        {
            if (Variables.Count == 0) return;
            Variables.RemoveAt(Variables.Count - 1);
            VariablesChanged?.Invoke();
        }
    }

    
}