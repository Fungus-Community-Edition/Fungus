using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.EditorUtils
{
    public class CommandListAdaptor
    {
        /// <summary>
        /// If true, scrolls to the currently selected command in the inspector when the editor is redrawn. A
        /// Automatically resets to false.
        /// </summary>
        public static bool ScrollToCommandOnDraw = false;

        public void DrawCommandList()
        {
            
            if (summaryStyle == null)
            {
                summaryStyle = new GUIStyle();
                summaryStyle.fontSize = 10;
                summaryStyle.padding.top += 5;
                summaryStyle.richText = true;
                summaryStyle.wordWrap = false;
                summaryStyle.clipping = TextClipping.Clip;
            }

            if (commandLabelStyle == null)
            {
                commandLabelStyle = new GUIStyle(GUI.skin.label);
                commandLabelStyle.normal.background = AmanitaEditorResources.CommandBackground;
                commandLabelStyle.normal.textColor = Color.black;
                int borderSize = 5;
                commandLabelStyle.border.top = borderSize;
                commandLabelStyle.border.bottom = borderSize;
                commandLabelStyle.border.left = borderSize;
                commandLabelStyle.border.right = borderSize;
                commandLabelStyle.alignment = TextAnchor.MiddleLeft;
                commandLabelStyle.richText = true;
                commandLabelStyle.fontSize = 11;
                commandLabelStyle.padding.top -= 1;
                commandLabelStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (block.CommandList.Count == 0)
            {
                if (!AmanitaEditorPreferences.suppressHelpBoxes)
                {
                    EditorGUILayout.HelpBox("Press the + button below to add a command to the list.", MessageType.Info); 
                }
            }
            else
            {
                EditorGUI.indentLevel++;
                list.DoLayoutList();
                EditorGUI.indentLevel--;
            }
        }

        protected SerializedProperty _arrayProperty;

        protected ReorderableList list;

        protected Block block;
        protected GUIStyle summaryStyle, commandLabelStyle;

        public float fixedItemHeight;

        public SerializedProperty this[int index]
        {
            get { return _arrayProperty.GetArrayElementAtIndex(index); }
        }

        public SerializedProperty ArrayProperty
        {
            get { return _arrayProperty; }
        }

        public CommandListAdaptor(Block _block, SerializedProperty arrayProperty)
        {
            Validate();
            void Validate()
            {
                if (arrayProperty == null)
                    throw new ArgumentNullException("Array property was null.");
                if (!arrayProperty.isArray)
                    throw new InvalidOperationException("Specified serialized propery is not an array.");
            }

            this._arrayProperty = arrayProperty;
            this.block = _block;

            list = new ReorderableList(arrayProperty.serializedObject, arrayProperty,
                draggable: true, displayHeader: true,
                displayAddButton: false, displayRemoveButton: false);

            list.drawHeaderCallback = DrawHeader;
            list.drawElementCallback = DrawItem;
            list.onSelectCallback = SelectChanged;

            list.elementHeight = EditorGUIUtility.singleLineHeight + 6;

            //list = new ReorderableList(arrayProperty.serializedObject, arrayProperty, true, true, false, false);
            //list.drawHeaderCallback = DrawHeader;
            //list.drawElementCallback = DrawItem;
            ////list.elementHeightCallback = GetElementHeight;
            //list.onSelectCallback = SelectChanged;
        }

        private void SelectChanged(ReorderableList list)
        {
            Command command = this[list.index].objectReferenceValue as Command;
            var flowchart = (Flowchart)command.GetFlowchart();
            BlockEditor.actionList.Add(delegate
            {
                flowchart.ClearSelectedCommands();
                flowchart.AddSelectedCommand(command);
            });
        }

        private void DrawHeader(Rect rect)
        {
            if (rect.width < 0) return;
            EditorGUI.LabelField(rect, new GUIContent("Commands"));
        }

        public void DrawItem(Rect position, int index, bool selected, bool focused)
        {
            // 1) Grab our data
            var prop = _arrayProperty.GetArrayElementAtIndex(index);
            var command = prop.objectReferenceValue as Command;
            var flowchart = (Flowchart)command?.GetFlowchart();
            if (command == null || flowchart == null) return;
            // Build the display name once
            string commandName = BuildCommandNameLabel(flowchart, command);

            // 2) Compute all the rects we need
            IList<Rect> indentRects;
            Rect labelRect, summaryRect, iconRect, clickRect;
            ComputeNeededRects();
            void ComputeNeededRects()
            {
                indentRects = CalculateIndentRects(position, command.IndentLevel);
                labelRect = CalculateLabelRect(position, command.IndentLevel);
                summaryRect = CalculateSummaryRect(labelRect, commandName);

                iconRect = CalculateIconRect(labelRect, command);
                clickRect = position;  // covers entire row
            }
            

            // 3) Paint indentation guides
            foreach (var r in indentRects)
                GUI.Box(r, "", commandLabelStyle);

            // 4) Paint background (tint or selection highlight)
            Color bgColor = DetermineBackgroundColor(flowchart, command);
            GUI.backgroundColor = bgColor;

            // 5) Paint background…
            GUI.Label(labelRect, commandName, commandLabelStyle);

            // 6) Paint the summary text
            GUI.Label(summaryRect, command.GetSummary() ?? "", summaryStyle);

            // 7) Paint the small executing-icon if needed
            DrawExecutingIcon(iconRect, command);

            // 8) Handle clicks (we’ll swap in a GUI.Button next)
            HandleClick(clickRect, flowchart, command);

            // 9) Restore UI state
            GUI.backgroundColor = Color.white;
            GUI.color = Color.white;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        List<Rect> CalculateIndentRects(Rect row, int level)
        {
            var list = new List<Rect>();
            float indentSize = 20;
            for (int i = 0; i < level; i++)
            {
                var r = row;
                r.x += i * indentSize;
                r.width = indentSize + 1;
                r.y -= 2;
                r.height += 5;
                list.Add(r);
            }
            return list;
        }

        Rect CalculateLabelRect(Rect row, int level)
        {
            float indentSize = 20;
            var r = row;
            r.x += level * indentSize;
            r.y -= 2;
            r.width -= level * indentSize;
            r.height += 5;
            return r;
        }

        Rect CalculateSummaryRect(Rect labelRect, string commandName)
        {
            var r = labelRect;
            r.x += summaryRectXOffset;
            return r;
        }

        protected static readonly float summaryRectXOffset = 100;

        Rect CalculateIconRect(Rect labelRect, Command command)
        {
            if (command.ExecutingIconTimer > Time.realtimeSinceStartup)
            {
                var r = labelRect;
                r.x += r.width - 20;
                r.width = 20;
                r.height = 20;
                return r;
            }
            return Rect.zero;
        }

        Color DetermineBackgroundColor(Flowchart f, Command cmd)
        {
            if (f.SelectedCommands.Contains(cmd))
                return Color.green;
            if (!cmd.enabled)
                return Color.grey;
            return cmd.GetButtonColor();
        }

        string BuildCommandNameLabel(Flowchart f, Command cmd)
        {
            string baseName = cmd.GetType()
                             .GetCustomAttribute<CommandInfoAttribute>()
                             ?.CommandName ?? cmd.name;
            return f.ShowLineNumbers
                ? $"{cmd.CommandIndex}: {baseName}"
                : baseName;
        }

        void DrawExecutingIcon(Rect iconRect, Command cmd)
        {
            if (iconRect == Rect.zero) return;
            float alpha = (cmd.ExecutingIconTimer - Time.realtimeSinceStartup)
                            / AmanitaConstants.ExecutingIconFadeTime;
            alpha = Mathf.Clamp01(alpha);
            var prevColor = GUI.color;
            GUI.color = new Color(1, 1, 1, alpha);
            GUI.Label(iconRect, AmanitaEditorResources.PlaySmall);
            GUI.color = prevColor;
        }

        void HandleClick(Rect clickRect, Flowchart flowchart, Command command)
        {
            // catches clicks on Layout, MouseDown, Repaint, etc.
            if (GUI.Button(clickRect, GUIContent.none, GUIStyle.none))
            {
                // 1) Handle modifier keys
                bool shiftHeld = Event.current.shift;
                bool ctrlHeld = EditorGUI.actionKey;

                if (!shiftHeld && !ctrlHeld)
                {
                    // single‐click: clear & select just this command
                    flowchart.ClearSelectedCommands();
                }

                if (ctrlHeld)
                {
                    // ctrl‐click: toggle selection
                    if (flowchart.Contains(command))
                        flowchart.RemoveFromSelection(command);
                    else
                        flowchart.AddSelectedCommand(command);
                }
                else
                {
                    // either single‐click or shift‐click (shift handled below)
                    flowchart.AddSelectedCommand(command);
                }

                if (shiftHeld && flowchart.SelectedBlock != null)
                {
                    // range‐select from first to this
                    var cmds = flowchart.SelectedBlock.CommandList;
                    int clickedIndex = command.CommandIndex;
                    int min = Mathf.Min(cmds.IndexOf(flowchart.SelectedCommands.First()), clickedIndex);
                    int max = Mathf.Max(cmds.IndexOf(flowchart.SelectedCommands.First()), clickedIndex);
                    flowchart.ClearSelectedCommands();
                    for (int i = min; i <= max; i++)
                        flowchart.AddSelectedCommand(cmds[i]);
                }

                // 2) Scroll it into view next draw (optional)
                ScrollToCommandOnDraw = true;

                //// 3) Force the FlowchartWindow to repaint
                //var fcWin = EditorWindow.GetWindow<FlowchartWindow>();
                //fcWin?.Repaint();

                //// 4) Force all Inspector windows to repaint so you see the green highlight
                //var inspectorType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
                //var all = Resources.FindObjectsOfTypeAll<EditorWindow>();
                //foreach (var w in all)
                //    if (inspectorType != null && w.GetType() == inspectorType)
                //        w.Repaint();

                Event.current.Use();
            }
        }



    }
}
