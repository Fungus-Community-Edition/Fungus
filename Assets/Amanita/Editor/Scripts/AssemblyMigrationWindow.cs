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
        private string currentAssemblyName = "Amanita";
        private string newAssemblyName = "Amanita.Core";
        private DefaultAsset asmdefFile;

        [MenuItem("Window/Amanita/Assembly Migration")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<AssemblyMigrationWindow>();
            wnd.titleContent = new GUIContent("Assembly Migration");
            wnd.minSize = wnd.maxSize = windowSize;
        }

        private readonly static Vector2 windowSize = new Vector2(600, 400);

        public void CreateGUI()
        {
            var root = rootVisualElement;
            IStyle rootStyle = root.style;
            rootStyle.paddingLeft = rootStyle.paddingRight = 20;
            rootStyle.paddingTop = rootStyle.paddingBottom = 10;

            PrepFields();
            #region Add Elements to Root
            root.Add(titleLabel);
            root.Add(asmDefToChangePicker);
            root.Add(currentAsmDefNameLabel);
            root.Add(newAsmDefNameLabel);
            root.Add(dryRunToggle);
            root.Add(runButton);
            #endregion
        }

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
            runButtonStyle.fontSize = asmFieldFontSize;
            runButtonStyle.height = runButtonHeight;
            #endregion

            UpdateCurrentAsmDefNameLabel();
        }

        Toggle dryRunToggle;

        protected virtual void OnAsmDefPickerValueChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            asmdefFile = evt.newValue as DefaultAsset;
            UpdateCurrentAsmDefNameLabel();
        }

        private void UpdateCurrentAsmDefNameLabel()
        {
            var currentAsmDefFile = asmDefToChangePicker.value as AssemblyDefinitionAsset;
            if (currentAsmDefFile != null)
            {
                currentAssemblyName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(currentAsmDefFile));
            }
            else
            {
                currentAssemblyName = string.Empty;
            }

            currentAsmDefNameLabel.value = currentAssemblyName;
        }

        UitkLabel titleLabel;
        ObjectField asmDefToChangePicker;
        TextField currentAsmDefNameLabel, newAsmDefNameLabel;

        Button runButton;

        private static readonly int gapBetweenStuff = 12;
        private static readonly int asmFieldFontSize = 20;
        private static readonly float labelWidthPercent = 40f;
        private static readonly float runButtonHeight = 40f;

        private void RunMigration()
        {
            #region Validation
            bool thereIsAsmDefToChange = asmDefToChangePicker.value != null;
            bool newNameIsValid = !string.IsNullOrEmpty(newAsmDefNameLabel.value) &&
                newAsmDefNameLabel.value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
            if (!thereIsAsmDefToChange)
            {
                Debug.LogError("Please select an asmdef to change.");
                return;
            }
            if (!newNameIsValid && !IsDryRun)
            {
                Debug.LogError("Please enter a valid new asmdef name.");
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
                    Debug.LogWarning($"Could not load asset at path: {path}");
                    continue;
                }

                if (IsDryRun)
                {
                    Debug.Log($"[Dry Run] Would mark VariableSourceAsset dirty: {path}");
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
                asmdefFile = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);

                if (json.Contains(currentAssemblyName))
                {
                    string updatedJson = json.Replace(currentAssemblyName, newAssemblyName);

                    if (IsDryRun)
                    {
                        Debug.Log($"[Dry Run] Would update asmdef: {path}\nFrom: {currentAssemblyName}\nTo:   {newAssemblyName}");
                    }
                    else
                    {
                        File.WriteAllText(path, updatedJson);
                        AssetDatabase.ImportAsset(path);
                        Debug.Log($"Updated asmdef: {path}");
                    }

                }

            }
            #endregion

            AssetDatabase.Refresh();
            Debug.Log($"Migration {(IsDryRun ? "dry run" : "complete")}. Checked {guids.Length} VariableSourceAssets and {asmdefGuids.Length} asmdefs.");

        }

        private void OnDisable()
        {
            asmDefToChangePicker.UnregisterValueChangedCallback(OnAsmDefPickerValueChanged);
        }
    }
}