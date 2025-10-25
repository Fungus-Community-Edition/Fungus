using Amanita.SaveSys;
using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using Amanita.FSExt;

namespace SaveSystemTests
{
    public class MetadataTests : CommonTestFunctionality
    {
        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            PrepMetaData();
        }

        protected virtual void PrepMetaData()
        {
            expectedTypeName = metaData.TypeName;
            expectedSaveVer = "3.32789f";
            expectedTimeStamp = DateTime.UtcNow.ToString("o");

            metaData.SaveVersion = expectedSaveVer;
            metaData.TimeStamp = DateTime.UtcNow;

        }

        protected SaveMetaData deserializedMetaData;
        protected string expectedTypeName, expectedTimeStamp, expectedSaveVer;


        [Test]
        public virtual void Metadata_AssignsOwnIDWhenNonePassed()
        {
            SaveMetaData testMeta = new SaveMetaData("");

            bool hasNoID = string.IsNullOrEmpty(testMeta.SaveID);
            Assert.IsFalse(hasNoID, "Meta did not set itself up with an id upon being given a null or empty one.");
        }

        [Test]
        public virtual void Metadata_AssignsCorrectTimeStampWhenNonePassed()
        {
            DateTime correctTimeStamp = DateTime.UtcNow;
            SaveMetaData testMeta = new SaveMetaData("");

            Assert.AreEqual(correctTimeStamp, testMeta.TimeStamp, "Wrong time stamp assigned");
        }

        [Test]
        public virtual void Metadata_AcceptsLegitTimeStampPassed()
        {
            DateTime correctTimeStamp = DateTime.UtcNow;
            SaveMetaData testMeta = new SaveMetaData("", correctTimeStamp);

            Assert.AreEqual(correctTimeStamp, testMeta.TimeStamp, "Wrong time stamp assigned");

            testMeta = new SaveMetaData("g4w578", correctTimeStamp);
            Assert.AreEqual(correctTimeStamp, testMeta.TimeStamp, "Wrong time stamp assigned after being passed an ID");
        }

        [Test]
        public virtual void Metadata_AcceptsLegitIDPassed()
        {
            string theID = "esahgui94r35hoifg";
            SaveMetaData testMeta = new SaveMetaData(theID);
            Assert.AreEqual(theID, testMeta.SaveID, "Save meta registered the wrong ID");

            DateTime someTimeStamp = DateTime.UtcNow;
            testMeta = new SaveMetaData(theID, someTimeStamp);
            Assert.AreEqual(someTimeStamp, testMeta.TimeStamp, "Wrong time stamp assigned after being passed an ID");
        }

        [Test]
        public virtual void MetadataConsistency_SerializeThenDeserialize_NONEncrypted()
        {
            SaveMetaData metaBefore = new SaveMetaData("egu8hohgb", DateTime.UtcNow);
            string asJson = serializer.ToJson(metaBefore);
            SaveMetaData metaAfter = serializer.FromJson<SaveMetaData>(asJson);

            Assert.AreEqual(metaBefore, metaAfter, "The serialization and deserialization are not complimentary.");
        }

        [Test]
        public virtual void Metadata_HandlesOverlyLongIDs()
        {
            string crazyLongID = string.Empty;

            int howManyLoops = 1000;
            for (int i = 0; i < howManyLoops; i++)
            {
                crazyLongID += System.Guid.NewGuid().ToString();
            }

            string expectedAsEndResult = crazyLongID[..SaveMetaData.IDAndVersionLengthCap];
            SaveMetaData testMeta = new SaveMetaData(crazyLongID, DateTime.UtcNow);
            Assert.AreEqual(expectedAsEndResult, testMeta.SaveID, "Did not enforce id length cap");
            Assert.AreNotEqual(crazyLongID, testMeta.SaveID, "Issue with crazy long id length?");
        }

        [Test]
        public virtual void Metadata_HandlesOverlyLongSaveVersions()
        {
            int howManyLoops = 1000;
            string crazyLongVersion = string.Empty;

            for (int i = 0; i < howManyLoops; i++)
            {
                crazyLongVersion += Guid.NewGuid().ToString();
            }

            string expectedVer = crazyLongVersion[..SaveMetaData.IDAndVersionLengthCap];
            SaveMetaData testMeta = new SaveMetaData("", DateTime.UtcNow);
            testMeta.SaveVersion = crazyLongVersion;

            Assert.AreEqual(expectedVer, testMeta.SaveVersion, "Version char count cap not properly enforced");
            Assert.AreNotEqual(crazyLongVersion, testMeta.SaveVersion, "Crazy long version issue?");
        }

        [Test]
        public virtual void Metadata_RejectsNullOrEmptySaveVersions()
        {
            SaveMetaData testMeta = new SaveMetaData(null, DateTime.UtcNow);
            Assert.Throws<ArgumentException>(() => testMeta.SaveVersion = null, "Did not throw an argument exception");
        }

        [Test] public virtual void Metadata_RejectsNegativeSlotNumbers()
        {
            SaveMetaData toAssignNegativeSlotNumbers = SaveMetaData.CreateFrom(metaData);

            IList<int> negativeNums = new int[] { -1, -3, -3249, -3459780, -2589 };

            foreach (var numEl in negativeNums)
            {
                Assert.Throws<ArgumentException>(() => toAssignNegativeSlotNumbers.SlotNumber = numEl, $"Allowed negative slot number {numEl}");
            }
        }


    }
}