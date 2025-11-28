using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace Amanita.EditorUtils
{
    public class AssemblyMigrationWindow : EditorWindow
    {
        private string CurrentAssemblyName => currentAsmDefNameLabel?.value;
        private string NewAssemblyName => newAsmDefNameLabel?.value;

        [MenuItem("Window/Atelier Mycelia/Amanita/Assembly Migration")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<AssemblyMigrationWindow>();
            wnd.titleContent = new GUIContent("Assembly Migration");
            wnd.minSize = wnd.maxSize = windowSize;
        }

        private readonly static Vector2 windowSize = new Vector2(600, 600);

        public void CreateGUI()
        {
            IStyle rootStyle = Root.style;
            rootStyle.paddingLeft = rootStyle.paddingRight = 20;
            rootStyle.paddingTop = rootStyle.paddingBottom = 10;

            PrepFields();
            AddElementsToRoot();
        }

        VisualElement Root => rootVisualElement;

        ScrollView summaryScroll;
        UitkLabel summaryLabel;

        private bool IsDryRun
        {
            get
            {
                if (dryRunToggle != null)
                {
                    return dryRunToggle.value;
                }

                return false;
            }
        }

        void PrepFields()
        {
            #region Title Label
            titleLabel = new UitkLabel("Assembly Rename Migration");
            IStyle titleStyle = titleLabel.style;
            titleStyle.unityFontStyleAndWeight = FontStyle.Bold;
            titleStyle.fontSize = 32;
            titleStyle.marginBottom = gapBetweenStuff;
            titleStyle.alignSelf = Align.Center;
            #endregion

            #region Assembly Fields

            PrepAsmDefToChangeField();
            void PrepAsmDefToChangeField()
            {
                asmDefToChangePicker = new ObjectField("Asmdef to Change");
                IStyle defToChangeStyle = asmDefToChangePicker.style;
                defToChangeStyle.marginBottom = gapBetweenStuff;
                defToChangeStyle.fontSize = asmFieldFontSize;
                IStyle defToChangeLabelStyle = asmDefToChangePicker.labelElement.style;
                defToChangeLabelStyle.width = Length.Percent(labelWidthPercent);
                asmDefToChangePicker.objectType = typeof(AssemblyDefinitionAsset);
                asmDefToChangePicker.RegisterValueChangedCallback(OnAsmDefPickerValueChanged);
            }

            PrepCurrentAsmDefNameField();
            void PrepCurrentAsmDefNameField()
            {
                currentAsmDefNameLabel = new TextField("Current Asmdef Name");
                IStyle currentAsmStyle = currentAsmDefNameLabel.style;
                currentAsmStyle.fontSize = asmFieldFontSize;
                currentAsmStyle.marginBottom = gapBetweenStuff;
                IStyle currentAsmLabelStyle = currentAsmDefNameLabel.labelElement.style;
                currentAsmLabelStyle.width = Length.Percent(labelWidthPercent);
                currentAsmDefNameLabel.isReadOnly = true;
            }

            PrepNewAsmDefNameField();
            void PrepNewAsmDefNameField()
            {
                newAsmDefNameLabel = new TextField("New Asmdef Name");
                IStyle newAsmStyle = newAsmDefNameLabel.style;
                newAsmStyle.fontSize = asmFieldFontSize;
                newAsmStyle.marginBottom = gapBetweenStuff;
                IStyle newAsmLabelStyle = newAsmDefNameLabel.labelElement.style;
                newAsmLabelStyle.width = Length.Percent(labelWidthPercent);
            }

            #endregion

            #region Dry Run Toggle
            dryRunToggle = new Toggle("Dry Run Mode");
            dryRunToggle.value = true;
            IStyle dryRunStyle = dryRunToggle.style;
            dryRunStyle.fontSize = asmFieldFontSize;
            dryRunStyle.marginBottom = gapBetweenStuff;
            IStyle dryRunToggleLabelStyle = dryRunToggle.labelElement.style;
            dryRunToggleLabelStyle.width = Length.Percent(labelWidthPercent);
            #endregion

            #region Run Button
            runButton = new Button(() => RunMigration())
            {
                text = "Run Migration"
            };

            IStyle runButtonStyle = runButton.style;
            runButtonStyle.marginBottom = gapBetweenStuff;
            runButtonStyle.fontSize = asmFieldFontSize;
            runButtonStyle.height = runButtonHeight;
            #endregion

            UpdateCurrentAsmDefNameLabel();

            #region Summary Report

            summaryScroll = new ScrollView();
            IStyle scrollStyle = summaryScroll.style;
            scrollStyle.fontSize = asmFieldFontSize;
            scrollStyle.paddingBottom = scrollStyle.paddingTop = 
                scrollStyle.paddingLeft = scrollStyle.paddingRight = 8;
            scrollStyle.flexGrow = 1; // fill remaining space
            scrollStyle.borderTopWidth = scrollStyle.borderRightWidth = 
                scrollStyle.borderBottomWidth = scrollStyle.borderLeftWidth = 1;

            scrollStyle.borderTopColor = scrollStyle.borderRightColor = 
                scrollStyle.borderBottomColor = scrollStyle.borderLeftColor = Color.gray;

            summaryLabel = new UitkLabel("Summary report will appear here...");
            IStyle summaryLabelStyle = summaryLabel.style;
            summaryLabelStyle.whiteSpace = WhiteSpace.Normal; // allow wrapping
            summaryLabelStyle.fontSize = 14;
            summaryLabelStyle.unityTextAlign = TextAnchor.UpperLeft;

            summaryScroll.Add(summaryLabel);

            #endregion

        }

        Toggle dryRunToggle;

        private void AddElementsToRoot()
        {
            Root.Add(titleLabel);
            Root.Add(asmDefToChangePicker);
            Root.Add(currentAsmDefNameLabel);
            Root.Add(newAsmDefNameLabel);
            Root.Add(dryRunToggle);
            Root.Add(runButton);
            Root.Add(summaryScroll);
        }

        private void AppendSummary(string message)
        {
            if (summaryLabel != null)
            {
                summaryLabel.text += message + "\n\n";
            }
        }

        protected virtual void OnAsmDefPickerValueChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            UpdateCurrentAsmDefNameLabel();
        }

        private void UpdateCurrentAsmDefNameLabel()
        {
            var currentAsmDefFile = asmDefToChangePicker.value as AssemblyDefinitionAsset;
            

            string updatedCurrentAssemblyName;
            if (currentAsmDefFile != null)
            {
                AsmdefData defData = AsmdefData.FromAsset(currentAsmDefFile);
                updatedCurrentAssemblyName = defData.name;
            }
            else
            {
                updatedCurrentAssemblyName = string.Empty;
            }

            currentAsmDefNameLabel.value = updatedCurrentAssemblyName;
        }

        UitkLabel titleLabel;
        ObjectField asmDefToChangePicker;
        TextField currentAsmDefNameLabel, newAsmDefNameLabel;

        Button runButton;

        private static readonly int gapBetweenStuff = 12;
        private static readonly int asmFieldFontSize = 20;
        private static readonly float labelWidthPercent = 40f;
        private static readonly float runButtonHeight = 40f;

        private void ClearSummary()
        {
            if (summaryLabel != null)
            {
                summaryLabel.text = string.Empty;
            }
        }

        private void RunMigration()
        {
            ClearSummary();
            #region Validation
            bool thereIsAsmDefToChange = asmDefToChangePicker.value != null;
            bool newNameIsValid = !string.IsNullOrEmpty(newAsmDefNameLabel.value) &&
                newAsmDefNameLabel.value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
            if (!thereIsAsmDefToChange)
            {
                AppendSummary("ERROR: Please select an asmdef to change.");
                return;
            }
            if (!newNameIsValid && !IsDryRun)
            {
                AppendSummary("ERROR: Please enter a valid new asmdef name.");
                return;
            }
            #endregion

            #region Migrate ScriptableObjects
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            IDictionary<string, ScriptableObject> assetsToMigrate = new Dictionary<string, ScriptableObject>();

            string oldAsmMarker = $"asm: {CurrentAssemblyName}}}";
            string newAsmMarker = $"asm: {NewAssemblyName}}}";
            string summaryToAppend = string.Empty;
            foreach (string guidEl in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guidEl);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset == null)
                {
                    AppendSummary($"WARNING: Could not load asset at path: {path}");
                    continue;
                }

                // To see if we should consider this asset for migration, we need to check the contents
                // of its YAML. When it references an instance of a type, it has lines like this:
                // asm: {insertAssemblyNameHere}. 
                // What we need to do is change those to asm: {newAssemblyNameHere}.
                string assetYaml = File.ReadAllText(path);
                if (assetYaml == null)
                {
                    AppendSummary($"WARNING: Could not read YAML for asset at path: {path}");
                    continue;
                }

                if (assetYaml.Contains(oldAsmMarker))
                {
                    if (IsDryRun)
                    {
                        summaryToAppend = $"[Dry Run] Would update ScriptableObject {asset.name}'s YAML: " +
                            $"{path}\nFrom: {oldAsmMarker}\nTo: {newAsmMarker}";
                        AppendSummary(summaryToAppend);
                    }
                    assetsToMigrate.Add(path, asset);
                }

            }

            if (!IsDryRun)
            {
                foreach (var pathToAsset in assetsToMigrate.Keys)
                {
                    ScriptableObject asset = assetsToMigrate[pathToAsset];
                    string assetYaml = File.ReadAllText(pathToAsset);
                    string updatedYaml = assetYaml.Replace(oldAsmMarker, newAsmMarker);
                    File.WriteAllText(pathToAsset, updatedYaml);
                    AssetDatabase.ImportAsset(pathToAsset);
                    AppendSummary($"Updated VariableSourceAsset: {pathToAsset}");
                    AssetDatabase.SaveAssetIfDirty(asset);
                }
            }
            #endregion

            // Update the current asmdef file name and all references in other asmdef files
            // Get path to current asmdef file
            AssemblyDefinitionAsset currentAsmDef = asmDefToChangePicker.value as AssemblyDefinitionAsset;
            AsmdefData defData = AsmdefData.FromAsset(currentAsmDef);
            string prevName = defData.name;

            defData.name = NewAssemblyName;

            if (IsDryRun)
            {
                summaryToAppend = $"[Dry Run] Would update part of asmdef {currentAsmDef.name}'s json." +
                    $"\nFrom: {prevName}\nTo: {NewAssemblyName}";
                AppendSummary(summaryToAppend);
            }
            else
            {
                string currentAsmDefPath = AssetDatabase.GetAssetPath(currentAsmDef);
                string updatedJson = JsonUtility.ToJson(defData, true);
                //currentAsmDef.name = NewAssemblyName;
                File.WriteAllText(currentAsmDefPath, updatedJson);
                AssetDatabase.ImportAsset(currentAsmDefPath);
                AppendSummary($"Updated asmdef {currentAsmDef.name} @ {currentAsmDefPath}");
            }

            AssetDatabase.Refresh();
            summaryToAppend = $"Migration {(IsDryRun ? "dry run" : "complete")}. Checked {guids.Length} " +
                $"ScriptableObjects.";
            AppendSummary(summaryToAppend);

        }

        private void OnDisable()
        {
            asmDefToChangePicker.UnregisterValueChangedCallback(OnAsmDefPickerValueChanged);
        }

        [System.Serializable]
        public class AsmdefData
        {
            public string name;
            public string rootNamespace;
            public string[] references;
            public string[] includePlatforms;
            public string[] excludePlatforms;
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public string[] precompiledReferences;
            public string[] defineConstraints;
            public string[] versionDefines;
            public bool noEngineReferences;

            public static AsmdefData FromAsset(AssemblyDefinitionAsset asmDefAsset)
            {
                string pathToFile = AssetDatabase.GetAssetPath(asmDefAsset);
                string json = File.ReadAllText(pathToFile);
                AsmdefData result = JsonUtility.FromJson<AsmdefData>(json);
                return result;
            }
        }

    }
}