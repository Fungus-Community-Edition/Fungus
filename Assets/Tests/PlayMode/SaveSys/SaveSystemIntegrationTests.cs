using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amanita.VScripting;

namespace Amanita.SaveSystemTests
{
    public class SaveSystemIntegrationTests : CommonTestFunctionality
    {
        protected override string PathToTestScene => "ScenePrefabs/SaveSysMonoBehaviourTests";
        protected SaveSystem saveSystem;
        protected new ISaveManager saveManager;
        protected IMetaFactory metaFactory;
        protected IMainStateFactory mainStateFactory;

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            // Assume AmanitaManager and SaveSystem are set up in CommonTestFunctionality
            saveSystem = SaveSystem.S;
            saveManager = saveSystem.SaveManager;
            metaFactory = saveSystem.MetaFactory;
            mainStateFactory = saveSystem.MainStateFactory;
        }

        [Test]
        public async Task SaveAndLoad_Flowchart_Main_RoundTrip_Works()
        {
            await CommonSetupAsync();

            // Arrange: get the Flowchart from the test scene (set up by CommonTestFunctionality)
            var flowchart = testScene.GetComponentInChildren<Flowchart>();
            Assert.IsNotNull(flowchart, "Test scene does not contain a Flowchart.");

            int slot = 10;
            ISaveMetaData meta = metaFactory.CreateMeta(slot); 

            // Use the main state factory to create the main state (should include FlowchartSaveData)
            CompositeSaveData mainState = await mainStateFactory.CreateMainState();

            // Save
            var saveDataSet = new SaveDataSet(meta, mainState);
            await saveSystem.SaveTo(slot);

            // Load
            var loadedMain = await saveSystem.LoadMain(slot, loadScene: false);

            // Assert: loadedMain should be a CompositeSaveData and contain FlowchartSaveData
            Assert.IsInstanceOf<CompositeSaveData>(loadedMain, "Loaded main state is not CompositeSaveData.");
            var loadedComposite = loadedMain as CompositeSaveData;
            var flowchartUnit = loadedComposite.Units.FirstOrDefault(u => u.DataTypeName == nameof(FlowchartSaveData));
            Assert.IsNotNull(flowchartUnit, "Loaded main state does not contain FlowchartSaveData unit.");
            bool loadedExpectedMainState = mainState.Equals(loadedMain);
            Assert.IsTrue(loadedExpectedMainState, "Main state was not loaded correctly");
        }

        [Test]
        public async Task SaveAndLoad_Flowchart_Variable_RoundTrip_Works()
        {
            await CommonSetupAsync();

            // Arrange
            var flowchart = testScene.GetComponentInChildren<Flowchart>();
            Assert.IsNotNull(flowchart, "Test scene does not contain a Flowchart.");

            // Set a known variable value
            // stringVar was already fetched in CommonTestFunctionality, so let's use that
            stringVar.Value = "TestValue123";

            int slot = 10;
            ISaveMetaData meta = metaFactory.CreateMeta(slot);

            // Save
            await saveSystem.SaveTo(slot);

            // Change the variable to something else to ensure load will restore it
            stringVar.Value = "ChangedValue";

            // Load
            CompositeSaveData loadedMain = await saveSystem.LoadMain(slot, loadScene: false);

            // Assert: variable value should be restored
            Assert.AreEqual("TestValue123", stringVar.Value, "Flowchart variable was not restored after load.");
        }

        [Test]
        public void RegisterMainCodec_DelegatesToManager()
        {
            var dummyCodec = new DummyMainSaveCodec();
            saveSystem.RegisterMainCodec(dummyCodec);

            // There is no direct way to check registration, but this ensures no exceptions and coverage of the delegation.
            Assert.Pass("RegisterMainCodec did not throw and delegated as expected.");
        }

        [Test]
        public void RegisterSaveDataApplier_AddsToList()
        {
            var dummyApplier = new DummySaveDataApplier();
            saveSystem.RegisterSaveDataApplier(dummyApplier);

            Assert.Contains(dummyApplier, (System.Collections.ICollection)saveSystem.SaveDataAppliers);
        }

        [Test]
        public void SaveDirectoryPaths_CanBeSetAndGet()
        {
            var paths = new Dictionary<SaveDirectoryType, string>
            {
                { SaveDirectoryType.DataPath, "/tmp/test" }
            };
            saveSystem.SaveDirectoryPaths = paths;

            var result = saveSystem.SaveDirectoryPaths;
            Assert.AreEqual("/tmp/test", result[SaveDirectoryType.DataPath]);
        }

        [Test]
        public void SetSaveDirPath_UpdatesPath()
        {
            string newPath = "/tmp/another";
            saveSystem.SetSaveDirPath(SaveDirectoryType.DataPath, newPath);

            var result = saveSystem.SaveDirectoryPaths;
            Assert.AreEqual(newPath, result[SaveDirectoryType.DataPath]);
        }

        [Test]
        public void SaveName_SetAndGet_Works()
        {
            saveSystem.SaveName = "TestSave";
            Assert.AreEqual("TestSave", saveSystem.SaveName);
        }

        [Test]
        public void SaveNamePrefixSuffix_SetAndGet_Works()
        {
            saveSystem.SaveNamePrefix = "PRE_";
            saveSystem.SaveNameSuffix = "_SUF";
            Assert.AreEqual("PRE_", saveSystem.SaveNamePrefix);
            Assert.AreEqual("_SUF", saveSystem.SaveNameSuffix);
        }

        // Dummy implementations for testing registration
        public class DummyMainSaveCodec : IMainSaveCodec
        {
            public int Order { get; set; } = 0;

            public object ToMakeFrom { get; set; }

            public bool NeedsInput { get; set; } = false;

            public SaveDataUnit EncodeToUnit()
            {
                // Return a dummy SaveDataUnit for testing
                return new SaveDataUnit("DummyType", "{\"dummy\":true}");
            }

            public SaveData DecodeFrom(SaveDataUnit unit)
            {
                // Return null or a dummy SaveData as needed for your tests
                return null;
            }

            public bool CanHandle(object toMakeFrom)
            {
                // Dummy logic: can handle anything
                return true;
            }

            public bool CanHandle(string typeName)
            {
                // Dummy logic: can handle any type name
                return true;
            }

            public IList<SaveDataUnit> FindAndEncodeAll(System.Action<IList<SaveDataUnit>> onComplete = null)
            {
                var result = new List<SaveDataUnit> { EncodeToUnit() };
                onComplete?.Invoke(result);
                return result;
            }
        }

        protected class DummySaveDataApplier : ISaveDataApplier
        {
            public int Order => 0;
            public bool CanApply(SaveData saveData) => false;
            public bool CanApply(SaveDataUnit unit) => false;
            public Task ApplyRange(IList<SaveData> datas) => Task.CompletedTask;
            public Task Apply(SaveData saveData) => Task.CompletedTask;
        }
    }
}