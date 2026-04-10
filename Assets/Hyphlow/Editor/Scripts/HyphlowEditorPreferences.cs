using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Shows Amanita section in the Edit->Preferences in unity allows you to configure Amanita behaviour
    /// 
    /// ref https://docs.unity3d.com/ScriptReference/PreferenceItem.html
    /// </summary>
    [InitializeOnLoad]
    public static class HyphlowEditorPreferences
    {
        // Have we loaded the prefs yet
        private static bool prefsLoaded = false;
        private const string HIDE_MUSH_KEY = "hideMushroomInHierarchy";
        private const string USE_LEGACY_MENUS = "useLegacyMenus";
        private const string USE_GRID_SNAP = "useGridSnap";
        private const string COMMAND_LIST_ITEM_TINT = "commandListTint";
        private const string FLOWCHART_WINDIOW_BLOCK_TINT = "flowchartWindowBlockTint";
        private const string SUPPRESS_HELP_BOXES = "suppressHelpBoxes";
        private const string NAV_CMD_WITH_ARROWS = "navigateCmdListWithArrows";

        public static bool hideMushroomInHierarchy;
        public static bool useLegacyMenus;
        public static bool useGridSnap;
        public static Color commandListTint = Color.white;
        public static Color flowchartBlockTint = Color.white;
        public static bool suppressHelpBoxes = false;
        public static bool navigateCmdListWithArrows = false;

        static HyphlowEditorPreferences()
        {
            LoadOnScriptLoad();
        }

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider CreateAmanitaSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("Project/Amanita", SettingsScope.Project)
            {
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) => PreferencesGUI()

                // // Populate the search keywords to enable smart search filtering and label highlighting:
                // keywords = new HashSet<string>(new[] { "Number", "Some String" })
            };

            return provider;
        }

#else

        [PreferenceItem("Amanita")]
#endif
        private static void PreferencesGUI()
        {
            // Load the preferences
            if (!prefsLoaded)
            {
                LoadOnScriptLoad();
            }

            // Preferences GUI
            hideMushroomInHierarchy = EditorGUILayout.Toggle("Hide Mushroom Flowchart Icon", hideMushroomInHierarchy);
            useLegacyMenus = EditorGUILayout.Toggle(new GUIContent("Legacy Menus", "Force Legacy menus for Event, Add Variable and Add Command menus"), useLegacyMenus);
            useGridSnap = EditorGUILayout.Toggle(new GUIContent("Grid Snap", "Align and Snap block positions and widths in the flowchart window to the grid"), useGridSnap);
            flowchartBlockTint = EditorGUILayout.ColorField(new GUIContent("Flowchart Window Block Tint", "Custom tint used on the Block icons in the Flowchart Window. Default is white."), flowchartBlockTint);
            commandListTint = EditorGUILayout.ColorField(new GUIContent("Command List Tint", "Custom tint used on the Command List in the Block Inspector. Default is white."), commandListTint);
            suppressHelpBoxes = EditorGUILayout.Toggle(new GUIContent("Hide Help Boxes", "Hides the Default Help boxes shown in in Block inspector for EventHandlers and Commands."), suppressHelpBoxes);
            navigateCmdListWithArrows = EditorGUILayout.Toggle(new GUIContent("Use Arrows In CMD List", "Allows the use of Arrows UP and Down to move between Commands in the Command list, in the block inspector"), navigateCmdListWithArrows);

            EditorGUILayout.Space();
            //ideally if any are null, but typically it is all or nothing that have broken links due to version changes or moving files external to Unity
            if (HyphlowEditorSysAssets.Add == null)
            {
                EditorGUILayout.HelpBox("HyphlowEditorSysAssets need to be regenerated!", MessageType.Error);
            }

            if (GUILayout.Button(new GUIContent("Select Amanita Editor Resources SO", "If Amanita icons are not showing correctly you may need to reassign the references in the HyphlowEditorSysAssets. Button below will locate it.")))
            {
                var ids = AssetDatabase.FindAssets("t:HyphlowEditorSysAssets");
                if (ids.Length > 0)
                {
                    var p = AssetDatabase.GUIDToAssetPath(ids[0]);
                    var asset = AssetDatabase.LoadAssetAtPath<HyphlowEditorSysAssets>(p);
                    Selection.activeObject = asset;
                }
                else
                {
                    Debug.LogError("No HyphlowEditorSysAssets found!");
                }
            }

            if (GUILayout.Button("Open Changelog (version info)"))
            {
                //From project path down, look for our Amanita\Docs\ChangeLog.txt
                var projectPath = System.IO.Directory.GetParent(Application.dataPath);
                var fileMacthes = System.IO.Directory.GetFiles(projectPath.FullName, "CHANGELOG.txt", System.IO.SearchOption.AllDirectories);

                fileMacthes = fileMacthes.Where((x) =>
                {
                    var fileFolder = System.IO.Directory.GetParent(x);
                    return fileFolder.Name == "Docs" && fileFolder.Parent.Name == "Amanita";
                }).ToArray();

                if (fileMacthes == null || fileMacthes.Length == 0)
                {
                    Debug.LogWarning("Cannot locate Amanita\\Docs\\CHANGELONG.txt");
                }
                else
                {
                    Application.OpenURL(fileMacthes[0]);
                }
            }

            // Save the preferences
            if (GUI.changed)
            {
                EditorPrefs.SetBool(HIDE_MUSH_KEY, hideMushroomInHierarchy);
                EditorPrefs.SetBool(USE_LEGACY_MENUS, useLegacyMenus);
                EditorPrefs.SetBool(USE_GRID_SNAP, useGridSnap);
                var colAsString = "#" + ColorUtility.ToHtmlStringRGBA(commandListTint);
                EditorPrefs.SetString(COMMAND_LIST_ITEM_TINT, colAsString); 
                colAsString = "#" + ColorUtility.ToHtmlStringRGBA(flowchartBlockTint);
                EditorPrefs.SetString(FLOWCHART_WINDIOW_BLOCK_TINT, colAsString);
                EditorPrefs.SetBool(SUPPRESS_HELP_BOXES, suppressHelpBoxes); 
                EditorPrefs.SetBool(NAV_CMD_WITH_ARROWS, navigateCmdListWithArrows); 
            }
        }

        public static void LoadOnScriptLoad()
        {
            hideMushroomInHierarchy = EditorPrefs.GetBool(HIDE_MUSH_KEY, false);
            useLegacyMenus = EditorPrefs.GetBool(USE_LEGACY_MENUS, false);
            useGridSnap = EditorPrefs.GetBool(USE_GRID_SNAP, false);

            if(ColorUtility.TryParseHtmlString(EditorPrefs.GetString(COMMAND_LIST_ITEM_TINT), out var col))
            {
                commandListTint = col;
            }
            if (ColorUtility.TryParseHtmlString(EditorPrefs.GetString(FLOWCHART_WINDIOW_BLOCK_TINT), out col))
            {
                flowchartBlockTint = col;
            }
            suppressHelpBoxes = EditorPrefs.GetBool(SUPPRESS_HELP_BOXES, false);
            navigateCmdListWithArrows = EditorPrefs.GetBool(NAV_CMD_WITH_ARROWS, false);
            prefsLoaded = true;
        }
    }
    
}
