using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amanita.SaveSys;
using Amanita.VScripting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaveSystemTests
{
    public class VariableSourceAssetApplierTests : CommonTestFunctionality
    {
        // Simple test VarCodec that handles GenericMuscariable (object) for strings and ints
        // We avoid mocks of variables/sources by using real VariableSourceAsset and Muscariable,
        // but provide this tiny codec so encode/decode can occur in tests.
        private class GenericVarCodec : ScriptableObject, IVarCodec
        {
            public bool CanHandle(IVariable variable)
                => variable is GenericMuscariable;

            public bool CanHandle(string typeName)
                => typeName == nameof(GenericMuscariable) || typeName == typeof(GenericMuscariable).FullName;

            public bool CanHandle(VariableSaveData variable)
                => variable != null && (variable.VarTypeName == nameof(GenericMuscariable) || variable.VarTypeName == typeof(GenericMuscariable).FullName);

            public string EncodeToString(IVariable variable)
            {
                return variable?.BoxedValue != null ? variable.BoxedValue.ToString() : string.Empty;
            }

            public void Decode(IVariable variable, string data)
            {
                // Best-effort roundtrip: try int, else string
                if (int.TryParse(data, out var i))
                {
                    variable.BoxedValue = i;
                }
                else
                {
                    variable.BoxedValue = data;
                }
            }

            public void Decode(IVariable variable, VariableSaveData data)
            {
                if (data == null) return;
                Decode(variable, data.Value);
            }

            public T DecodeTo<T>(string data)
            {
                object result = default(T);

                // try to coerce to T from string
                if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(data, out var i))
                        result = i;
                }
                else if (typeof(T) == typeof(string))
                {
                    result = data;
                }

                return (T)result;
            }

            public VariableSaveData EncodeToSave(IVariable variable)
            {
                return new VariableSaveData
                {
                    VarTypeName = nameof(GenericMuscariable),
                    ItemId = variable.ItemId,
                    Key = variable.Key,
                    Value = EncodeToString(variable)
                };
            }
        }

        private const string ResourcesFolder = "Assets/Resources";
        private const string TestResourcesSubFolder = "Assets/Resources/VarSrcApplierTests";
        private const string AssetNameA = "TestVarSrcA.asset";
        private const string AssetNameB = "TestVarSrcB.asset";


        private GenericVarCodec genericVarCodec;
        private VariableSourceAssetSaveCodec _saveCodec;
        private VariableSourceAssetApplier _applier;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            // Ensure Resources path
            if (!AssetDatabase.IsValidFolder(ResourcesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder(TestResourcesSubFolder))
            {
                AssetDatabase.CreateFolder(ResourcesFolder, "VarSrcApplierTests");
            }

            // Create VariableSourceAssets as real assets in Resources so the applier can find them
            firstVsa = CreateVarSourceAsset(Path.Combine(TestResourcesSubFolder, AssetNameA));
            secondVsa = CreateVarSourceAsset(Path.Combine(TestResourcesSubFolder, AssetNameB));

            // Create variables on A
            var firstStringMuscari = firstVsa.AddNewVariableOfContentType<string>("playerName", "Amanita");
            var firstIntMuscari = firstVsa.AddNewVariableOfContentType<int>("playerLevel", 3);
            Assert.NotNull(firstStringMuscari);
            Assert.NotNull(firstIntMuscari);
            // Ensure stable IDs and owner
            firstVsa.Refresh();

            // Create variables on B
            var secondStringMuscari = secondVsa.AddNewVariableOfContentType<string>("chapter", "Intro");
            var secondIntMuscari = secondVsa.AddNewVariableOfContentType<int>("coins", 25);
            Assert.NotNull(secondStringMuscari);
            Assert.NotNull(secondIntMuscari);
            secondVsa.Refresh();

            // Save assets to disk
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Create codec and hook it up to both encoder and applier
            genericVarCodec = ScriptableObject.CreateInstance<GenericVarCodec>();

            _saveCodec = ScriptableObject.CreateInstance<VariableSourceAssetSaveCodec>();
            _saveCodec.RegisterVarCodec(genericVarCodec);
            var builtInVarSaveCodec = ScriptableObject.CreateInstance<BuiltinVarSaveCodec>();
            _saveCodec.RegisterVarCodec(builtInVarSaveCodec);

            _applier = ScriptableObject.CreateInstance<VariableSourceAssetApplier>();
            _applier.RegisterVarCodec(genericVarCodec);
            _applier.RegisterVarCodec(builtInVarSaveCodec);
            _applier.Init();

            // Wait a frame for Resources changes to settle
            yield return null;

            toDestroyInTearDown.Add(genericVarCodec);
            toDestroyInTearDown.Add(_saveCodec);
            toDestroyInTearDown.Add(_applier);
#else
            yield break;
#endif
        }

        private VariableSourceAsset firstVsa;
        private VariableSourceAsset secondVsa;
        [UnityTearDown]
        public IEnumerator TearDown()
        {
#if UNITY_EDITOR
            // Clean Resources assets we created
            TryDeleteAsset(Path.Combine(TestResourcesSubFolder, AssetNameA));
            TryDeleteAsset(Path.Combine(TestResourcesSubFolder, AssetNameB));

            foreach (var obj in toDestroyInTearDown)
            {
                if (obj != null)
                {
                    UnityObj.DestroyImmediate(obj);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
            yield return null;
        }

        // 1) Recording states of a VariableSourceAsset on disk
        //    - Includes each Muscariable and assetIds
        [UnityTest]
        public IEnumerator Encode_VariableSourceAsset_RecordsAssetIdAndEachVariable()
        {
            // Act
            var firstUnit = _saveCodec.EncodeToUnit(firstVsa);
            var secondUnit = _saveCodec.EncodeToUnit(secondVsa);

            // Assert content decodes back
            var firstDecodeResult = (VariableSourceAssetSaveData)_saveCodec.DecodeFrom(firstUnit);
            var secondDecodeResult = (VariableSourceAssetSaveData)_saveCodec.DecodeFrom(secondUnit);

            Assert.NotNull(firstDecodeResult);
            Assert.NotNull(secondDecodeResult);

            // AssetIds should match the source assets
            Assert.AreEqual(firstVsa.AssetId, firstDecodeResult.AssetId, "First Asset AssetId mismatch");
            Assert.AreEqual(secondVsa.AssetId, secondDecodeResult.AssetId, "Second Asset AssetId mismatch");

            // Each muscariable should be present with itemId and key recorded
            // Asset A
            var firstVars = firstVsa.Variables.ToList();
            Assert.GreaterOrEqual(firstDecodeResult.SavedVars.Count, firstVars.Count);
            foreach (var elem in firstVars.Cast<IVariable>())
            {
                var found = firstDecodeResult.SavedVars.FirstOrDefault(sv => sv.ItemId == elem.ItemId || sv.Key == elem.Key);
                Assert.NotNull(found, $"Missing saved var for first asset: {elem.Key} (ID {elem.ItemId})");
            }

            // Asset B
            var secondVars = secondVsa.Variables.ToList();
            Assert.GreaterOrEqual(secondDecodeResult.SavedVars.Count, secondVars.Count);
            foreach (var elem in secondVars.Cast<IVariable>())
            {
                var found = secondDecodeResult.SavedVars.FirstOrDefault(sv => sv.ItemId == elem.ItemId || sv.Key == elem.Key);
                Assert.NotNull(found, $"Missing saved var for second asset: {elem.Key} (ID {elem.ItemId})");
            }

            yield return null;
        }

        // 2) Applying VariableSourceAssetSaveDatas to the right assets
        //    - Includes Muscariables getting the right values
        [UnityTest]
        public IEnumerator Apply_SaveData_UpdatesRightAssetAndValues()
        {
            // Arrange: change values, encode as "save"
            SetVarValue(firstVsa, "playerName", "Shiitake");
            SetVarValue(firstVsa, "playerLevel", 99);

            SetVarValue(secondVsa, "chapter", "Finale");
            SetVarValue(secondVsa, "coins", 777);

            SaveDataUnit firstVsaSaveUnit = _saveCodec.EncodeToUnit(firstVsa);
            SaveDataUnit secondVsaSaveUnit = _saveCodec.EncodeToUnit(secondVsa);

            // Reset variables to different values to verify application will change them
            SetVarValue(firstVsa, "playerName", "ResetName");
            SetVarValue(firstVsa, "playerLevel", 1);

            SetVarValue(secondVsa, "chapter", "ResetChapter");
            SetVarValue(secondVsa, "coins", 0);

            // Act: decode and apply to assets (applier finds by AssetId via Resources)
            var firstVsaSaveData = (VariableSourceAssetSaveData)_saveCodec.DecodeFrom(firstVsaSaveUnit);
            var secondVsaSaveData = (VariableSourceAssetSaveData)_saveCodec.DecodeFrom(secondVsaSaveUnit);

            // Apply A then B
            yield return _applier.Apply(firstVsaSaveData).AsIEnumerator();
            yield return _applier.Apply(secondVsaSaveData).AsIEnumerator();

            // Assert values restored to saved ones
            string firstStringVarValue = GetVarValue<string>(firstVsa, "playerName");
            Assert.AreEqual("Shiitake", firstStringVarValue);

            int firstIntVarValue = GetVarValue<int>(firstVsa, "playerLevel");
            Assert.AreEqual(99, firstIntVarValue);

            string secondStringVarValue = GetVarValue<string>(secondVsa, "chapter");
            Assert.AreEqual("Finale", secondStringVarValue);

            int secondIntVarValue = GetVarValue<int>(secondVsa, "coins");
            Assert.AreEqual(777, secondIntVarValue);

            yield return null;
        }

        // Helper to get typed muscariable value through BoxedValue
        private static T GetVarValue<T>(VariableSourceAsset asset, string key)
        {
            var varToCheck = ((IMuscariableSource)asset).GetVariable(key);
            Assert.NotNull(varToCheck, $"Var '{key}' not found on asset '{asset.name}'");
            return varToCheck is Muscariable<T> typed ? typed.Value : (T)varToCheck.BoxedValue;
        }

        private static void SetVarValue<T>(VariableSourceAsset asset, string key, T value)
        {
            var varToSetValOf = ((IMuscariableSource)asset).GetVariable(key);
            Assert.NotNull(varToSetValOf, $"Var '{key}' not found on asset '{asset.name}'");
            varToSetValOf.BoxedValue = value;
        }

#if UNITY_EDITOR
        private static VariableSourceAsset CreateVarSourceAsset(string assetPath)
        {
            var instance = ScriptableObject.CreateInstance<VariableSourceAsset>();
            AssetDatabase.CreateAsset(instance, assetPath);
            // Force OnValidate to create an AssetId if needed
            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();
            return instance;
        }

        private static void TryDeleteAsset(string assetPath)
        {
            if (File.Exists(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
#endif

        [UnityTest]
        public IEnumerator EncodeToSave_Respects_IncludeInSaves_False()
        {
#if UNITY_EDITOR
            // Flip flag off via SerializedObject
            var so = new SerializedObject(firstVsa);
            so.FindProperty("includeInSaves").boolValue = false;
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            // Act
            var saveData = _saveCodec.EncodeToSave(firstVsa);

            // Assert
            Assert.IsNull(saveData, "EncodeToSave should return null when IncludeInSaves is false.");

            // Restore for other tests
            so.Update();
            so.FindProperty("includeInSaves").boolValue = true;
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
#endif
            yield return null;
        }

        [UnityTest]
        public IEnumerator Apply_WithMismatchedItemId_FallsBackToVarName()
        {
            // Arrange: set known values and encode
            SetVarValue(firstVsa, "playerName", "FallbackName");
            SetVarValue(firstVsa, "playerLevel", 42);

            var unit = _saveCodec.EncodeToUnit(firstVsa);
            var data = (VariableSourceAssetSaveData)_saveCodec.DecodeFrom(unit);

            // Corrupt all ItemIds so lookup by ID fails
            foreach (var sv in data.SavedVars)
            {
                sv.ItemId += 999999; // ensure no match
            }

            // Change current values so we can detect the apply
            SetVarValue(firstVsa, "playerName", "BeforeApplyName");
            SetVarValue(firstVsa, "playerLevel", -1);

            // Act
            yield return _applier.Apply(data).AsIEnumerator();

            // Assert: values still applied via name fallback
            Assert.AreEqual("FallbackName", GetVarValue<string>(firstVsa, "playerName"));
            Assert.AreEqual(42, GetVarValue<int>(firstVsa, "playerLevel"));
        }

        [UnityTest]
        public IEnumerator Apply_UnknownAssetId_WarnsAndDoesNothing()
        {
            // Capture pre-values
            var originalName = GetVarValue<string>(firstVsa, "playerName");
            var originalLevel = GetVarValue<int>(firstVsa, "playerLevel");

            // Create save data with unknown AssetId
            var bogus = new VariableSourceAssetSaveData
            {
                AssetId = "this-id-does-not-exist"
            };

            // Expect a warning
            LogAssert.Expect(LogType.Warning,
                $"No VariableSourceAsset with AssetId {bogus.AssetId} was found to apply save data to.");

            // Act
            yield return _applier.Apply(bogus).AsIEnumerator();

            // Assert unchanged
            Assert.AreEqual(originalName, GetVarValue<string>(firstVsa, "playerName"));
            Assert.AreEqual(originalLevel, GetVarValue<int>(firstVsa, "playerLevel"));
        }

        [UnityTest]
        public IEnumerator Apply_VariableWithoutCodec_SkipsAndWarns()
        {
            // Arrange: encode current values
            SetVarValue(firstVsa, "playerName", "CodecSkipName");
            var unit = _saveCodec.EncodeToUnit(firstVsa);
            var data = (VariableSourceAssetSaveData)_saveCodec.DecodeFrom(unit);

            // Tamper VarTypeName so no codec matches
            foreach (var sv in data.SavedVars)
            {
                sv.VarTypeName = "UnknownTypeToForceNoCodec";
            }

            // Change live value to detect if it gets changed (it shouldn't)
            SetVarValue(firstVsa, "playerName", "Unchanged");

            // Expect one warning per variable in the save data
            for (int i = 0; i < data.SavedVars.Count; i++)
            {
                LogAssert.Expect(LogType.Warning, "No codec found for variable type: VariableSaveData");
            }

            // Act
            yield return _applier.Apply(data).AsIEnumerator();

            // Assert: value is not changed
            Assert.AreEqual("Unchanged", GetVarValue<string>(firstVsa, "playerName"));
        }

        [UnityTest]
        public IEnumerator ApplyRange_AppliesMultipleSaveDatas()
        {
            // Arrange values and encode both
            SetVarValue(firstVsa, "playerName", "RangeName");
            SetVarValue(secondVsa, "chapter", "RangeChapter");

            var dataA = (VariableSourceAssetSaveData)_saveCodec.DecodeFrom(_saveCodec.EncodeToUnit(firstVsa));
            var dataB = (VariableSourceAssetSaveData)_saveCodec.DecodeFrom(_saveCodec.EncodeToUnit(secondVsa));

            // Change current to detect apply
            SetVarValue(firstVsa, "playerName", "BeforeRange");
            SetVarValue(secondVsa, "chapter", "BeforeRange");

            // Act
            var list = new List<SaveData> { dataA, dataB };
            yield return _applier.ApplyRange(list).AsIEnumerator();

            // Assert
            Assert.AreEqual("RangeName", GetVarValue<string>(firstVsa, "playerName"));
            Assert.AreEqual("RangeChapter", GetVarValue<string>(secondVsa, "chapter"));
        }

        [UnityTest]
        public IEnumerator SaveCodec_FindAndEncodeAll_FindsBothAssets()
        {
            // Act
            var units = _saveCodec.FindAndEncodeAll();

            // Assert: decode and ensure both assetIds are present
            var ids = units
                .Select(u => (VariableSourceAssetSaveData)_saveCodec.DecodeFrom(u))
                .Where(d => d != null)
                .Select(d => d.AssetId)
                .ToList();

            Assert.Contains(firstVsa.AssetId, ids);
            Assert.Contains(secondVsa.AssetId, ids);

            yield return null;
        }
    }

    internal static class TaskExtensions
    {
        public static IEnumerator AsIEnumerator(this System.Threading.Tasks.Task task)
        {
            while (!task.IsCompleted)
            {
                if (task.IsFaulted)
                { 
                    throw task.Exception;
                }
                yield return null;
            }

            yield return null; // One final yield to ensure completion
        }
    }
}