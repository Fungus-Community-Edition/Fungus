using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UIToolkitLabel = UnityEngine.UIElements.Label;
using System;
using System.Linq;

namespace Amanita.VScripting.EditorUtils
{
    // Backing list for ListView
    public class UitkVariableListAdaptor : IDisposable
    {
        public UitkVariableListAdaptor(SerializedProperty variablesProp, Flowchart flowchart)
        {
            this.variablesProp = variablesProp;
            this.flowchartSO = variablesProp.serializedObject;
            this.TargetFlowchart = flowchart;
            ListenForEvents();
        }

        protected SerializedProperty variablesProp;
        protected SerializedObject flowchartSO;
        public Flowchart TargetFlowchart { get; }

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
            if (selectedGO != null && hasOurFlowchart && varsList != null)
            {
                Debug.Log("Rebuilding list view");
                listView.itemsSource = varsList;
                listView?.Rebuild(); // Since RefreshItems leads to weird bugs
            }

        }

        protected List<IVariable> varsList => TargetFlowchart.Variables.Cast<IVariable>().ToList();

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

            
            PrepValueField(); // Will bind to SerializedProperty of the Variable component
            void PrepValueField()
            {
                PropertyField valueField = new PropertyField
                {
                    name = "value",
                    style =
                    {
                        width = valueFieldWidth
                    }
                };

                valueField.style.whiteSpace = WhiteSpace.NoWrap;
                valueField.style.overflow = Overflow.Hidden;
                row.Add(valueField);
            }

            PrepScopeField();
            void PrepScopeField()
            {
                var scopeField = new EnumField
                {
                    name = "scope",
                    style =
                    {
                        width = scopeFieldWidth
                    }
                };
                row.Add(scopeField);
            }

            PrepRemoveButton();
            void PrepRemoveButton()
            {
                var removeBtn = new Button(() => RemoveCurrentRow(row)) { text = "–" };
                removeBtn.style.width = removeButtonWidth;
                removeBtn.AddToClassList("variable-remove-button");
                row.Add(removeBtn);
            }

            return row;
        }

        protected static int typeLabelWidth = 80,
            keyFieldWidth = 100,
            valueFieldWidth = 150,
            scopeFieldWidth = 70,
            removeButtonWidth = 25;

        // Called by `bindItem`
        protected virtual void BindVariableRow(VisualElement element, int index)
        {
            var flowchart = TargetFlowchart;
            if (flowchart == null)
            {
                return;
            }

            IVariable varToRepresent = flowchart.GetVariable(index);

            // Need to account for when a var was just deleted
            while (flowchart.VariableCount > 0 && varToRepresent == null)
            {
                flowchart.RemoveVariable(index);
                bool validIndex = index < varsList.Count;
                if (validIndex)
                {
                    varToRepresent = flowchart.GetVariable(index);
                }
                else
                {
                    return;
                }
            }

            if (varToRepresent == null)
            {
                return;
            }

            HandleTypeLabel();
            void HandleTypeLabel()
            {
                string newTypeLabelText = varToRepresent.GetType().Name;
                // We don't want "Variable" to be in the label, so...
                int varWordLength = "Variable".Length;
                newTypeLabelText = newTypeLabelText.Substring(0, newTypeLabelText.Length - varWordLength);
                var typeNameField = element.Q<UIToolkitLabel>("type");

                if (typeNameField == null)
                {
                    Debug.LogWarning($"Missing a type label field for the Variables UI");
                    return;
                }

                typeNameField.text = newTypeLabelText;
            }

            HandleKeyField();
            void HandleKeyField()
            {
                var keyField = element.Q<TextField>("key");
                if (keyField == null)
                {
                    Debug.LogWarning($"Missing a key field for the Variables UI");
                    return;
                }
                keyField.value = varToRepresent.Key;
                keyField.RegisterValueChangedCallback(evt =>
                {
                    if (varToRepresent == null)
                    {
                        return;
                    }
                    Undo.RecordObject(varToRepresent as UnityEngine.Object, "Change Variable Key");
                    varToRepresent.Key = flowchart.GetUniqueVariableKey(evt.newValue, varToRepresent);
                    flowchartSO.ApplyModifiedProperties();
                });
            }

            HandleValueField();
            void HandleValueField()
            {
                var valProp = new SerializedObject(varToRepresent as UnityEngine.Object).FindProperty("value");
                var valField = element.Q<PropertyField>("value");

                if (valField == null)
                {
                    Debug.LogWarning($"Missing value field in Variables UI");
                    return;
                }
                valField.BindProperty(valProp);
            }

            HandleScopeField();
            void HandleScopeField()
            {
                var scopeProp = new SerializedObject(varToRepresent as UnityEngine.Object).FindProperty("scope");
                var scopeField = element.Q<EnumField>("scope");

                if (scopeField == null)
                {
                    Debug.LogWarning($"Missing scope field in Variables UI");
                    return;
                }

                scopeField.Init(varToRepresent.Scope);
                scopeField.RegisterValueChangedCallback(evt =>
                {
                    scopeProp.enumValueIndex = System.Convert.ToInt32(evt.newValue);
                    scopeProp.serializedObject.ApplyModifiedProperties();
                });
            }

            // 5) Remove ⇒ store index on the row for lookup
            element.userData = index;
        }

        protected virtual void RemoveCurrentRow(VisualElement row)
        {
            int index = (int)row.userData;
            // Destroy component and remove from list
            var varToRemove = varsList[index] as UnityEngine.Object;

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