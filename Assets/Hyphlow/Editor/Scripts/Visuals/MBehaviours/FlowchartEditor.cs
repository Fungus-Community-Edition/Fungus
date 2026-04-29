using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    [CustomEditor(typeof(Flowchart))]
    public class FlowchartEditor : Editor
    {
        public static bool FlowchartDataStale { get; set; }

        protected virtual void OnEnable()
        {
            if (EraseOrphanedInstance())
            {
                return;
            }

            addTexture = HyphlowEditorSysAssets.AddSmall;
        }

        /// <summary>
        /// When modifying custom editor code you can occasionally end up with orphaned editor instances.
        /// When this happens, you'll get a null exception error every time the scene serializes / deserialized.
        /// Once this situation occurs, the only way to fix it is to restart the Unity editor.
        /// As a workaround, this function detects if this editor is an orphan and deletes it. 
        /// </summary>
        protected virtual bool EraseOrphanedInstance()
        {
            try
            {
#pragma warning disable 0219
                SerializedObject so = serializedObject;
            }
            catch (System.NullReferenceException)
            {
                DestroyImmediate(this);
                return true;
            }

            return false;
        }

        protected Texture2D addTexture;

        public override VisualElement CreateInspectorGUI()
        {
            var rootElement = new VisualElement();
            var uxml = Resources.Load<VisualTreeAsset>(_pathToUxml);
            var inspectorRoot = uxml.CloneTree();
            Button flowchartWindowButton = inspectorRoot.Q<Button>(_openFlowchartWindowButtonName);
            flowchartWindowButton.RegisterCallback<ClickEvent>(OpenFlowchartWindow);
            rootElement.Add(inspectorRoot);

            return rootElement;
        }

        private static readonly string _pathToUxml = "UIToolkitTemplates/FlowchartInspector";
        private static readonly string _openFlowchartWindowButtonName = "OpenFlowchartWindow";

        protected virtual void OpenFlowchartWindow(ClickEvent clickEvent)
        {
            FlowchartWindow.BringUp();
        }
    }
}
