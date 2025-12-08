using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Encoding = System.Text.Encoding;
using Amanita.FSExt;
using FullSerializer;
using Amanita;

namespace SaveSystemTests
{
    public class EncryptorTests : CommonTestFunctionality
    {
        protected override bool ReqSaveSystem => false;

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();

            // Since we're skipping the save sys, we need to instantiate our own encryptor.
            if (encryptor == null)
            {
                encryptor = ScriptableObject.CreateInstance<Encryptor>();
                toDestroyInTearDown.Add(encryptor);
            }
        }

        [Test]
        public virtual void ReturnsExpectedBytes()
        {
            try
            {
                string expectedMetaDataJson = serializerForTest.ToJson(saveDataSet.Meta, true);
                string expectedMainSaveDataJson = serializerForTest.ToJson(saveDataSet.MainState, true);

                string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}{SaveDiskAccessor.CompletionMarker}";

                byte key = 0xAA;
                IList<byte> expectedBytes = utf8.GetBytes(expectedJsonText)
                    .Select(b => (byte)(b ^ key))
                    .ToArray(); // Simple XOR encryption for testing

                BaseEncryptionRequest encryptionRequest = new BaseEncryptionRequest()
                {
                    SaveDataSet = saveDataSet,
                    CompletionMarker = SaveDiskAccessor.CompletionMarker
                };
                object output = encryptor.GetOutput(encryptionRequest);
                byte[] bytesWeGot = (byte[])output;

                bool success = expectedBytes.SequenceEqual(bytesWeGot);
                Assert.IsTrue(success, "Got the wrong set of encrypted bytes.");
            }
            catch (System.Exception ex)
            {
                Debug.Log("Caught exception: " + ex);
                throw;

            }
        }

        protected Encoding utf8 = Encoding.UTF8;

        [Test]
        public virtual void RejectsNullInput()
        {
            Assert.Throws<System.NullReferenceException>(() => { encryptor.GetOutput(null); },
                "Does not reject null input.");
        }

        [Test]
        public virtual void RejectsNonSaveDataSetInput()
        {
            Assert.Throws<System.ArgumentException>(() => { encryptor.GetOutput(testScene); },
                $"Accepted a scene as input when it shouldn't.");
            Assert.Throws<System.ArgumentException>(() => { encryptor.GetOutput(flowchartApplier); },
                "Accepted a FlowchartApplier when it shouldn't.");
            Assert.Throws<System.ArgumentException>(() => { encryptor.GetOutput(encryptor); },
                "Accepted itself when it shouldn't.");
            Assert.Throws<System.ArgumentException>(() => { encryptor.GetOutput(saveDataSet.Meta); },
                "Accepted the metadata itself when it should've been in another container.");
            Assert.Throws<System.ArgumentException>(() => { encryptor.GetOutput(saveDataSet.MainState); },
                "Accepted the main state itself when it should've been in another container.");
        }

        [Test]
        public void RejectsNullSaveDataSet()
        {
            var req = new BaseEncryptionRequest { SaveDataSet = null, CompletionMarker = "X" };
            Assert.Throws<ArgumentNullException>(() => encryptor.GetOutput(req));
        }

        [Test]
        public void RejectsNullMainState()
        {
            var dataSet = new SaveDataSet(metaData, null);
            var req = new BaseEncryptionRequest { SaveDataSet = dataSet, CompletionMarker = "X" };
            Assert.Throws<NullReferenceException>(() => encryptor.GetOutput(req));
        }

        [Test]
        public void RejectsNullOrEmptyCompletionMarker()
        {
            var req1 = new BaseEncryptionRequest { SaveDataSet = saveDataSet, CompletionMarker = null };
            var req2 = new BaseEncryptionRequest { SaveDataSet = saveDataSet, CompletionMarker = "" };
            Assert.Throws<NullReferenceException>(() => encryptor.GetOutput(req1));
            Assert.Throws<NullReferenceException>(() => encryptor.GetOutput(req2));
        }

        [Test]
        public void EncryptsLargeDataEfficiently()
        {
            // Arrange: create a large string for main save data
            string largeString = new string('A', 10_000_000); // 10 MB of 'A'
            MainSave.Add(new RawStringSaveData(largeString));

            string expectedMetaDataJson = serializerForTest.ToJson(saveDataSet.Meta, true);
            string expectedMainSaveDataJson = serializerForTest.ToJson(saveDataSet.MainState, true);
            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}{SaveDiskAccessor.CompletionMarker}";
            byte key = 0xAA;
            IList<byte> expectedBytes = utf8.GetBytes(expectedJsonText)
                .Select(b => (byte)(b ^ key))
                .ToArray();

            var encryptionRequest = new BaseEncryptionRequest
            {
                SaveDataSet = saveDataSet,
                CompletionMarker = SaveDiskAccessor.CompletionMarker
            };

            // Act & Assert: ensure it completes in a reasonable time and is correct
            var sw = System.Diagnostics.Stopwatch.StartNew();
            object output = encryptor.GetOutput(encryptionRequest);
            sw.Stop();
            byte[] bytesWeGot = (byte[])output;
            Assert.IsTrue(expectedBytes.SequenceEqual(bytesWeGot), "Large data encryption output mismatch.");
            Assert.Less(sw.Elapsed.TotalSeconds, 5, "Encryption took too long for large data.");
        }

        [Test]
        public void EncryptsUnicodeDataCorrectly()
        {
            // Arrange: add Unicode characters to a string variable
            stringVar.Value = "こんにちは世界🌏 Привет мир 𝄞";
            string expectedMetaDataJson = serializerForTest.ToJson(saveDataSet.Meta, true);
            string expectedMainSaveDataJson = serializerForTest.ToJson(saveDataSet.MainState, true);
            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}{SaveDiskAccessor.CompletionMarker}";
            byte key = 0xAA;
            IList<byte> expectedBytes = utf8.GetBytes(expectedJsonText)
                .Select(b => (byte)(b ^ key))
                .ToArray();

            var encryptionRequest = new BaseEncryptionRequest
            {
                SaveDataSet = saveDataSet,
                CompletionMarker = SaveDiskAccessor.CompletionMarker
            };

            // Act
            object output = encryptor.GetOutput(encryptionRequest);
            byte[] bytesWeGot = (byte[])output;

            // Assert
            Assert.IsTrue(expectedBytes.SequenceEqual(bytesWeGot), "Unicode data was not encrypted as expected.");
        }

        [Test]
        public void EncryptsConsistentlyForSameInput()
        {
            // Arrange
            var encryptionRequest = new BaseEncryptionRequest
            {
                SaveDataSet = saveDataSet,
                CompletionMarker = SaveDiskAccessor.CompletionMarker
            };

            // Act
            object output1 = encryptor.GetOutput(encryptionRequest);
            object output2 = encryptor.GetOutput(encryptionRequest);

            // Assert
            Assert.IsTrue(((byte[])output1).SequenceEqual((byte[])output2), "Encryptor did not produce consistent output for the same input.");
        }

        [Test]
        public void EncryptsDataContainingDelimiterAndMarker()
        {
            // Arrange: inject delimiter and completion marker into the data
            string delimiter = SaveDiskAccessor.ReadWriteDelimiter;
            string marker = SaveDiskAccessor.CompletionMarker;
            stringVar.Value = $"Value with delimiter: {delimiter} and marker: {marker}";
            
            IList<byte> expectedBytes;
            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                string expectedMetaDataJson = serializer.ToJson(metaData as ISaveMetaData, true);
                string expectedMainSaveDataJson = serializer.ToJson(MainSave as ISaveData, true);
                // ^Need to cast here so that the serialized json here and in the encryptor match.
                // Turns out that when fsSerializer serializes an interface type, it includes type metadata,
                // and not when passed a concrete type.
                string expectedJsonText = $"{expectedMetaDataJson}{delimiter}{expectedMainSaveDataJson}{marker}";
                byte key = 0xAA;
                expectedBytes = utf8.GetBytes(expectedJsonText)
                    .Select(b => (byte)(b ^ key))
                    .ToArray();
            }

            var encryptionRequest = new BaseEncryptionRequest
            {
                SaveDataSet = saveDataSet,
                CompletionMarker = marker
            };

            // Act
            object output = encryptor.GetOutput(encryptionRequest);
            byte[] bytesWeGot = (byte[])output;

            // Assert
            Assert.IsTrue(expectedBytes.SequenceEqual(bytesWeGot),
                "Encryptor did not handle delimiter/marker in data as expected.");
        }

        [Test]
        public void EncryptsEmptyStringVariable()
        {
            // Arrange: set string variable to empty
            stringVar.Value = "";
            
            string expectedMetaDataJson = serializerForTest.ToJson(saveDataSet.Meta, true);
            string expectedMainSaveDataJson = serializerForTest.ToJson(saveDataSet.MainState, true);
            // ^Remember: whether or not the serializer includes the $type depends on whether it's
            // passed the exact concrete type to serialize. Parent classes are treated the same
            // as interfaces when it comes to deciding whether or not to include $type.

            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}{SaveDiskAccessor.CompletionMarker}";
            byte key = 0xAA;
            IList<byte> expectedBytes = utf8.GetBytes(expectedJsonText)
                .Select(b => (byte)(b ^ key))
                .ToArray();

            var encryptionRequest = new BaseEncryptionRequest
            {
                SaveDataSet = saveDataSet,
                CompletionMarker = SaveDiskAccessor.CompletionMarker
            };

            // Act
            object output = encryptor.GetOutput(encryptionRequest);
            byte[] bytesWeGot = (byte[])output;

            // Assert
            Assert.IsTrue(expectedBytes.SequenceEqual(bytesWeGot), "Encryptor did not handle empty string variable as expected.");
        }

        [Test]
        public void EncryptsWhitespaceOnlyStringVariable()
        {
            // Arrange: set string variable to whitespace
            stringVar.Value = "   \t\n";
            string expectedMetaDataJson = serializerForTest.ToJson(saveDataSet.Meta, true);
            string expectedMainSaveDataJson = serializerForTest.ToJson(saveDataSet.MainState, true);
            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}{SaveDiskAccessor.CompletionMarker}";
            byte key = 0xAA;
            IList<byte> expectedBytes = utf8.GetBytes(expectedJsonText)
                .Select(b => (byte)(b ^ key))
                .ToArray();

            var encryptionRequest = new BaseEncryptionRequest
            {
                SaveDataSet = saveDataSet,
                CompletionMarker = SaveDiskAccessor.CompletionMarker
            };

            // Act
            object output = encryptor.GetOutput(encryptionRequest);
            byte[] bytesWeGot = (byte[])output;

            // Assert
            Assert.IsTrue(expectedBytes.SequenceEqual(bytesWeGot), "Encryptor did not handle whitespace-only string variable as expected.");
        }

        [Test]
        public void EncryptsDifferentDataDifferently()
        {
            // Arrange
            var encryptionRequest = new BaseEncryptionRequest
            {
                SaveDataSet = saveDataSet,
                CompletionMarker = SaveDiskAccessor.CompletionMarker
            };

            // Get output for the current state
            object output1 = encryptor.GetOutput(encryptionRequest);

            // Mutate the main save data: add a new SaveData item
            MainSave.Add(new RawStringSaveData("Some new data"));

            // (Optional) Mutate metaData as well
            metaData.SaveVersion = Guid.NewGuid().ToString();

            // Re-create the SaveDataSet to ensure it picks up the changed MainSave
            saveDataSet = new SaveDataSet(metaData, MainSave);

            // Create a new request with the updated SaveDataSet
            var encryptionRequest2 = new BaseEncryptionRequest
            {
                SaveDataSet = saveDataSet,
                CompletionMarker = SaveDiskAccessor.CompletionMarker
            };

            // Get output for the mutated state
            object output2 = encryptor.GetOutput(encryptionRequest2);

            // Assert
            Assert.IsFalse(((byte[])output1).SequenceEqual((byte[])output2), "Encryptor produced the same output for different data.");
        }

        [Test]
        public void EncryptorIsThreadSafeForParallelCalls()
        {
            // Arrange
            var encryptionRequest = new BaseEncryptionRequest
            {
                SaveDataSet = saveDataSet,
                CompletionMarker = SaveDiskAccessor.CompletionMarker
            };
            const int threadCount = 8;
            byte[][] results = new byte[threadCount][];
            Exception threadException = null;

            // Act
            System.Threading.Tasks.Parallel.For(0, threadCount, i =>
            {
                try
                {
                    object output = encryptor.GetOutput(encryptionRequest);
                    results[i] = (byte[])output;
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            // Assert
            Assert.IsNull(threadException, "Encryptor threw an exception during parallel calls.");
            for (int i = 1; i < threadCount; i++)
            {
                Assert.IsTrue(results[0].SequenceEqual(results[i]), $"Encryptor output mismatch between threads {0} and {i}.");
            }
        }
    }
}