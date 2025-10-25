using Amanita.SaveSys;
using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;

namespace SaveSystemTests
{

    public class VsaSaveCodecTests : CommonTestFunctionality
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


        [Test]
        public void RecordsAssetIds()
        {
            // Act
            var firstSaveData = _saveCodec.EncodeToSave(firstVsa);
            var secondSaveData = _saveCodec.EncodeToSave(secondVsa);

            bool recordsAssetID = firstSaveData.AssetId == firstVsa.AssetId &&
                secondSaveData.AssetId == secondVsa.AssetId;
            Assert.IsTrue(recordsAssetID,
                "Encoded VariableSourceAssetSaveData should record the AssetId of the VariableSourceAsset it was created from.");

        }

        [Test]
        public void RecordsKeysOfEachVar()
        {
            // Act
            var firstSaveData = _saveCodec.EncodeToSave(firstVsa);
            var secondSaveData = _saveCodec.EncodeToSave(secondVsa);
            var firstKeys = firstVsa.Variables.Select(v => v.Key).ToList();
            var secondKeys = secondVsa.Variables.Select(v => v.Key).ToList();

            var firstSavedVars = firstSaveData.SavedVars;
            IList<string> firstEncodedKeys = firstSavedVars.Select(vs => vs.Key).ToList();
            IList<string> secondEncodedKeys = secondSaveData.SavedVars.Select(vs => vs.Key).ToList();

            bool success = firstKeys.SequenceEqual(firstEncodedKeys) &&
                secondKeys.SequenceEqual(secondEncodedKeys);
            Assert.IsTrue(success, "The keys of each variable should be recorded in the VariableSourceAssetSaveData.");
        }

        [Test]
        public void RecordsItemIdsOfEachVar()
        {
            // Act
            var firstSaveData = _saveCodec.EncodeToSave(firstVsa);
            var secondSaveData = _saveCodec.EncodeToSave(secondVsa);

            var firstSavedVars = firstSaveData.SavedVars;
            var secondSavedVars = secondSaveData.SavedVars;

            IList<int> firstEncodedItemIds = firstSavedVars.Select(vs => vs.ItemId).ToList();
            IList<int> secondEncodedItemIds = secondSavedVars.Select(vs => vs.ItemId).ToList();

            var firstVarItemIds = firstVsa.Variables.Select(v => v.ItemId).ToList();
            var secondVarItemIds = secondVsa.Variables.Select(v => v.ItemId).ToList();

            bool success = firstVarItemIds.SequenceEqual(firstEncodedItemIds) &&
                secondVarItemIds.SequenceEqual(secondEncodedItemIds);
            Assert.IsTrue(success, "The ItemIds of each variable should be recorded in the VariableSourceAssetSaveData.");
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
        public IEnumerator Respects_IncludeInSaves_False()
        {
            firstVsa.IncludeInSaves = false;

            var saveData = _saveCodec.EncodeToSave(firstVsa);

            Assert.IsNull(saveData, "Save data should be null for VariableSourceAsset with IncludeInSaves = false");

            firstVsa.IncludeInSaves = true; // For other tests
            yield return null;
        }
    }
}