using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Temp hidden object which lets us use the entire inspector window to inspect the block command list.
    /// </summary>
    public class BlockInspector : ScriptableObject 
    {
        [FormerlySerializedAs("sequence")]
        public Block block;
    }

    /// <summary>
    /// Custom editor for the temp hidden object.
    /// </summary>
    [CustomEditor (typeof(BlockInspector), true)]
    public class BlockInspectorEditor : Editor
    {
        // Cache the block and command editors so we only create and destroy them
        // when a different block / command is selected.
        protected BlockEditor activeBlockEditor;
        protected CommandEditor activeCommandEditor;
        protected Command activeCommand; // Command currently being inspected

        // Cached command editors to avoid creating / destroying editors more than necessary
        // This list is static so persists between {something}
        // CG-Tespy's note: At some point, we might want to make it so that we don't need more
        // than one CommandEditor at a time. So we can reuse the same one for different 
        // Commands to display.
        protected static List<CommandEditor> cachedCommandEditors = new List<CommandEditor>();

        protected void OnEnable()
        {
            ClearEditors();
            var ammieManager = AmanitaManager.S;
            Flowchart currentFc = FlowchartWindow.GetFlowchart();
            if (ammieManager != null && currentFc != null)
            {
                Debug.Log($"Rebuilding Variable Registry for Block Inspector and Flowchart {currentFc.name}");
                var varRegistry = ammieManager.VariableRegistry;
                varRegistry.Rebuild(currentFc);
                // ^For cases where the fc the FlowchartWindow is handling is not selected
            }
        }

        protected void ClearEditors()
        {
            foreach (CommandEditor commandEditor in cachedCommandEditors)
            {
                DestroyImmediate(commandEditor);
            }

            cachedCommandEditors.Clear();
            activeCommandEditor = null;
        }

        protected void OnDisable()
        {
            ClearEditors();
        }

        protected void OnDestroy()
        {
            ClearEditors();
        }

        public override void OnInspectorGUI() 
        {
            BlockInspector blockInspector = target as BlockInspector;
            Block block = blockInspector.block;
            bool weHaveAnythingToDraw = block != null && block.IsSelected;
            if (!weHaveAnythingToDraw)
            {
                return;
            }

            var flowchart = block.GetFlowchart();

            if (flowchart.SelectedBlockCount > 1)
            {
                GUILayout.Label("Multiple blocks selected");
                return;
            }

            EnsureBlockEditorTargetsOurBlock();
            void EnsureBlockEditorTargetsOurBlock()
            {
                if (activeBlockEditor == null ||
                    !block.Equals(activeBlockEditor.target))
                {
                    DestroyImmediate(activeBlockEditor);
                    activeBlockEditor = Editor.CreateEditor(block, typeof(BlockEditor)) as BlockEditor;
                }
            }

            UpdateWindowHeight();

            float width = EditorGUIUtility.currentViewWidth;

            DrawBaseBlockGUIInScrollView();
            void DrawBaseBlockGUIInScrollView()
            {
                blockScrollPos = GUILayout.BeginScrollView(blockScrollPos, GUILayout.Height(flowchart.BlockViewHeight));
                activeBlockEditor.DrawBlockName(flowchart);
                activeBlockEditor.DrawBlockGUI(flowchart);
                GUILayout.EndScrollView();
            }

            Command commandToInspect = null;
            if (flowchart.SelectedCommandCount == 1)
            {
                commandToInspect = flowchart.SelectedCommands[0];
            }

            if (Application.isPlaying &&
                commandToInspect != null &&
                !commandToInspect.ParentBlock.Equals(block))
            {
                Repaint();
                return;
            }

            // Only change the activeCommand at the start of the GUI call sequence
            if (Event.current.type == EventType.Layout)
            {
                activeCommand = commandToInspect;
            }

            DrawCommandUI(flowchart, commandToInspect);
        }

        protected Vector2 blockScrollPos;
        
        /// <summary>
        /// In Unity 5.4, Screen.height returns the pixel height instead of the point height
        /// of the inspector window. We can use EditorGUIUtility.currentViewWidth to get the window width
        /// but we have to use this horrible hack to find the window height.
        /// For one frame the windowheight will be 0, but it doesn't seem to be noticeable.
        /// </summary>
        protected void UpdateWindowHeight()
        {
            windowHeight = Screen.height * EditorGUIUtility.pixelsPerPoint;
        }

        protected float windowHeight = 0f;

        public void DrawCommandUI(Flowchart flowchart, Command inspectCommand)
        {
            ResizeScrollView(flowchart);

            EditorGUILayout.Space();

            activeBlockEditor.DrawButtonToolbar();

            commandScrollPos = GUILayout.BeginScrollView(commandScrollPos);

            if (inspectCommand != null)
            {
                if (activeCommandEditor == null ||
                    !inspectCommand.Equals(activeCommandEditor.target))
                {
                    // See if we have a cached version of the command editor already
                    var editors = cachedCommandEditors
                        .Where(e => e != null && e.target.Equals(inspectCommand));

                    if (editors.Any())
                    {
                        activeCommandEditor = editors.First();
                    }
                    else
                    {
                        activeCommandEditor = Editor.CreateEditor(inspectCommand) as CommandEditor;
                        cachedCommandEditors.Add(activeCommandEditor);
                    }
                }

                // 🔹 SAFETY WRAP
                SafeIMGUI.Draw(() => activeCommandEditor.DrawCommandInspectorGUI(), inspectCommand.name);
            }

            GUILayout.EndScrollView();

            DrawResizeBar();
            void DrawResizeBar()
            {
                Vector2 resizeRectPos = new Vector2(0, flowchart.BlockViewHeight);
                Vector2 resizeRectSize = new Vector2(EditorGUIUtility.currentViewWidth, 4f);
                Rect resizeRect = new Rect(resizeRectPos, resizeRectSize);

                GUI.color = new Color(0.64f, 0.64f, 0.64f);
                GUI.DrawTexture(resizeRect, EditorGUIUtility.whiteTexture);
                resizeRect.height = 1;

                GUI.color = new Color32(132, 132, 132, 255);
                GUI.DrawTexture(resizeRect, EditorGUIUtility.whiteTexture);
                resizeRect.y += 3;

                GUI.DrawTexture(resizeRect, EditorGUIUtility.whiteTexture);
                GUI.color = Color.white;
            }

            Repaint();
        }

        protected Vector2 commandScrollPos;

        protected void ResizeScrollView(Flowchart flowchart)
        {
            Vector2 cursorChangePos = new Vector2(0, flowchart.BlockViewHeight + 1);
            Vector2 cursorChangeSize = new Vector2(EditorGUIUtility.currentViewWidth, 4f);
            Rect cursorChangeRect = new Rect(cursorChangePos, cursorChangeSize);

            EditorGUIUtility.AddCursorRect(cursorChangeRect, MouseCursor.ResizeVertical);
            
            if (cursorChangeRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    resize = true;
                }
            }

            if (resize && Event.current.type == EventType.Repaint)
            {
                Undo.RecordObject(flowchart, "Resize view");
                flowchart.BlockViewHeight = Event.current.mousePosition.y;
            }
            
            ClampBlockViewHeight(flowchart);
            
            // Stop resizing if mouse is outside inspector window.
            // This isn't standard Unity UI behavior but it is robust and safe.
            if (resize && Event.current.type == EventType.MouseDrag)
            {
                Rect windowRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, windowHeight);
                bool mouseOutsideInspectorWindow = !windowRect.Contains(Event.current.mousePosition);
                if (mouseOutsideInspectorWindow)
                {
                    resize = false;
                }
            }

            bool releasedMouse = Event.current.type == EventType.MouseUp;
            if (releasedMouse)
            {
                resize = false;
            }
        }
        
        protected bool resize = false;

        protected virtual void ClampBlockViewHeight(Flowchart flowchart)
        {
            // Screen.height seems to temporarily reset to 480 for a single frame whenever a command like 
            // Copy, Paste, etc. happens. Only clamp the block view height when one of
            // these operations is NOT occuring.

            if (Event.current.commandName != "")
            {
                clamp = false;
            }
            
            if (clamp)
            {
                // Make sure block view is always clamped to visible area
                float height = flowchart.BlockViewHeight;
                height = Mathf.Max(200, height);
                height = Mathf.Min(windowHeight - 200,height);
                flowchart.BlockViewHeight = height;
            }
            
            if (Event.current.type == EventType.Repaint)
            {
                clamp = true;
            }
        }

        protected bool clamp = false;

    }
}
