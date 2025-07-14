using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UIToolkitLabel = UnityEngine.UIElements.Label;
using Amanita.VScripting;
using System;

namespace Amanita.EditorUtils
{
    public class UitkVariableListAdaptor : IDisposable
    {
        public Flowchart TargetFlowchart { get; }
        private SerializedProperty variablesProp;
        private SerializedObject flowchartSO;
        public static readonly int ReorderListSkirts = 50;

        // Backing list for ListView
        private List<Variable> varsList => TargetFlowchart.Variables;

        public UitkVariableListAdaptor(SerializedProperty variablesProp, Flowchart flowchart)
        {
            this.variablesProp = variablesProp;
            this.flowchartSO = variablesProp.serializedObject;
            this.TargetFlowchart = flowchart;
            ListenForEvents();
        }

        protected virtual void ListenForEvents()
        {
            Debug.Log("Listening for flowchart events");
            this.TargetFlowchart.VariableAdded += OnVariableAddedOrRemoved;
            this.TargetFlowchart.VariableRemoved += OnVariableAddedOrRemoved;
            Undo.undoRedoPerformed += RefreshListView;
        }

        protected virtual void OnVariableAddedOrRemoved(IVariable added)
        {
            RefreshListView();
        }

        protected virtual void RefreshListView()
        {
            GameObject selectedGO = Selection.activeGameObject;
            bool hasOurFlowchart = selectedGO.GetComponent<Flowchart>() == TargetFlowchart;
            if (selectedGO != null && hasOurFlowchart)
            {
                Debug.Log("Rebuilding list view");
                listView.itemsSource = varsList;
                listView?.Rebuild(); // Since RefreshItems leads to weird bugs
            }

        }

        /// <summary>
        /// Builds and returns a UI Toolkit foldout containing:
        ///  - An "Add" button (dropdown) to create new Variables
        ///  - A ListView of existing Variables (type | key | value | scope)
        /// </summary>
        public VisualElement CreateVariablesUI()
        {
            var root = new Foldout { text = "Variables" };
            root.AddToClassList("variable-list-root");

            addButton = new Button(ShowAddMenu) { text = "+ Add Variable" };
            addButton.AddToClassList("variable-add-button");
            root.Add(addButton);

            listView = new ListView
            {
                itemsSource = varsList,
                makeItem = MakeVariableRow,
                bindItem = BindVariableRow,
                fixedItemHeight = (int)(EditorGUIUtility.singleLineHeight + 4),
                selectionType = SelectionType.None,
                style =
                {
                    flexGrow = 1
                }
            };
            root.Add(listView);

            return root;
        }

        protected Button addButton;

        protected ListView listView;

        // Called by `makeItem`
        protected virtual VisualElement MakeVariableRow()
        {
            var row = new VisualElement { name = "variable-row" };
            row.style.flexDirection = FlexDirection.Row;

            // 1. Type label
            var typeLabel = PrepTypeLabel();
            UIToolkitLabel PrepTypeLabel()
            {
                var typeLabel = new UIToolkitLabel()
                {
                    name = "type"
                };

                typeLabel.style.width = typeLabelWidth;
                typeLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                return typeLabel;
            }

            row.Add(typeLabel);

            // 2. Key text field
            var keyField = PrepKeyField();
            TextField PrepKeyField()
            {
                var keyField = new TextField { name = "key" };
                keyField.style.width = keyFieldWidth;
                keyField.style.whiteSpace = WhiteSpace.NoWrap;
                keyField.style.overflow = Overflow.Hidden;

                return keyField;
            }
            row.Add(keyField);

            // 3. Value field (will bind to SerializedProperty of the Variable component)
            var valueField = new PropertyField { name = "value", style = { width = valueFieldWidth } };
            valueField.style.whiteSpace = WhiteSpace.NoWrap;
            valueField.style.overflow = Overflow.Hidden;
            row.Add(valueField);

            // 4. Scope enum dropdown
            var scopeField = new EnumField { name = "scope", style = { width = scopeFieldWidth } };
            row.Add(scopeField);

            // 5. Remove button
            var removeBtn = new Button(() => RemoveCurrentRow(row)) { text = "–" };
            removeBtn.AddToClassList("variable-remove-button");
            row.Add(removeBtn);

            return row;
        }

        protected static int typeLabelWidth = 80,
            keyFieldWidth = 100,
            valueFieldWidth = 150,
            scopeFieldWidth = 70;

        // Called by `bindItem`
        protected virtual void BindVariableRow(VisualElement element, int index)
        {
            var variable = varsList[index];

            // Need to account for when a var was just deleted
            while (varsList.Count > 0 && variable == null)
            {
                varsList.RemoveAt(index);
                bool validIndex = index < varsList.Count;
                if (validIndex)
                {
                    variable = varsList[index];
                }
                else
                {
                    return;
                }
            }

            if (variable == null)
            {
                return;
            }

            var flowchart = TargetFlowchart;

            // 1) Type
            string newTypeLabelText = variable.GetType().Name;
            // We don't want "Variable" to be in the label, so...
            int l = "Variable".Length;
            newTypeLabelText = newTypeLabelText.Substring(0, newTypeLabelText.Length - l);
            var typeNameField = element.Q<UIToolkitLabel>("type");
            typeNameField.text = newTypeLabelText;

            // 2) Key
            var keyField = element.Q<TextField>("key");
            keyField.value = variable.Key;
            keyField.RegisterValueChangedCallback(evt =>
            {
                if (variable == null)
                {
                    return;
                }
                Undo.RecordObject(variable, "Change Variable Key");
                variable.Key = flowchart.GetUniqueVariableKey(evt.newValue, variable);
                flowchartSO.ApplyModifiedProperties();
            });

            // 3) Value
            var valProp = new SerializedObject(variable).FindProperty("value");
            var valField = element.Q<PropertyField>("value");
            valField.BindProperty(valProp);

            // 4) Scope
            var scopeProp = new SerializedObject(variable).FindProperty("scope");
            var scopeField = element.Q<EnumField>("scope");
            scopeField.Init(variable.Scope);
            scopeField.RegisterValueChangedCallback(evt =>
            {
                scopeProp.enumValueIndex = System.Convert.ToInt32(evt.newValue);
                scopeProp.serializedObject.ApplyModifiedProperties();
            });

            // 5) Remove ⇒ store index on the row for lookup
            element.userData = index;
        }

        protected virtual void RemoveCurrentRow(VisualElement row)
        {
            int index = (int)row.userData;
            // Destroy component and remove from list
            var varToRemove = varsList[index];

            Undo.DestroyObjectImmediate(varToRemove);

            // Apply & refresh UI
            variablesProp.serializedObject.ApplyModifiedProperties();
            OnVariableAddedOrRemoved(null);
            // Note: The ListView source (varsList) has mutated,
            // Unity will automatically call bindItem for visible rows.
        }

        protected virtual void ShowAddMenu()
        {
            // Use the same popup as IMGUI version
            // We can't get a Rect here easily, but Supply zero‐rect for DoAddVariable
            Rect rect = new Rect();

            if (addButton != null)
            {
                rect = addButton.worldBound;
            }

            VariableSelectPopupWindowContent.DoAddVariable(rect, "", TargetFlowchart);
        }

        public virtual void Dispose()
        {
            UnregisterCallbacks();
        }

        protected virtual void UnregisterCallbacks()
        {
            if (this.TargetFlowchart != null)
            {
                Debug.Log("No longer listening for flowchart events");
                this.TargetFlowchart.VariableAdded -= OnVariableAddedOrRemoved;
                this.TargetFlowchart.VariableRemoved -= OnVariableAddedOrRemoved;
            }
            Undo.undoRedoPerformed -= RefreshListView;
        }

    }
}