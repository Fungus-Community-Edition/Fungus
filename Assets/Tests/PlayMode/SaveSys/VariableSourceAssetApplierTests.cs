using System.Collections;
using System.Collections.Generic;
using System.IO;
using Amanita.SaveSys;
using Amanita.VScripting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;
using Amanita.FSExt;
using UnityEditor;

namespace SaveSystemTests
{
    public class VariableSourceAssetApplierTests : CommonTestFunctionality
    {
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

            VariableSourceAssetSaveData firstVsaSaveData = _saveCodec.EncodeToSave(firstVsa);
            VariableSourceAssetSaveData secondVsaSaveData = _saveCodec.EncodeToSave(secondVsa);

            // Reset variables to different values to verify application will change them
            SetVarValue(firstVsa, "playerName", "ResetName");
            SetVarValue(firstVsa, "playerLevel", 1);

            SetVarValue(secondVsa, "chapter", "ResetChapter");
            SetVarValue(secondVsa, "coins", 0);

            // Act: decode and apply to assets (applier finds by AssetId via Resources)

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

        
        [UnityTest]
        public IEnumerator Apply_WithMismatchedItemId_FallsBackToVarName()
        {
            // Arrange: set known values and encode
            SetVarValue(firstVsa, "playerName", "FallbackName");
            SetVarValue(firstVsa, "playerLevel", 42);

            var data = _saveCodec.EncodeToSave(firstVsa);

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
            var unit = serializer.ToJson(firstVsa);
            var data = serializer.FromJson<VariableSourceAssetSaveData>(unit);

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

            var dataA = _saveCodec.EncodeToSave(firstVsa);
            var dataB = _saveCodec.EncodeToSave(secondVsa);

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