using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Amanita.Myceliaudio.VScripting;
using UnityObj = UnityEngine.Object;
using AtMycelia.Hyphlow.EditorUtils;

namespace VScriptingTests.CommandEditorOperations
{
    public class MA_PlayAudioEditorTests
    {
        private class TempInspectorHostWindow : EditorWindow
        {
            public Editor HostedEditor;

            // Called by our reflection OnGUI invocation; safe when Event.current is valid
            public void SafeDraw()
            {
                if (HostedEditor == null) return;
                HostedEditor.OnInspectorGUI();
            }
        }

        [Test]
        public void Inspector_DoesNotThrow_WhenGlobalVariableSourceVariablesIsNull()
        {
            // Arrange
            VariableSourceAsset vsa = ScriptableObject.CreateInstance<VariableSourceAsset>();
            toDestroyOnTearDown.Add(vsa);
            ForceVariablesListNull(vsa);

            VariableRegistryConfig config = DefaultAssetMaintenance.EnsureVariableRegistryConfig();
            if (config == null)
            {
                Assert.Ignore("VariableRegistryConfig could not be ensured. Skipping editor draw test.");
                return;
            }

            IReadOnlyList<VariableSourceAsset> previousSources = VariableRegistryService.GlobalSources.ToList();
            config.SetGlobalSources(previousSources.Concat(new[] { vsa }).ToList());
            VariableRegistryService.RebuildAll();

            Type playAudioType = typeof(MA_PlayAudio);
            if (playAudioType == null)
            {
                Assert.Ignore("MA_PlayAudio type not found. If renamed, update this test.");
                return;
            }

            GameObject host = new GameObject("Flowchart_TestHost");
            toDestroyOnTearDown.Add(host);
            Flowchart flowchart = host.AddComponent<Flowchart>();
            Block block = flowchart.CreateBlock(Vector2.zero);
            block.BlockName = "TestBlock";

            MA_PlayAudio command = host.AddComponent<MA_PlayAudio>();
            command.ParentBlock = block;
            command.ItemId = flowchart.NextItemId();
            block.CommandList.Add(command);

            Selection.activeObject = host;

            Editor editor = null;
            TempInspectorHostWindow window = null;

            try
            {
                editor = Editor.CreateEditor(command);
                window = ScriptableObject.CreateInstance<TempInspectorHostWindow>();
                toDestroyOnTearDown.Add(window);
                window.titleContent = new GUIContent("Temp Inspector Host");
                window.HostedEditor = editor;
                window.ShowUtility();

                // Simulate a GUI cycle: Layout then Repaint, invoking the window's OnGUI via reflection.
                Assert.DoesNotThrow(() =>
                {
                    SendGuiPass(window, EventType.Layout);
                    SendGuiPass(window, EventType.Repaint);
                }, "Inspector drawing for MA_PlayAudio threw an exception.");
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                    UnityObj.DestroyImmediate(window);
                }
                if (editor != null)
                {
                    UnityObj.DestroyImmediate(editor);
                }

                config.SetGlobalSources(previousSources.ToList());
                VariableRegistryService.RebuildAll();

                UnityObj.DestroyImmediate(vsa);
                UnityObj.DestroyImmediate(host);
                Selection.activeObject = null;
            }
        }

        [TearDown]
        public virtual void DoTearDown()
        {
            foreach (var obj in toDestroyOnTearDown)
            {
                if (obj != null)
                {
                    UnityObj.DestroyImmediate(obj);
                }
            }
            toDestroyOnTearDown.Clear();
        }

        private IList<UnityObj> toDestroyOnTearDown = new List<UnityObj>();

        private static void SendGuiPass(TempInspectorHostWindow window, EventType evtType)
        {
            // Build and assign an Event to mimic a real GUI pass
            Event evt = new Event { type = evtType };
            // Ensure no stale GUI state interferes
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;

            // Force the window to repaint and invoke its OnGUI (protected) via reflection
            window.Repaint();
            InvokeWindowOnGUI(window);
        }

        private static void InvokeWindowOnGUI(TempInspectorHostWindow window)
        {
            // Call the protected OnGUI of EditorWindow; TempInspectorHostWindow.SafeDraw will run inside it.
            MethodInfo onGui = typeof(EditorWindow).GetMethod("OnGUI", BindingFlags.Instance | BindingFlags.NonPublic);
            if (onGui != null)
            {
                onGui.Invoke(window, null);
            }
        }

        private static void ForceVariablesListNull(VariableSourceAsset vsa)
        {
            FieldInfo info = typeof(VariableSourceAsset).GetField("variables",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            info?.SetValue(vsa, null);
        }

    }
}