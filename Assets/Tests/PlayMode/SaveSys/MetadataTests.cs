using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Amanita.FSExt;

namespace SaveSystemTests
{
    public class MetadataTests : CommonTestFunctionality
    {
        // Reduce setup: no scene, flowchart, or save system required.
        protected override bool ReqSceneLoad => false;
        protected override bool ReqFlowchart => false;
        protected override bool ReqSaveSystem => false;

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
            Assert.IsFalse(string.IsNullOrEmpty(testMeta.SaveID));
        }

        [Test]
        public virtual void Metadata_AssignsCorrectTimeStampWhenNonePassed()
        {
            // Capture a time window around construction to avoid flaky millisecond differences
            DateTime before = DateTime.UtcNow;
            SaveMetaData testMeta = new SaveMetaData("");
            DateTime after = DateTime.UtcNow;

            Assert.That(
                testMeta.TimeStamp,
                Is.InRange(before, after),
                $"Expected timestamp to be between {before:o} and {after:o}, but was {testMeta.TimeStamp:o}");
        }

        [Test]
        public virtual void Metadata_AcceptsLegitTimeStampPassed()
        {
            DateTime ts = DateTime.UtcNow;
            SaveMetaData testMeta = new SaveMetaData("", ts);
            Assert.AreEqual(ts, testMeta.TimeStamp);

            testMeta = new SaveMetaData("g4w578", ts);
            Assert.AreEqual(ts, testMeta.TimeStamp);
        }

        [Test]
        public virtual void Metadata_AcceptsLegitIDPassed()
        {
            string theID = "esahgui94r35hoifg";
            SaveMetaData testMeta = new SaveMetaData(theID);
            Assert.AreEqual(theID, testMeta.SaveID);
        }

        [Test]
        public virtual void MetadataConsistency_SerializeThenDeserialize_NONEncrypted()
        {
            SaveMetaData metaBefore = new SaveMetaData("egu8hohgb", DateTime.UtcNow);
            string asJson = serializer.ToJson(metaBefore);
            SaveMetaData metaAfter = serializer.FromJson<SaveMetaData>(asJson);
            Assert.AreEqual(metaBefore, metaAfter);
        }

        [Test]
        public virtual void Metadata_HandlesOverlyLongIDs()
        {
            string crazyLongID = string.Empty;
            for (int i = 0; i < 1000; i++)
                crazyLongID += Guid.NewGuid().ToString();

            string expected = crazyLongID[..SaveMetaData.IDAndVersionLengthCap];
            SaveMetaData testMeta = new SaveMetaData(crazyLongID, DateTime.UtcNow);
            Assert.AreEqual(expected, testMeta.SaveID);
        }

        [Test]
        public virtual void Metadata_HandlesOverlyLongSaveVersions()
        {
            string crazyLongVersion = string.Empty;
            for (int i = 0; i < 1000; i++)
                crazyLongVersion += Guid.NewGuid().ToString();

            string expectedVer = crazyLongVersion[..SaveMetaData.IDAndVersionLengthCap];
            SaveMetaData testMeta = new SaveMetaData("", DateTime.UtcNow);
            testMeta.SaveVersion = crazyLongVersion;
            Assert.AreEqual(expectedVer, testMeta.SaveVersion);
        }

        [Test]
        public virtual void Metadata_RejectsNullOrEmptySaveVersions()
        {
            SaveMetaData testMeta = new SaveMetaData(null, DateTime.UtcNow);
            Assert.Throws<ArgumentException>(() => testMeta.SaveVersion = null);
        }

        [Test]
        public virtual void Metadata_RejectsNegativeSlotNumbers()
        {
            SaveMetaData copy = SaveMetaData.CreateFrom(metaData);
            IList<int> negatives = new int[] { -1, -3, -3249, -3459780, -2589 };
            foreach (var n in negatives)
            {
                Assert.Throws<ArgumentException>(() => copy.SlotNumber = n);
            }
        }
    }
}