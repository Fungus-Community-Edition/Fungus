using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections.Generic;

namespace SaveSystemTests
{
    public class FakeVersionProvider : IVersionProvider
    {
        public string VersionToReturn { get; set; }
        public string GetVersion() => VersionToReturn;
    }

    public class MetaFactoryTests : CommonTestFunctionality
    {
        protected override bool ReqFlowchart => false;
        protected override bool ReqSceneLoad => false;
        protected override bool ReqSaveSystem => true;

        protected FakeVersionProvider VersionProvider { get; set; }
        protected IMetaFactory Factory { get; set; }

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            VersionProvider = new FakeVersionProvider();
            Factory = new DefaultMetaFactory(VersionProvider);
        }

        [Test]
        [TestCaseSource(nameof(SlotsToTestWith))]
        public void CreateMeta_AssignsSlotNumber(int expectedSlot)
        {
            var meta = Factory.CreateMeta(expectedSlot);
            Assert.AreEqual(expectedSlot, meta.SlotNumber);
        }

        public static IEnumerable<int> SlotsToTestWith()
        {
            yield return 1;
            yield return 7;
            yield return 12;
        }

        [Test]
        [TestCaseSource(nameof(VersionStringsToTestWith))]
        public void CreateMeta_SetsSaveVersion_WhenApplicationVersionIsNotEmpty(string appVersion)
        {
            VersionProvider.VersionToReturn = appVersion;
            var meta = Factory.CreateMeta(slotNumber: 1);
            Assert.AreEqual(appVersion, meta.SaveVersion);
        }

        public static IEnumerable<string> VersionStringsToTestWith()
        {
            yield return "1.0.0";
            yield return "1.2.3";
            yield return "4.5.6";
        }

        [Test]
        public void CreateMeta_HasNullSaveVersion_WhenApplicationVersionIsEmpty()
        {
            VersionProvider.VersionToReturn = string.Empty;
            var meta = Factory.CreateMeta(slotNumber: 1);
            bool success = meta.SaveVersion == SaveSysConstants.NullSaveVer;
            Assert.IsTrue(success, "Meta save version is not null when app version is empty");
        }

        [Test]
        public void CreateMeta_HasNullSaveVersion_WhenApplicationVersionIsNullConstant()
        {
            VersionProvider.VersionToReturn = SaveSysConstants.NullSaveVer;
            var meta = Factory.CreateMeta(slotNumber: 1);
            bool success = meta.SaveVersion == SaveSysConstants.NullSaveVer;
            Assert.IsTrue(success, "Meta save version is not null when app version is empty");
        }

        [Test]
        public void CreateMeta_ReturnsDistinctInstances()
        {
            var first = Factory.CreateMeta(1);
            var second = Factory.CreateMeta(1);
            Assert.AreNotSame(first, second);
        }
    }
}