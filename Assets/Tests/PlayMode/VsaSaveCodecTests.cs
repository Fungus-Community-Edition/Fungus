using Amanita.SaveSys;
using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;

namespace SaveSystemTests
{
    public class VsaSaveCodecTests : CommonTestFunctionality
    {
        // ---- Resource / Asset Paths ----
        private const string ResourcesFolder = "Assets/Resources";
        private const string TestResourcesSubFolder = "Assets/Resources/VarSrcApplierTests";
        private const string FirstAssetName = "TestVarSrcA.asset";
        private const string SecondAssetName = "TestVarSrcB.asset";

        // ---- Test Assets / Codecs ----
        private VariableSourceAsset firstVsa;
        private VariableSourceAsset secondVsa;
        private GenericVarCodec genericVarCodec;
        private VariableSourceAssetSaveCodec vsaSaveCodec;
        private VariableSourceAssetApplier vsaApplier;

        // ---- Setup / Teardown ----
        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            EnsureResourcesFolders();
            CreateAndRegisterVariableSourceAssets();
            CreateVariablesOnAssets();
            PersistAssetsToDisk();
            InstantiateCodecsAndApplier();

            // Allow a frame for Resources DB changes to settle.
            yield return null;

            TrackObjectsForTeardown();
#else
            yield break;
#endif
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
#if UNITY_EDITOR
            DeleteCreatedTestAssets();

            foreach (var obj in toDestroyInTearDown)
            {
                if (obj != null)
                    UnityObj.DestroyImmediate(obj);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
            yield return null;
        }

        // ---- Tests ----

        [Test]
        public void RecordsAssetIds()
        {
            var (firstSaveData, secondSaveData) = EncodeBoth();

            bool recordsAssetID =
                firstSaveData.UniqueId == firstVsa.UniqueId &&
                secondSaveData.UniqueId == secondVsa.UniqueId;

            Assert.IsTrue(recordsAssetID,
                "Encoded VariableSourceAssetSaveData should record the AssetId of the VariableSourceAsset it was created from.");
        }

        [Test]
        public void RecordsKeysOfEachVar()
        {
            var (firstSaveData, secondSaveData) = EncodeBoth();

            var expectedFirstKeys = firstVsa.Variables.Select(v => v.Key).ToList();
            var expectedSecondKeys = secondVsa.Variables.Select(v => v.Key).ToList();

            IList<string> encodedFirstKeys = firstSaveData.SavedVars.Select(vs => vs.Key).ToList();
            IList<string> encodedSecondKeys = secondSaveData.SavedVars.Select(vs => vs.Key).ToList();

            bool success = expectedFirstKeys.SequenceEqual(encodedFirstKeys) &&
                           expectedSecondKeys.SequenceEqual(encodedSecondKeys);

            Assert.IsTrue(success,
                "The keys of each variable should be recorded in the VariableSourceAssetSaveData.");
        }

        [Test]
        public void RecordsItemIdsOfEachVar()
        {
            var (firstSaveData, secondSaveData) = EncodeBoth();

            IList<byte> encodedFirstItemIds = firstSaveData.SavedVars.Select(vs => vs.ItemId).ToList();
            IList<byte> encodedSecondItemIds = secondSaveData.SavedVars.Select(vs => vs.ItemId).ToList();

            var expectedFirstItemIds = firstVsa.Variables.Select(v => v.ItemId).ToList();
            var expectedSecondItemIds = secondVsa.Variables.Select(v => v.ItemId).ToList();

            bool success = expectedFirstItemIds.SequenceEqual(encodedFirstItemIds) &&
                           expectedSecondItemIds.SequenceEqual(encodedSecondItemIds);

            Assert.IsTrue(success,
                "The ItemIds of each variable should be recorded in the VariableSourceAssetSaveData.");
        }

        [UnityTest]
        public IEnumerator Respects_IncludeInSaves_False()
        {
            firstVsa.IncludeInSaves = false;

            var saveData = Encode(firstVsa);

            Assert.IsNull(saveData,
                "Save data should be null for VariableSourceAsset with IncludeInSaves = false");

            firstVsa.IncludeInSaves = true; // Restore for other tests
            yield return null;
        }

        // ---- Encoding Helpers ----
        private VariableSourceAssetSaveData Encode(VariableSourceAsset vsa) =>
            vsaSaveCodec.EncodeToSave(vsa);

        private (VariableSourceAssetSaveData first, VariableSourceAssetSaveData second) EncodeBoth() =>
            (Encode(firstVsa), Encode(secondVsa));

        // ---- Setup Helpers ----
        private void EnsureResourcesFolders()
        {
            if (!AssetDatabase.IsValidFolder(ResourcesFolder))
                AssetDatabase.CreateFolder("Assets", "Resources");

            if (!AssetDatabase.IsValidFolder(TestResourcesSubFolder))
                AssetDatabase.CreateFolder(ResourcesFolder, "VarSrcApplierTests");
        }

        private void CreateAndRegisterVariableSourceAssets()
        {
            firstVsa = CreateVarSourceAsset($"{TestResourcesSubFolder}/{FirstAssetName}");
            secondVsa = CreateVarSourceAsset($"{TestResourcesSubFolder}/{SecondAssetName}");

            // Register test-only UID so GUID cleanup matches harness expectations.
            RegisterTestOnlyVsa(firstVsa);
            RegisterTestOnlyVsa(secondVsa);
        }

        private void CreateVariablesOnAssets()
        {
            // Asset A
            var playerNameVar = firstVsa.AddNewVariableOfContentType<string>("playerName", "Amanita");
            var playerLevelVar = firstVsa.AddNewVariableOfContentType<int>("playerLevel", 3);
            Assert.NotNull(playerNameVar);
            Assert.NotNull(playerLevelVar);
            firstVsa.Refresh(); // Ensure stable IDs

            // Asset B
            var chapterVar = secondVsa.AddNewVariableOfContentType<string>("chapter", "Intro");
            var coinsVar = secondVsa.AddNewVariableOfContentType<int>("coins", 25);
            Assert.NotNull(chapterVar);
            Assert.NotNull(coinsVar);
            secondVsa.Refresh();
        }

        private void PersistAssetsToDisk()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void InstantiateCodecsAndApplier()
        {
            // Test codec + applier instances (mirrors style in CommonTestFunctionality)
            genericVarCodec = ScriptableObject.CreateInstance<GenericVarCodec>();
            vsaSaveCodec = ScriptableObject.CreateInstance<VariableSourceAssetSaveCodec>();
            vsaApplier = ScriptableObject.CreateInstance<VariableSourceAssetApplier>();
            vsaApplier.PreInstallInit();
        }

        private void TrackObjectsForTeardown()
        {
            toDestroyInTearDown.Add(genericVarCodec);
            toDestroyInTearDown.Add(vsaSaveCodec);
            toDestroyInTearDown.Add(vsaApplier);
        }

        private static VariableSourceAsset CreateVarSourceAsset(string assetPath)
        {
            var instance = ScriptableObject.CreateInstance<VariableSourceAsset>();
            AssetDatabase.CreateAsset(instance, assetPath);
            // Force OnValidate to generate an AssetId if needed.
            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();
            return instance;
        }

        private static void DeleteAssetIfExists(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null)
                AssetDatabase.DeleteAsset(assetPath);
        }

        private void DeleteCreatedTestAssets()
        {
            DeleteAssetIfExists($"{TestResourcesSubFolder}/{FirstAssetName}");
            DeleteAssetIfExists($"{TestResourcesSubFolder}/{SecondAssetName}");
        }
    }
}