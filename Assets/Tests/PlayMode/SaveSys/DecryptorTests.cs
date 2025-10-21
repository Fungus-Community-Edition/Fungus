using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Encoding = System.Text.Encoding;

namespace SaveSystemTests
{
    public class DecryptorTests : CommonTestFunctionality
    {
        protected Decryptor decryptor;

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            decryptor = ScriptableObject.CreateInstance<Decryptor>();
        }

        protected byte[] Encrypt(string plainText)
        {
            byte key = 0xAA;
            return utf8.GetBytes(plainText).Select(b => (byte)(b ^ key)).ToArray();
        }

        protected Encoding utf8 = Encoding.UTF8;

        [Test]
        public void DecryptsMetaDataCorrectly()
        {
            // Arrange
            string metaJson = JsonUtility.ToJson(metaData, true);
            string mainJson = JsonUtility.ToJson(MainSave, true);
            string fullJson = $"{metaJson}{SaveDiskAccessor.ReadWriteDelimiter}{mainJson}{SaveDiskAccessor.CompletionMarker}";
            byte[] encrypted = Encrypt(fullJson);

            var req = new BaseDecryptionRequest
            {
                RawBytes = encrypted,
                WrittenAsPlainText = false
            };

            // Act
            ISaveMetaData result = decryptor.DecryptMeta(req);

            // Assert
            Assert.IsNotNull(result, "Decrypted meta data is null.");
            Assert.AreEqual(metaData.SaveVersion, result.SaveVersion, "Meta data SaveVersion mismatch.");
        }

        [Test]
        public void DecryptsMainStateCorrectly()
        {
            // Arrange
            string metaJson = JsonUtility.ToJson(metaData, true);
            string mainJson = JsonUtility.ToJson(MainSave, true);
            string fullJson = $"{metaJson}{SaveDiskAccessor.ReadWriteDelimiter}{mainJson}{SaveDiskAccessor.CompletionMarker}";
            byte[] encrypted = Encrypt(fullJson);

            var req = new BaseDecryptionRequest
            {
                RawBytes = encrypted,
                WrittenAsPlainText = false
            };

            // Act
            ISaveData result = decryptor.DecryptMainState(req);

            // Assert
            Assert.IsNotNull(result, "Decrypted main state is null.");
            Assert.AreEqual(MainSave.TypeName, result.TypeName, "Main state type name mismatch.");
        }

        [Test]
        public void DecryptsWholeSetCorrectly()
        {
            // Arrange
            string metaJson = JsonUtility.ToJson(metaData, true);
            string mainJson = JsonUtility.ToJson(MainSave, true);
            string fullJson = $"{metaJson}{SaveDiskAccessor.ReadWriteDelimiter}{mainJson}{SaveDiskAccessor.CompletionMarker}";
            byte[] encrypted = Encrypt(fullJson);

            var req = new BaseDecryptionRequest
            {
                RawBytes = encrypted,
                WrittenAsPlainText = false
            };

            // Act
            ISaveDataSet result = decryptor.DecryptWholeSet(req);

            // Assert
            Assert.IsNotNull(result, "Decrypted whole set is null.");
            Assert.AreEqual(metaData.SaveVersion, result.Meta.SaveVersion, "Meta SaveVersion mismatch.");
            Assert.AreEqual(MainSave.TypeName, result.MainState.TypeName, "Main state type name mismatch.");
        }

        [Test]
        public void RejectsNullInput()
        {
            Assert.Throws<NullReferenceException>(() => decryptor.DecryptMeta(null), "Did not reject null input.");
            Assert.Throws<NullReferenceException>(() => decryptor.DecryptMainState(null), "Did not reject null input.");
            Assert.Throws<NullReferenceException>(() => decryptor.DecryptWholeSet(null), "Did not reject null input.");
        }

        [Test]
        public void RejectsNonDecryptionRequestInput()
        {
            Assert.Throws<ArgumentException>(() => decryptor.DecryptMeta("not a request"), "Did not reject non-request input.");
            Assert.Throws<ArgumentException>(() => decryptor.DecryptMainState(123), "Did not reject non-request input.");
            Assert.Throws<ArgumentException>(() => decryptor.DecryptWholeSet(new object()), "Did not reject non-request input.");
        }

        [Test]
        public void RejectsCorruptedOrIncompleteData()
        {
            // Arrange: missing completion marker
            string metaJson = JsonUtility.ToJson(metaData, true);
            string mainJson = JsonUtility.ToJson(MainSave, true);
            string fullJson = $"{metaJson}{SaveDiskAccessor.ReadWriteDelimiter}{mainJson}";
            byte[] encrypted = Encrypt(fullJson);

            var req = new BaseDecryptionRequest
            {
                RawBytes = encrypted,
                WrittenAsPlainText = false
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => decryptor.DecryptMeta(req), "Did not reject data missing completion marker.");
        }

        [Test, TestCaseSource(nameof(UnicodeTestCases))]
        public void DecryptsUnicodeDataCorrectly(string testVal)
        {
            SaveDataUnit testUnit = new SaveDataUnit("testType", testVal);
            MainSave.Add(testUnit);
            string metaJson = JsonUtility.ToJson(metaData, true);
            string mainJson = JsonUtility.ToJson(MainSave, true);
            string fullJson = $"{metaJson}{SaveDiskAccessor.ReadWriteDelimiter}{mainJson}{SaveDiskAccessor.CompletionMarker}";
            byte[] encrypted = Encrypt(fullJson);

            var req = new BaseDecryptionRequest
            {
                RawBytes = encrypted,
                WrittenAsPlainText = false
            };

            ISaveDataSet result = decryptor.DecryptWholeSet(req);
            Assert.That(result.MainState is CompositeSaveData, "Main state is not encoded as CompositeSaveData");

            var decryptedMain = result.MainState as CompositeSaveData;
            var decryptedUnits = decryptedMain.Units;

            bool hasTheValue = decryptedUnits.Any(elem => elem.Equals(testUnit));
            Assert.IsTrue(hasTheValue, $"Unicode data '{testVal}' not present after decryption.");
        }


        public static IEnumerable<string> UnicodeTestCases()
        {
            yield return "こんにちは世界🌏 Привет мир 𝄞"; // Japanese, Russian, emoji, music symbol
            yield return "你好，世界"; // Chinese
            yield return "안녕하세요 세계"; // Korean
            yield return "مرحبا بالعالم"; // Arabic
            yield return "שלום עולם"; // Hebrew
            yield return "😀😃😄😁😆😅😂🤣"; // Emoji sequence
            yield return "Café naïve façade coöperate"; // Accented Latin characters
            yield return "𝔘𝔫𝔦𝔠𝔬𝔡𝔢 𝕋𝕖𝕤𝕥"; // Mathematical/Fraktur/Double-struck
            yield return "हैलो वर्ल्ड"; // Hindi
            yield return "Zażółć gęślą jaźń"; // Polish diacritics
        }

        [Test]
        public void DecryptsDataContainingDelimiterAndMarker()
        {
            // Arrange
            string delimiter = SaveDiskAccessor.ReadWriteDelimiter;
            string marker = SaveDiskAccessor.CompletionMarker;
            string testValue = $"Value with delimiter: {delimiter} and marker: {marker}";

            stringVar.Value = testValue;
            StringVarCodec stringVarCodec = new StringVarCodec();
            VariableSaveData variableSaveData = stringVarCodec.EncodeToSave(stringVar);
            SaveDataUnit newUnit = variableSaveData.Serialized();
            MainSave.Add(newUnit);

            string metaJson = JsonUtility.ToJson(metaData, true);
            string mainJson = JsonUtility.ToJson(MainSave, true);
            string fullJson = $"{metaJson}{delimiter}{mainJson}{marker}";
            byte[] encrypted = Encrypt(fullJson);

            var req = new BaseDecryptionRequest
            {
                RawBytes = encrypted,
                WrittenAsPlainText = false
            };

            // Act
            ISaveDataSet result = decryptor.DecryptWholeSet(req);

            // Assert
            Assert.IsNotNull(result, "Decrypted whole set is null.");

            // Suppose you know the structure and can get the SaveDataUnit or variable
            var composite = result.MainState as CompositeSaveData;
            var unit = composite.Units.FirstOrDefault(unitEl => unitEl.DataTypeName == nameof(VariableSaveData));

            if (unit != null)
            {
                // If the value is stored as JSON, you may need to deserialize again
                VariableSaveData variableData = JsonUtility.FromJson<VariableSaveData>(unit.Content);
                Assert.AreEqual(testValue, variableData.Value);
            }
            else
            {
                Assert.Fail("VariableSaveData unit with delimiter and marker not found in main state.");
            }

        }

        [Test]
        public void DecryptsConsistentlyForSameInput()
        {
            // Arrange
            string metaJson = JsonUtility.ToJson(metaData, true);
            string mainJson = JsonUtility.ToJson(MainSave, true);
            string fullJson = $"{metaJson}{SaveDiskAccessor.ReadWriteDelimiter}{mainJson}{SaveDiskAccessor.CompletionMarker}";
            byte[] encrypted = Encrypt(fullJson);

            var req = new BaseDecryptionRequest
            {
                RawBytes = encrypted,
                WrittenAsPlainText = false
            };

            // Act
            var result1 = decryptor.DecryptWholeSet(req);
            var result2 = decryptor.DecryptWholeSet(req);

            // Assert
            Assert.AreEqual(JsonUtility.ToJson(result1.Meta), JsonUtility.ToJson(result2.Meta), "Meta data mismatch between decryptions.");
            Assert.AreEqual(JsonUtility.ToJson(result1.MainState), JsonUtility.ToJson(result2.MainState), "Main state mismatch between decryptions.");
        }

        [Test]
        public void DecryptorIsThreadSafeForParallelCalls()
        {
            // Arrange
            string metaJson = JsonUtility.ToJson(metaData, true);
            string mainJson = JsonUtility.ToJson(MainSave, true);
            string fullJson = $"{metaJson}{SaveDiskAccessor.ReadWriteDelimiter}{mainJson}{SaveDiskAccessor.CompletionMarker}";
            byte[] encrypted = Encrypt(fullJson);

            var req = new BaseDecryptionRequest
            {
                RawBytes = encrypted,
                WrittenAsPlainText = false
            };

            const int threadCount = 8;
            ISaveDataSet[] results = new ISaveDataSet[threadCount];
            Exception threadException = null;

            // Act
            System.Threading.Tasks.Parallel.For(0, threadCount, i =>
            {
                try
                {
                    results[i] = decryptor.DecryptWholeSet(req);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            // Assert
            Assert.IsNull(threadException, "Decryptor threw an exception during parallel calls.");
            for (int i = 1; i < threadCount; i++)
            {
                Assert.AreEqual(JsonUtility.ToJson(results[0].Meta), JsonUtility.ToJson(results[i].Meta), $"Meta data mismatch between threads {0} and {i}.");
                Assert.AreEqual(JsonUtility.ToJson(results[0].MainState), JsonUtility.ToJson(results[i].MainState), $"Main state mismatch between threads {0} and {i}.");
            }
        }
    }
}