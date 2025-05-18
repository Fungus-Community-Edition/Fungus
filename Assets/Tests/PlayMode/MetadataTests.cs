using NUnit.Framework;
using UnityEngine;
using Amanita.SaveSys;
using System;

namespace Amanita.SaveSystemTests
{
    public class MetadataTests
    {
        [SetUp]
        public virtual void DoSetUp()
        {
            PrepMetaData();
        }

        protected virtual void PrepMetaData()
        {
            expectedTypeName = metaData.TypeName;
            expectedSaveVer = 3.32789f;
            expectedTimeStamp = DateTime.UtcNow.ToString("o");

            metaData.SaveVersion = expectedSaveVer;
            metaData.UTCTimeStamp = expectedTimeStamp;

            serializedMetaData = metaData.Serialized();
            deserializedMetaData = SaveMetaData.DeserializeFrom(serializedMetaData);
        }

        SaveMetaData metaData = new SaveMetaData();
        protected SerializedSaveData serializedMetaData;
        protected SaveMetaData deserializedMetaData;
        protected string expectedTypeName, expectedTimeStamp;
        float expectedSaveVer;

        [TearDown]
        public virtual void DoTearDown()
        {

        }

        [Test]
        public virtual void Metadata_TypeNameSerializedProperly()
        {
            Assert.AreEqual(serializedMetaData.DataTypeName, expectedTypeName);
        }

        [Test]
        public virtual void Metadata_MainFieldsSerializedProperly()
        {
            Debug.Log($"Checking if the main metadata fields were serialized properly.");
            bool success = metaData.Equals(deserializedMetaData);
            Assert.IsTrue(success);
        }


    }
}