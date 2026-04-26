using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Temp hidden object which lets us use the entire inspector window to inspect the block command list.
    /// </summary>
    public class BlockInspector : ScriptableObject 
    {
        [FormerlySerializedAs("sequence")]
        [FormerlySerializedAs("block")]
        public Block _block;
    }

    /// <summary>
    /// Custom editor for the temp hidden object.
    /// </summary>
    [CustomEditor (typeof(BlockInspector), true)]
    public class BlockInspectorEditor : Editor
    {
        // Cache the block and command editors so we only create and destroy them
        // when a different block / command is selected.
        protected BlockEditor _activeBlockEditor;
        protected CommandEditor _activeCommandEditor;
        protected Command _activeCommand; // Command currently being inspected

        // Cached command editors to avoid creating / destroying editors more than necessary
        // This list is static so persists between {something}
        // CG-Tespy's note: At some point, we might want to make it so that we don't need more
        // than one CommandEditor at a time. So we can reuse the same one for different 
        // Commands to display.
        protected static List<CommandEditor> _cachedCommandEditors = new List<CommandEditor>();

        protected void OnEnable()
        {
            ClearEditors();
            Flowchart currentFc = EditorSelectionTracker.ActiveFlowchart;
            if (currentFc == null)
            {
                currentFc = EditorSelectionTracker.LastActiveFlowchart;
            }
            if (currentFc != null)
            {
                //Debug.Log($"Rebuilding Variable Registry for Block Inspector and Flowchart {currentFc.name}");
                VariableRegistryService.RebuildAll(currentFc);
                // ^For cases where the fc the FlowchartWindow is handling is not selected
            }
        }

        protected void ClearEditors()
        {
            foreach (CommandEditor commandEditor in _cachedCommandEditors)
            {
                DestroyImmediate(commandEditor);
            }

            _cachedCommandEditors.Clear();
            _activeCommandEditor = null;
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
            Block block = blockInspector._block;
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
                if (_activeBlockEditor == null ||
                    !block.Equals(_activeBlockEditor.target))
                {
                    DestroyImmediate(_activeBlockEditor);
                    _activeBlockEditor = Editor.CreateEditor(block, typeof(BlockEditor)) as BlockEditor;
                }
            }

            DrawBlockEnabledToggle();
            void DrawBlockEnabledToggle()
            {
                EditorGUI.BeginChangeCheck();
                bool enabled = EditorGUILayout.Toggle("Enabled", block.enabled);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(block, "Toggle Block Enabled");
                    block.enabled = enabled;
                    EditorUtility.SetDirty(block);
                }
            }

            UpdateWindowHeight();

            float width = EditorGUIUtility.currentViewWidth;

            DrawBaseBlockGUIInScrollView();
            void DrawBaseBlockGUIInScrollView()
            {
                var uiModel = flowchart.UIModel;
                _blockScrollPos = GUILayout.BeginScrollView(_blockScrollPos, GUILayout.Height(uiModel.BlockViewHeight));
                _activeBlockEditor.DrawBlockName(flowchart);
                _activeBlockEditor.DrawBlockGUI(flowchart);
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
                _activeCommand = commandToInspect;
            }

            DrawCommandUI(flowchart, commandToInspect);
        }

        protected Vector2 _blockScrollPos;
        
        /// <summary>
        /// In Unity 5.4, Screen.height returns the pixel height instead of the point height
        /// of the inspector window. We can use EditorGUIUtility.currentViewWidth to get the window width
        /// but we have to use this horrible hack to find the window height.
        /// For one frame the windowheight will be 0, but it doesn't seem to be noticeable.
        /// </summary>
        protected void UpdateWindowHeight()
        {
            _windowHeight = Screen.height * EditorGUIUtility.pixelsPerPoint;
        }

        protected float _windowHeight = 0f;

        public void DrawCommandUI(Flowchart flowchart, Command inspectCommand)
        {
            ResizeScrollView(flowchart);

            EditorGUILayout.Space();

            _activeBlockEditor.DrawButtonToolbar();

            _commandScrollPos = GUILayout.BeginScrollView(_commandScrollPos);

            if (inspectCommand != null)
            {
                if (_activeCommandEditor == null ||
                    !inspectCommand.Equals(_activeCommandEditor.target))
                {
                    // See if we have a cached version of the command editor already
                    var editors = _cachedCommandEditors
                        .Where(e => e != null && e.target.Equals(inspectCommand));

                    if (editors.Any())
                    {
                        _activeCommandEditor = editors.First();
                    }
                    else
                    {
                        _activeCommandEditor = Editor.CreateEditor(inspectCommand) as CommandEditor;
                        _cachedCommandEditors.Add(_activeCommandEditor);
                    }
                }

                // 🔹 SAFETY WRAP
                SafeIMGUI.Draw(() => _activeCommandEditor.DrawCommandInspectorGUI(), inspectCommand.name);
            }

            GUILayout.EndScrollView();

            DrawResizeBar();
            void DrawResizeBar()
            {
                var uiModel = flowchart.UIModel;
                Vector2 resizeRectPos = new Vector2(0, uiModel.BlockViewHeight);
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

            //Repaint();
        }

        protected Vector2 _commandScrollPos;

        protected void ResizeScrollView(Flowchart flowchart)
        {
            var uiModel = flowchart.UIModel;
            Vector2 cursorChangePos = new Vector2(0, uiModel.BlockViewHeight + 1);
            Vector2 cursorChangeSize = new Vector2(EditorGUIUtility.currentViewWidth, 4f);
            Rect cursorChangeRect = new Rect(cursorChangePos, cursorChangeSize);

            EditorGUIUtility.AddCursorRect(cursorChangeRect, MouseCursor.ResizeVertical);
            
            if (cursorChangeRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    _resize = true;
                }
            }

            if (_resize && Event.current.type == EventType.Repaint)
            {
                Undo.RecordObject(flowchart, "Resize view");
                uiModel.BlockViewHeight = Event.current.mousePosition.y;
            }
            
            ClampBlockViewHeight(flowchart);
            
            // Stop resizing if mouse is outside inspector window.
            // This isn't standard Unity UI behavior but it is robust and safe.
            if (_resize && Event.current.type == EventType.MouseDrag)
            {
                Rect windowRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, _windowHeight);
                bool mouseOutsideInspectorWindow = !windowRect.Contains(Event.current.mousePosition);
                if (mouseOutsideInspectorWindow)
                {
                    _resize = false;
                }
            }

            bool releasedMouse = Event.current.type == EventType.MouseUp;
            if (releasedMouse)
            {
                _resize = false;
            }
        }
        
        protected bool _resize = false;

        protected virtual void ClampBlockViewHeight(Flowchart flowchart)
        {
            // Screen.height seems to temporarily reset to 480 for a single frame whenever a command like 
            // Copy, Paste, etc. happens. Only clamp the block view height when one of
            // these operations is NOT occuring.

            if (Event.current.commandName != "")
            {
                _clamp = false;
            }
            
            if (_clamp)
            {
                // Make sure block view is always clamped to visible area
                var uiModel = flowchart.UIModel;
                float height = uiModel.BlockViewHeight;
                height = Mathf.Max(200, height);
                height = Mathf.Min(_windowHeight - 200,height);
                uiModel.BlockViewHeight = height;
            }
            
            if (Event.current.type == EventType.Repaint)
            {
                _clamp = true;
            }
        }

        protected bool _clamp = false;

    }
}
