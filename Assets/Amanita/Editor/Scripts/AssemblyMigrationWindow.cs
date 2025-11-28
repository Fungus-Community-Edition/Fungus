using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Collections.Generic;
using Amanita.VScripting;
using UitkLabel = UnityEngine.UIElements.Label;
using System;
using UnityEditorInternal;

namespace Amanita.EditorUtils
{
    public class AssemblyMigrationWindow : EditorWindow
    {
        private string CurrentAssemblyName => currentAsmDefNameLabel?.value;
        private string NewAssemblyName => newAsmDefNameLabel?.value;

        [MenuItem("Window/Amanita/Assembly Migration")]
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
                updatedCurrentAssemblyName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(currentAsmDefFile));
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

            #region Migrate VariableSourceAssets
            string[] guids = AssetDatabase.FindAssets("t:VariableSourceAsset");
            IList<VariableSourceAsset> assetsToMigrate = new List<VariableSourceAsset>();

            foreach (string guidEl in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guidEl);
                var asset = AssetDatabase.LoadAssetAtPath<VariableSourceAsset>(path);

                if (asset == null)
                {
                    AppendSummary($"WARNING: Could not load asset at path: {path}");
                    continue;
                }

                if (IsDryRun)
                {
                    AppendSummary($"[Dry Run] Would mark VariableSourceAsset dirty: {path}");
                }
                else
                {
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssetIfDirty(asset);
                }

            }

            foreach (var asset in assetsToMigrate)
            {
                AssetDatabase.SaveAssetIfDirty(asset);
            }
            #endregion

            #region Update Asmdef Files
            if (!IsDryRun)
            {
                var currentAsmDefFile = asmDefToChangePicker.value as AssemblyDefinitionAsset;
                currentAsmDefFile.name = newAsmDefNameLabel.value;
            }

            string[] asmdefGuids = AssetDatabase.FindAssets("t:asmdef");
            foreach (string guidEl in asmdefGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guidEl);
                string json = File.ReadAllText(path);
                var asmdefFile = asmDefToChangePicker.value as AssemblyDefinitionAsset;

                if (json.Contains(CurrentAssemblyName))
                {
                    string updatedJson = json.Replace(CurrentAssemblyName, NewAssemblyName);

                    if (IsDryRun)
                    {
                        AppendSummary($"[Dry Run] Would update asmdef: {path}\nFrom: {CurrentAssemblyName}\nTo: {NewAssemblyName}");
                    }
                    else
                    {
                        File.WriteAllText(path, updatedJson);
                        AssetDatabase.ImportAsset(path);
                        AppendSummary($"Updated asmdef: {path}");
                    }

                }

            }
            #endregion

            AssetDatabase.Refresh();
            AppendSummary($"Migration {(IsDryRun ? "dry run" : "complete")}. Checked {guids.Length} VariableSourceAssets and {asmdefGuids.Length} asmdefs.");

        }

        private void OnDisable()
        {
            asmDefToChangePicker.UnregisterValueChangedCallback(OnAsmDefPickerValueChanged);
        }
    }
}