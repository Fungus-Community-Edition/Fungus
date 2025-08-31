using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UIToolkitLabel = UnityEngine.UIElements.Label;
using System;
using System.Linq;
using UnityObject = UnityEngine.Object;

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

        public virtual void RefreshListView()
        {
            GameObject selectedGO = Selection.activeGameObject;
            bool hasOurFlowchart = selectedGO != null && selectedGO.GetComponent<Flowchart>() == TargetFlowchart;
            if (selectedGO != null && hasOurFlowchart && VarsList != null)
            {
                Debug.Log("Rebuilding list view");
                listView.itemsSource = VarsList;
                listView?.Rebuild(); // Since RefreshItems leads to weird bugs
            }

        }

        public List<IVariable> VarsList => TargetFlowchart.Variables.Cast<IVariable>().ToList();

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
                itemsSource = VarsList,
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
                // Placeholder, will be replaced in BindVariableRow
                var valuePlaceholder = new VisualElement { name = "value" };
                valuePlaceholder.style.width = valueFieldWidth;
                row.Add(valuePlaceholder);
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
            if (flowchart == null) return;

            IVariable varToRepresent = flowchart.GetVariable(index);
            if (varToRepresent == null) return;

            // Type label
            string newTypeLabelText = varToRepresent.GetType().Name;
            int varWordLength = "Variable".Length;
            newTypeLabelText = newTypeLabelText.Substring(0, newTypeLabelText.Length - varWordLength);
            var typeNameField = element.Q<UIToolkitLabel>("type");
            if (typeNameField != null)
            {
                typeNameField.text = newTypeLabelText;
            }

            // Key field
            var keyField = element.Q<TextField>("key");
            if (keyField != null)
            {
                keyField.value = varToRepresent.Key;
                keyField.RegisterValueChangedCallback(evt =>
                {
                    if (varToRepresent == null)
                    {
                        return;
                    }
                    Undo.RecordObject(varToRepresent as UnityObject, "Change Variable Key");
                    varToRepresent.Key = flowchart.GetUniqueVariableKey(evt.newValue, varToRepresent);
                    flowchartSO.ApplyModifiedProperties();
                });
            }

            HandleValueField();
            void HandleValueField()
            {
                var valueContainer = element.Q<VisualElement>("value");
                valueContainer.Clear();
                VisualElement fieldToAdd = null;
                Type varType = varToRepresent.ValueType;
                if (varToRepresent.Value != null)
                    varType = varToRepresent.Value.GetType();

                if (varToRepresent is FloatVariable floatVar)
                {
                    var floatField = new FloatField { value = floatVar.Value };
                    floatField.RegisterValueChangedCallback(evt =>
                    {
                        Undo.RecordObject(floatVar, "Change Float Variable Value");
                        floatVar.Value = evt.newValue;
                        EditorUtility.SetDirty(floatVar);
                    });
                    fieldToAdd = floatField;
                }
                else if (varToRepresent is IntegerVariable boolVar)
                {
                    var intField = new IntegerField { value = boolVar.Value };
                    intField.RegisterValueChangedCallback(evt =>
                    {
                        Undo.RecordObject(boolVar, "Change Integer Variable Value");
                        boolVar.Value = evt.newValue;
                        EditorUtility.SetDirty(boolVar);
                    });
                    fieldToAdd = intField;
                }
                else if (varToRepresent is BooleanVariable booleanVar)
                {
                    var boolField = new Toggle { value = booleanVar.Value };
                    boolField.RegisterValueChangedCallback(evt =>
                    {
                        Undo.RecordObject(booleanVar, "Change Integer Variable Value");
                        booleanVar.Value = evt.newValue;
                        EditorUtility.SetDirty(booleanVar);
                    });
                    fieldToAdd = boolField;
                }
                else if (varToRepresent is StringVariable strVar)
                {
                    var strField = new TextField { value = strVar.Value };
                    strField.RegisterValueChangedCallback(evt =>
                    {
                        Undo.RecordObject(strVar, "Change Float Variable Value");
                        strVar.Value = evt.newValue;
                        EditorUtility.SetDirty(strVar);
                    });
                    fieldToAdd = strField;
                }
                else if (typeof(UnityObject).IsAssignableFrom(varType))
                {
                    var varAsObj = varToRepresent as UnityObject;
                    var objField = new ObjectField
                    {
                        objectType = varType,
                        value = varToRepresent.Value as UnityObject,
                    };
                    objField.RegisterValueChangedCallback(evt =>
                    {
                        var so = new SerializedObject(varAsObj);
                        var valProp = so.FindProperty("value");
                        Undo.RecordObject(varAsObj, $"Change {varType.Name} Variable Value");
                        valProp.objectReferenceValue = evt.newValue as UnityObject;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(varAsObj);
                    });
                    fieldToAdd = objField;
                }
                //else if (varToRepresent is AudioClipVariable audioVar)
                //{
                //    var objField = new ObjectField
                //    {
                //        objectType = typeof(AudioClip),
                //        value = audioVar.Value as AudioClip,
                //    };
                //    objField.RegisterValueChangedCallback(evt =>
                //    {
                //        var so = new SerializedObject(audioVar);
                //        var valProp = so.FindProperty("value");
                //        Undo.RecordObject(audioVar, "Change AudioClip Variable Value");
                //        valProp.objectReferenceValue = evt.newValue as AudioClip;
                //        so.ApplyModifiedProperties();
                //        EditorUtility.SetDirty(audioVar);
                //    });
                //    fieldToAdd = objField;
                //}

                if (fieldToAdd == null)
                {
                    Debug.LogWarning($"Could not set up proper value field for variable of type {varType.Name}");
                }
                valueContainer.Add(fieldToAdd);
                
            }

            // Scope field
            var scopeField = element.Q<EnumField>("scope");
            if (scopeField != null)
            {
                var so = new SerializedObject(varToRepresent as UnityEngine.Object);
                so.Update();
                var scopeProp = so.FindProperty("scope");
                if (scopeProp != null)
                {
                    scopeField.Init(varToRepresent.Scope);
                    scopeField.RegisterValueChangedCallback(evt =>
                    {
                        scopeProp.enumValueIndex = Convert.ToInt32(evt.newValue);
                        scopeProp.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            element.userData = index;
        }

        protected virtual void RemoveCurrentRow(VisualElement row)
        {
            int index = (int)row.userData;
            // Destroy component and remove from list
            var varToRemove = VarsList[index] as UnityEngine.Object;

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