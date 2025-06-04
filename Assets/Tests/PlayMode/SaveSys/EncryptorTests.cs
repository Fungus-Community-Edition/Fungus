using Amanita.Myceliaudio;
using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Encoding = System.Text.Encoding;
using UnityObject = UnityEngine.Object;

namespace Amanita.SaveSystemTests
{
    public class EncryptorTests : CommonTestFunctionality
    {

        [Test]
        public virtual void ReturnsExpectedBytes()
        {
            try
            {
                string expectedMetaDataJson = JsonUtility.ToJson(metaData, true);
                string expectedMainSaveDataJson = JsonUtility.ToJson(MainSave, true);

                string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}";

                byte key = 0xAA;
                IList<byte> expectedBytes = utf8.GetBytes(expectedJsonText)
                    .Select(b => (byte)(b ^ key))
                    .ToArray(); // Simple XOR encryption for testing

                object output = encryptor.GetOutput(saveDataSet);
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
    }
}