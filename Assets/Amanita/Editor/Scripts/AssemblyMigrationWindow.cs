using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using System.Text.RegularExpressions;

namespace AtMycelia.EditorUtils
{
    public class AssemblyMigrationWindow : EditorWindow
    {
        private string CurrentAssemblyName => currentAsmDefNameLabel?.value;
        private string NewAssemblyName => newAsmDefNameLabel?.value;

        [MenuItem("Window/Atelier Mycelia/Assembly Migration")]
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
                //currentAsmDefNameLabel.isReadOnly = true;
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

        public void AppendSummary(string message)
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
            // ^That extra } at the end is important. Need to include it to avoid matching the wrong parts of the yaml.
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
                    AppendSummary($"Updated ScriptableObject: {pathToAsset}");
                    AssetDatabase.SaveAssetIfDirty(asset);
                }
            }
            #endregion

            // Now we go through all other asmdefs and update references to this one, keeping
            // in mind whether or not they likely use guids or not.
            AssemblyDefinitionAsset asmDefWeAreChangingNameOf = asmDefToChangePicker.value as AssemblyDefinitionAsset;

            #region Migrate Asmdef References in Other Asmdefs
            string[] asmDefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");
            foreach (string elem in asmDefGuids)
            {
                string asmDefPath = AssetDatabase.GUIDToAssetPath(elem);
                var asmDefAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(asmDefPath);
                if (asmDefAsset == asmDefWeAreChangingNameOf)
                {
                    continue; 
                }

                AsmdefData asmDefData = AsmdefData.FromAsset(asmDefAsset);
                bool needsUpdate = false;
                for (int i = 0; i < asmDefData.references.Length; i++)
                {
                    string reference = asmDefData.references[i];
                    
                    if (reference.Contains(CurrentAssemblyName))
                    {
                        asmDefData.references[i] = reference.Replace(CurrentAssemblyName, NewAssemblyName);
                        needsUpdate = true;
                    }
                }

                if (needsUpdate)
                {
                    if (IsDryRun)
                    {
                        summaryToAppend = $"[Dry Run] Would update asmdef {asmDefAsset.name}'s references in its json.\n" +
                            "(It seems to NOT reference other assemblies through guids.)\n" +
                        $"From: references containing {CurrentAssemblyName}\nTo: references containing {NewAssemblyName}";
                    }
                    else
                    {
                        string updatedJson = JsonUtility.ToJson(asmDefData, true);
                        File.WriteAllText(asmDefPath, updatedJson);
                        AssetDatabase.ImportAsset(asmDefPath);
                        summaryToAppend = $"Updated asmdef {asmDefAsset.name}'s references in its json." +
                            $"\nFrom: references containing {CurrentAssemblyName}\nTo: references containing {NewAssemblyName}";
                    }

                    AppendSummary(summaryToAppend);
                }
            }
            #endregion

            #region Update Current Asmdef File
            // Update the current asmdef file name and all references in other asmdef files
            // Get path to current asmdef file
            AsmdefData defData = AsmdefData.FromAsset(asmDefWeAreChangingNameOf);
            string prevName = defData.name;
            defData.name = NewAssemblyName;

            if (IsDryRun)
            {
                summaryToAppend = $"[Dry Run] Would update part of asmdef {asmDefWeAreChangingNameOf.name}'s json." +
                    $"\nFrom: {prevName}\nTo: {NewAssemblyName}";
                AppendSummary(summaryToAppend);
            }
            else
            {
                string currentAsmDefPath = AssetDatabase.GetAssetPath(asmDefWeAreChangingNameOf);
                string updatedJson = JsonUtility.ToJson(defData, true);
                asmDefWeAreChangingNameOf.name = NewAssemblyName;
                File.WriteAllText(currentAsmDefPath, updatedJson);
                AssetDatabase.ImportAsset(currentAsmDefPath);
                AppendSummary($"Updated asmdef {asmDefWeAreChangingNameOf.name} @ {currentAsmDefPath}");
            }
            #endregion

            #region Update References to the old assembly in Uxmls

            // Need to find all assets that are UXML files
            string[] uxmlGuids = AssetDatabase.FindAssets("t:VisualTreeAsset");
            foreach (string guidElem in uxmlGuids)
            {
                string uxmlPath = AssetDatabase.GUIDToAssetPath(guidElem);
                var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                if (uxmlAsset == null)
                {
                    AppendSummary($"WARNING: Could not load UXML asset at path: {uxmlPath}");
                    continue;
                }
                string uxmlText = File.ReadAllText(uxmlPath);
                if (uxmlText == null)
                {
                    AppendSummary($"WARNING: Could not read UXML text for asset at path: {uxmlPath}");
                    continue;
                }

                // Use the UxmlFixer to fix the uxml text
                UxmlAssemblyFixer.FixUxmlFile(uxmlPath, CurrentAssemblyName, NewAssemblyName, this, IsDryRun);

            }

            #endregion
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
            public string name = string.Empty;
            public string rootNamespace = string.Empty;
            public string[] references = new string[] { };
            public string[] includePlatforms = new string[] { };
            public string[] excludePlatforms = new string[] { };
            public bool allowUnsafeCode = false;
            public bool overrideReferences = false;
            public string[] precompiledReferences = new string[] { };
            public bool autoReferenced = false;
            public string[] defineConstraints = new string[] { };
            public string[] versionDefines = new string[] { };
            public bool noEngineReferences = false;

            public static AsmdefData FromAsset(AssemblyDefinitionAsset asmDefAsset)
            {
                string pathToFile = AssetDatabase.GetAssetPath(asmDefAsset);
                string json = File.ReadAllText(pathToFile);
                AsmdefData result = JsonUtility.FromJson<AsmdefData>(json);
                return result;
            }

            public bool LikelyGoesWithGuids
            {
                get
                {
                    // We can tell based on the contents of the references array. If it has anything
                    // that starts with "GUID:", then it's likely that this asmdef goes with guids.
                    // Otherwise, it instead references things by assembly name. The name in the json,
                    // not the name of the file on disk.
                    if (references != null)
                    {
                        foreach (var reference in references)
                        {
                            if (reference.StartsWith("GUID:"))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
        }

    }


    public static class UxmlAssemblyFixer
    {
        // Regex to capture type attributes with assembly names
        // Example match: type="Amanita.VScripting.VariableScope, Amanita"
        // We want to make sure to match the exact assembly name only,
        // not substrings inside longer names.
        static Regex TypeRegex(string oldAssembly) => new Regex(
            $@"(type|data-source-type)=""([^""]+),\s*({Regex.Escape(oldAssembly)})""",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Scans a UXML file and replaces old assembly names with new ones.
        /// </summary>
        public static void FixUxmlFile(string path,
                                       string oldAssembly,
                                       string newAssembly,
                                       AssemblyMigrationWindow migrationWindow,
                                       bool dryRun = true)
        {
            string text = File.ReadAllText(path);
            bool changed = false;

            var regex = TypeRegex(oldAssembly);

            string result = regex.Replace(text, match =>
            {
                string attrName = match.Groups[1].Value; // "type" or "data-source-type"
                string fullType = match.Groups[2].Value; // e.g. Amanita.VScripting.VariableScope
                string assembly = match.Groups[3].Value; // e.g. Amanita

                bool exactMatch = assembly.Equals(oldAssembly);
                if (exactMatch)
                {
                    changed = true;
                    return $"{attrName}=\"{fullType}, {newAssembly}\"";
                }
                return match.Value;
            });

            string summaryToAppend = string.Empty;
            if (changed)
            {
                if (dryRun)
                {
                    summaryToAppend = $"[Dry Run] Would update UXML file: {path}\n" +
                        $"From assembly: {oldAssembly}\nTo assembly: {newAssembly}";
                }
                else
                {
                    File.WriteAllText(path, result);
                    summaryToAppend = $"Updated UXML file: {path}\n" +
                        $"From assembly: {oldAssembly}\nTo assembly: {newAssembly}";
                }

                migrationWindow.AppendSummary(summaryToAppend);
            }
        }

    }

}