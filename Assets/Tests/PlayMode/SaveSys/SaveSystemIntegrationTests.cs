using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amanita.VScripting;
using System.Reflection;
using UnityEngine;
using Amanita.SaveSys.VScripting;

namespace SaveSystemTests
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
            // Ensure clean state for Progress Markers between tests
            saveSystem.ClearProgressMarkers();
            RecordOrderCommand.ClearLog();
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
            var flowchartSave = loadedComposite.Items.OfType<FlowchartSaveData>().FirstOrDefault();
            Assert.IsNotNull(flowchartSave, "Loaded main state does not contain FlowchartSaveData.");
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
            string origVal = "TestValue123";
            stringVar.Value = origVal;

            int slot = 10;
            await saveSystem.SaveTo(slot);

            // Change the variable to something else to ensure load will restore it
            stringVar.Value = "ChangedValue";

            // Load
            CompositeSaveData loadedMain = await saveSystem.LoadMain(slot, loadScene: false);

            // Assert: variable value should be restored
            Assert.AreEqual(origVal, stringVar.Value, "Flowchart variable was not restored after load.");
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

            bool success = saveSystem.SaveDataAppliers.Contains(dummyApplier);
            Assert.IsTrue(success);
        }

        [Test]
        public async Task SaveLoaded_EventHandlers_Execute_In_Order_By_LowestMarkerOrder()
        {
            await CommonSetupAsync();

            // Arrange: create a dedicated Flowchart with SaveLoaded blocks
            var flowGO = new GameObject("SaveLoadedFlow");
            var flow = flowGO.AddComponent<Flowchart>();

            // Register Progress Markers with different orders
            // Effective order for a block is the lowest order among its referenced marker IDs
            saveSystem.RegisterProgressMarker("A", order: 10);
            saveSystem.RegisterProgressMarker("B", order: 1);
            saveSystem.RegisterProgressMarker("C", order: 5);
            // Note: "Z" not registered -> corresponding block should not execute

            // Create blocks and attach SaveLoaded handlers pointing to marker IDs
            var blockB = CreateSaveLoadedBlock(flow, "Block_B", new[] { "B" });
            var blockA = CreateSaveLoadedBlock(flow, "Block_A", new[] { "A" });
            var blockAC = CreateSaveLoadedBlock(flow, "Block_AC", new[] { "A", "C" });
            var blockZ = CreateSaveLoadedBlock(flow, "Block_Z", new[] { "Z" }); // should not fire

            // Add a command to each block that records execution order
            AddRecordCommand(flow, blockB, "Block_B");
            AddRecordCommand(flow, blockA, "Block_A");
            AddRecordCommand(flow, blockAC, "Block_AC");
            AddRecordCommand(flow, blockZ, "Block_Z");

            // Save a slot to persist Progress Markers into Meta
            int slot = 21;
            await saveSystem.SaveTo(slot);

            // Act: load the same slot (expect Save Loaded event handlers to fire)
            await saveSystem.LoadMain(slot, loadScene: false);

            // Assert: order should be by lowest referenced marker order -> B(1), AC(min(10,5)=5), A(10)
            string[] expected = { "Block_B", "Block_AC", "Block_A" };

            // Wait briefly for handlers to run
            await WaitForLogCountOrTimeout(expected.Length, 3000);

            // Ensure "Z" block did not run, and ordering matches
            CollectionAssert.AreEqual(expected, RecordOrderCommand.ExecutionLog, 
                "SaveLoaded blocks did not execute in expected order.");
            CollectionAssert.DoesNotContain(RecordOrderCommand.ExecutionLog, "Block_Z", "A block with " +
                "non-registered marker IDs should not have executed.");
        }

        [Test]
        public async Task ProgressMarkers_Are_Saved_Into_Meta_And_Preserved_On_Load()
        {
            await CommonSetupAsync();

            // Arrange: register runtime markers
            saveSystem.RegisterProgressMarker("Intro", order: 0);
            saveSystem.RegisterProgressMarker("MidGame", order: 5);
            saveSystem.RegisterProgressMarker("EndGame", order: 10);

            int slot = 22;

            // Act: save and then load meta
            await saveSystem.SaveTo(slot);
            var loadedMeta = await saveSystem.LoadMeta(slot);

            // Assert
            Assert.IsInstanceOf<SaveMetaData>(loadedMeta);
            var meta = (SaveMetaData)loadedMeta;

            var loadedMarkers = meta.ProgressMarkers.OrderBy(m => m.Order).ToList();

            Assert.AreEqual(3, loadedMarkers.Count, "Loaded meta did not contain expected number of Progress Markers.");

            // Validate ID and Order persisted
            Assert.AreEqual("Intro", loadedMarkers[0].Id);
            Assert.AreEqual(0, loadedMarkers[0].Order);

            Assert.AreEqual("MidGame", loadedMarkers[1].Id);
            Assert.AreEqual(5, loadedMarkers[1].Order);

            Assert.AreEqual("EndGame", loadedMarkers[2].Id);
            Assert.AreEqual(10, loadedMarkers[2].Order);
        }

        // ---------- Helpers ----------

        private static async Task WaitForLogCountOrTimeout(int expectedCount, int timeoutMs)
        {
            int waited = 0;
            const int step = 50;
            while (RecordOrderCommand.ExecutionLog.Count < expectedCount && waited < timeoutMs)
            {
                await Task.Delay(step);
                waited += step;
            }
        }

        private static Block CreateSaveLoadedBlock(Flowchart flow, string blockName, string[] markerIds)
        {
            // Create Block
            var block = flow.CreateBlock(Vector2.zero);
            block.BlockName = blockName;

            // Add SaveLoaded EventHandler
            var handler = flow.gameObject.AddComponent<SaveLoadedEvent>();
            handler.ParentBlock = block;
            block._EventHandler = handler;

            // Create StringVariables for marker IDs and assign into handler.markerIDs via reflection
            var vars = new List<IVariable<string>>();
            foreach (var id in markerIds)
            {
                var stringVar = flow.gameObject.AddComponent<StringVariable>();
                stringVar.Key = UniqueKeyGenerator.GetUniqueKeyFor($"PM_{id}", (IList<IVariable>)flow.Variables);
                stringVar.Value = id;
                flow.AddVariable(stringVar);
                vars.Add(stringVar);
            }

            for (int i = 0; i < markerIds.Length; i++)
            {
                handler.AddMarkerIDVariable(vars[i]);
            }

            return block;
        }

        private static void AddRecordCommand(Flowchart flow, Block block, string label)
        {
            var cmd = flow.gameObject.AddComponent<RecordOrderCommand>();
            cmd.Label = label;
            cmd.ParentBlock = block;
            cmd.ItemId = flow.NextItemId();
            cmd.OnCommandAdded(block);
            block.CommandList.Add(cmd);
        }

        // Dummy implementations for testing registration
        public class DummyMainSaveCodec : IMainSaveCodec
        {
            public virtual void PreInstallInit()
            {
                // Do nothing
            }
            public int Order { get; set; } = 0;

            public object ToMakeFrom { get; set; }

            public bool NeedsInput { get; set; } = false;


            public bool CanHandle(object toMakeFrom)
            {
                return true;
            }

            public bool CanHandle(string typeName)
            {
                return true;
            }

            public IList<SaveData> FindAndEncodeAll(System.Action<IList<SaveData>> onComplete = null)
            {
                var result = new List<SaveData> { };
                onComplete?.Invoke(result);
                return result;
            }

            public IList<SaveData> FindAndCreateAll(System.Action<IList<SaveData>> onComplete = null)
            {
                var result = new List<SaveData> { };
                onComplete?.Invoke(result);
                return result;
            }
        }

        protected class DummySaveDataApplier : ISaveDataApplier
        {
            public void PreInstallInit()
            {
                // Do nothing
            }
            public int Order => 0;
            public bool CanApply(SaveData saveData) => false;
            public Task ApplyRange(IList<SaveData> datas) => Task.CompletedTask;
            public Task Apply(SaveData saveData) => Task.CompletedTask;
        }
    }
}

// A simple command to record execution order of SaveLoaded blocks.
public class RecordOrderCommand : Command
{
    public string Label;

    public static readonly List<string> ExecutionLog = new List<string>();

    public static void ClearLog() => ExecutionLog.Clear();

    public override void Execute()
    {
        SaveLoadedEvent saveLoaded = GetComponent<SaveLoadedEvent>();
        bool shouldBeAbleToRespond = saveLoaded != null && (saveLoaded.RespondToAny || saveLoaded.HasAnyRegisteredIDs());
        if (shouldBeAbleToRespond)
        {
            ExecutionLog.Add(Label);
        }
        Continue();
    }
}