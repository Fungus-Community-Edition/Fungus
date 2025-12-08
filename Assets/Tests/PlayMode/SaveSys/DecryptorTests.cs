using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Encoding = System.Text.Encoding;
using Amanita.FSExt;

namespace SaveSystemTests
{
    public class DecryptorTests : CommonTestFunctionality
    {
        // Skip SaveSystem; we only need in-memory data and our own decryptor.
        protected override bool ReqSaveSystem => false;

        protected Decryptor decryptor;

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            decryptor = ScriptableObject.CreateInstance<Decryptor>();
            toDestroyInTearDown.Add(decryptor);
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
            string metaJson = serializerForTest.ToJson(metaData, true);
            string mainJson = serializerForTest.ToJson(MainSave, true);
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
            string metaJson = serializerForTest.ToJson(metaData, true);
            string mainJson = serializerForTest.ToJson(MainSave, true);
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
            string metaJson = serializerForTest.ToJson(metaData, true);
            string mainJson = serializerForTest.ToJson(MainSave, true);
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
            string mainJson = serializerForTest.ToJson(MainSave, true);
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
            // Add a simple concrete SaveData carrying the unicode string
            MainSave.Add(new RawStringSaveData(testVal));

            string metaJson = serializerForTest.ToJson(metaData, true);
            string mainJson = serializerForTest.ToJson(MainSave, true);
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
            bool hasTheValue = decryptedMain.Items.OfType<RawStringSaveData>().Any(elem => elem.Value == testVal);
            Assert.IsTrue(hasTheValue, $"Unicode data '{testVal}' not present after decryption.");
        }

        public static IEnumerable<string> UnicodeTestCases()
        {
            yield return "こんにちは世界🌏 Привет мир 𝄞";
            yield return "你好，世界";
            yield return "안녕하세요 세계";
            yield return "مرحبا بالعالم";
            yield return "שלום עולם";
            yield return "😀😃😄😁😆😅😂🤣";
            yield return "Café naïve façade coöperate";
            yield return "𝔘𝔫𝔦𝔠𝔬𝔡𝔢 𝕋𝕖𝕤𝕥";
            yield return "हैलो वर्ल्ड";
            yield return "Zażółć gęślą jaźń";
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

            // Add the VariableSaveData directly to the composite (no SaveDataUnit)
            MainSave.Add(variableSaveData);

            string metaJson = serializerForTest.ToJson(metaData, true);
            string mainJson = serializerForTest.ToJson(MainSave, true);
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

            var composite = result.MainState as CompositeSaveData;
            Assert.IsNotNull(composite, "Main state is not CompositeSaveData after decryption.");

            var varData = composite.Items.OfType<VariableSaveData>().FirstOrDefault();
            Assert.IsNotNull(varData, "VariableSaveData not found in main state.");
            Assert.AreEqual(testValue, varData.Value);
        }

        [Test]
        public void DecryptsConsistentlyForSameInput()
        {
            // Arrange
            string metaJson = serializerForTest.ToJson(metaData, true);
            string mainJson = serializerForTest.ToJson(MainSave, true);
            string fullJson = $"{metaJson}{SaveDiskAccessor.ReadWriteDelimiter}{mainJson}{SaveDiskAccessor.CompletionMarker}";
            byte[] encrypted = Encrypt(fullJson);

            var req = new BaseDecryptionRequest
            {
                RawBytes = encrypted,
                WrittenAsPlainText = false
            };

            // Act
            var firstResult = decryptor.DecryptWholeSet(req);
            var secondResult = decryptor.DecryptWholeSet(req);

            // Assert
            Assert.AreEqual(serializerForTest.ToJson(firstResult.Meta), serializerForTest.ToJson(secondResult.Meta), "Meta data mismatch between decryptions.");

            string r1Main = serializerForTest.ToJson(firstResult.MainState, true);
            string r2Main = serializerForTest.ToJson(secondResult.MainState, true);
            Assert.AreEqual(r1Main, r2Main, "Main state mismatch between decryptions.");
        }

        [Test]
        public void DecryptorIsThreadSafeForParallelCalls()
        {
            // Arrange
            string metaJson = serializerForTest.ToJson(metaData, true);
            string mainJson = serializerForTest.ToJson(MainSave, true);
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
                Assert.AreEqual(serializerForTest.ToJson(results[0].Meta),
                    serializerForTest.ToJson(results[i].Meta),
                    $"Meta data mismatch between threads {0} and {i}.");

                string r0Main = serializerForTest.ToJson(results[0].MainState, true);
                string riMain = serializerForTest.ToJson(results[i].MainState, true);
                Assert.AreEqual(r0Main, riMain, $"Main state mismatch between threads {0} and {i}.");
            }
        }
    }

    
}