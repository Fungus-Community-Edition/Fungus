using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomPropertyDrawer(typeof(VarDataTagFilter))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class VarDataTagFilterDrawer : PropertyDrawer
    {
        private const float LABEL_RATIO = 0.2f;
        private const float DRAG_HANDLE_WIDTH = 15f;
        private const float SPACING = 2f;
        private static readonly GUIStyle DragHandleStyle = new GUIStyle("RL DragHandle");

        private static readonly Dictionary<string, ReorderableList> ListsByPropertyPath =
            new Dictionary<string, ReorderableList>();

        // IMGUI fallback (used by current Block/EventHandler inspector)
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty listProp = property.FindPropertyRelative("_tagFilter");
            if (listProp == null)
            {
                EditorGUI.LabelField(position, label.text, "Tag filter property not found.");
                EditorGUI.EndProperty();
                return;
            }

            ReorderableList list = GetOrCreateList(property, listProp);
            Rect listRect = new Rect(position.x, position.y, position.width, list.GetHeight());
            list.DoList(listRect);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty listProp = property.FindPropertyRelative("_tagFilter");
            if (listProp == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            ReorderableList list = GetOrCreateList(property, listProp);
            return list.GetHeight();
        }

        private static ReorderableList GetOrCreateList(SerializedProperty property, SerializedProperty listProp)
        {
            string key = property.propertyPath;

            if (!ListsByPropertyPath.TryGetValue(key, out ReorderableList list) ||
                list.serializedProperty.serializedObject != property.serializedObject)
            {
                list = CreateList(listProp);
                ListsByPropertyPath[key] = list;
            }

            return list;
        }

        private static ReorderableList CreateList(SerializedProperty listProp)
        {
            ReorderableList list = new ReorderableList(
                listProp.serializedObject,
                listProp,
                draggable: true,
                displayHeader: true,
                displayAddButton: true,
                displayRemoveButton: true
            );

            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Tag Filters (Empty = Accept All)");
            };

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                if (element == null)
                {
                    return;
                }

                rect.y += SPACING;
                rect.height = EditorGUI.GetPropertyHeight(element);

                Rect handleRect = new Rect(rect.x, rect.y, DRAG_HANDLE_WIDTH, rect.height);
                GUI.Label(handleRect, GUIContent.none, DragHandleStyle);
                EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.Pan);

                Rect fieldRect = rect;
                fieldRect.xMin += DRAG_HANDLE_WIDTH;
                fieldRect.width = Mathf.Max(0, fieldRect.width);

                float prevLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = Mathf.RoundToInt(fieldRect.width * LABEL_RATIO);

                EditorGUI.PropertyField(fieldRect, element, GUIContent.none);

                EditorGUIUtility.labelWidth = prevLabelWidth;
            };

            list.elementHeightCallback = index =>
            {
                if (index < 0 || index >= list.serializedProperty.arraySize)
                {
                    return EditorGUIUtility.singleLineHeight;
                }

                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                if (element != null)
                {
                    return EditorGUI.GetPropertyHeight(element) + (SPACING * 2);
                }

                return EditorGUIUtility.singleLineHeight + (SPACING * 2);
            };

            list.onAddCallback = reorderableList =>
            {
                int index = reorderableList.serializedProperty.arraySize;
                reorderableList.serializedProperty.arraySize++;
                reorderableList.index = index;

                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                if (element != null)
                {
                    SerializedProperty valueProp = element.FindPropertyRelative("value");
                    SerializedProperty backingVarRefProp = element.FindPropertyRelative("backingVarRef");

                    if (valueProp != null)
                    {
                        valueProp.stringValue = string.Empty;
                    }

                    if (backingVarRefProp != null)
                    {
                        SerializedProperty itemIdProp = backingVarRefProp.FindPropertyRelative("itemId");
                        if (itemIdProp != null)
                        {
                            itemIdProp.intValue = Variable.InvalidID;
                        }
                    }
                }

                reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
            };

            list.onRemoveCallback = reorderableList =>
            {
                if (EditorUtility.DisplayDialog("Remove Tag Filter",
                    "Are you sure you want to remove this tag filter?", "Yes", "No"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(reorderableList);
                }
            };

            return list;
        }

        // UI Toolkit (used when inspector supports UITK)
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty listProp = property.FindPropertyRelative("_tagFilter");
            var root = new VisualElement();

            if (listProp == null)
            {
                root.Add(new HelpBox("Tag filter property not found.", HelpBoxMessageType.Error));
                return root;
            }

            var header = new UitkLabel("Tag Filters (Empty = Accept All)");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(header);

            var indices = new List<int>();
            var listView = new ListView
            {
                showAddRemoveFooter = true,
                reorderable = true,
                selectionType = SelectionType.None,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            listView.makeItem = () =>
            {
                var container = new VisualElement();
                var field = new PropertyField { label = string.Empty };
                container.Add(field);
                return container;
            };

            listView.bindItem = (element, index) =>
            {
                var field = element.Q<PropertyField>();
                var elementProp = listProp.GetArrayElementAtIndex(index);

                field.BindProperty(elementProp);

                field.RegisterCallback<GeometryChangedEvent>(_ =>
                {
                    UitkLabel label = field.Q<UitkLabel>();
                    if (label == null)
                    {
                        return;
                    }

                    float width = field.resolvedStyle.width;
                    if (width > 0f)
                    {
                        float labelWidth = width * LABEL_RATIO;
                        label.style.minWidth = labelWidth;
                        label.style.maxWidth = labelWidth;
                    }
                });
            };

            listView.itemsAdded += indicesAdded =>
            {
                foreach (int index in indicesAdded)
                {
                    listProp.InsertArrayElementAtIndex(index);
                    SerializedProperty element = listProp.GetArrayElementAtIndex(index);

                    SerializedProperty valueProp = element.FindPropertyRelative("value");
                    SerializedProperty backingVarRefProp = element.FindPropertyRelative("backingVarRef");

                    if (valueProp != null)
                    {
                        valueProp.stringValue = string.Empty;
                    }

                    if (backingVarRefProp != null)
                    {
                        SerializedProperty itemIdProp = backingVarRefProp.FindPropertyRelative("itemId");
                        if (itemIdProp != null)
                        {
                            itemIdProp.intValue = Variable.InvalidID;
                        }
                    }
                }

                listProp.serializedObject.ApplyModifiedProperties();
                RefreshItems();
            };

            listView.itemsRemoved += indicesRemoved =>
            {
                foreach (var index in indicesRemoved.OrderByDescending(i => i))
                {
                    listProp.DeleteArrayElementAtIndex(index);
                }

                listProp.serializedObject.ApplyModifiedProperties();
                RefreshItems();
            };

            listView.itemIndexChanged += (from, to) =>
            {
                listProp.MoveArrayElement(from, to);
                listProp.serializedObject.ApplyModifiedProperties();
                RefreshItems();
            };

            root.Add(listView);
            RefreshItems();

            void RefreshItems()
            {
                indices.Clear();
                for (int i = 0; i < listProp.arraySize; i++)
                {
                    indices.Add(i);
                }
                listView.itemsSource = indices;
                listView.Rebuild();
            }

            return root;
        }
    }
}