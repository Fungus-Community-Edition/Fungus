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
        protected override bool ReqSaveSystem => true;
        protected override bool ReqSceneLoad => true;
        protected override bool ReqFlowchart => true;
        protected override bool ShouldDeleteTestSavesAtEnd => true;

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

        protected virtual void ApplyResolvers()
        {
            saveSystem.SavePathResolver = testPathResolver;
            saveReader.PathResolver = testPathResolver;
            saveWriter.PathResolver = testPathResolver;
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

        // ---------- ProgressMarkerCommand Tests ----------

        [Test]
        public async Task ProgressMarkerCommand_Register_RegistersMarker_WithOrder()
        {
            await CommonSetupAsync();

            var flow = new GameObject("PMC_Flow_Register").AddComponent<Flowchart>();
            var block = CreatePlainBlock(flow, "PMC_Register");

            var cmd = AddProgressMarkerCommand(flow, block,
                ProgressMarkerCommand.PMCAction.Register, "P_REG", 7);

            cmd.Execute();

            Assert.IsTrue(saveSystem.IsProgressMarkerRegistered("P_REG"), "Marker was not registered.");
            var marker = saveSystem.GetProgressMarkerByID("P_REG");
            Assert.NotNull(marker);
            Assert.AreEqual(7, marker.Order, "Marker order not set during registration.");

            toDestroyInTearDown.Add(flow.gameObject);
        }

        [Test]
        public async Task ProgressMarkerCommand_Unregister_RemovesMarker()
        {
            await CommonSetupAsync();

            saveSystem.RegisterProgressMarker("P_UNREG", 3);

            var flow = new GameObject("PMC_Flow_Unregister").AddComponent<Flowchart>();
            var block = CreatePlainBlock(flow, "PMC_Unregister");

            var cmd = AddProgressMarkerCommand(flow, block,
                ProgressMarkerCommand.PMCAction.Unregister, "P_UNREG", 0);

            cmd.Execute();

            Assert.IsFalse(saveSystem.IsProgressMarkerRegistered("P_UNREG"), "Marker was not unregistered.");
            toDestroyInTearDown.Add(flow.gameObject);
        }

        [Test]
        public async Task ProgressMarkerCommand_SetOrder_CreatesMarker_WhenMissing()
        {
            await CommonSetupAsync();

            var flow = new GameObject("PMC_Flow_SetOrderCreate").AddComponent<Flowchart>();
            var block = CreatePlainBlock(flow, "PMC_SetOrderCreate");

            var cmd = AddProgressMarkerCommand(flow, block,
                ProgressMarkerCommand.PMCAction.SetOrder, "P_CREATE", 9);

            cmd.Execute();

            Assert.IsTrue(saveSystem.IsProgressMarkerRegistered("P_CREATE"), "Marker should have been created by SetOrder.");
            var marker = saveSystem.GetProgressMarkerByID("P_CREATE");
            Assert.NotNull(marker);
            Assert.AreEqual(9, marker.Order, "Marker order not set correctly on creation via SetOrder.");
            toDestroyInTearDown.Add(flow.gameObject);
        }

        [Test]
        public async Task ProgressMarkerCommand_SetOrder_UpdatesExistingMarker()
        {
            await CommonSetupAsync();

            saveSystem.RegisterProgressMarker("P_UPDATE", 1);

            var flow = new GameObject("PMC_Flow_SetOrderUpdate").AddComponent<Flowchart>();
            var block = CreatePlainBlock(flow, "PMC_SetOrderUpdate");

            var cmd = AddProgressMarkerCommand(flow, block,
                ProgressMarkerCommand.PMCAction.SetOrder, "P_UPDATE", 14);

            cmd.Execute();

            var marker = saveSystem.GetProgressMarkerByID("P_UPDATE");
            Assert.NotNull(marker);
            Assert.AreEqual(14, marker.Order, "Existing marker order was not updated.");

            toDestroyInTearDown.Add(flow.gameObject);
        }

        [Test]
        public async Task ProgressMarkerCommand_NullAction_DoesNothing()
        {
            await CommonSetupAsync();

            var flow = new GameObject("PMC_Flow_Null").AddComponent<Flowchart>();
            var block = CreatePlainBlock(flow, "PMC_Null");

            var cmd = AddProgressMarkerCommand(flow, block,
                ProgressMarkerCommand.PMCAction.Null, "IGNORED", 123);

            cmd.Execute();

            // No markers should have been created or modified
            Assert.AreEqual(0, saveSystem.ProgressMarkers.Count, "Null action should not affect progress markers.");
            toDestroyInTearDown.Add(flow.gameObject);
        }

        [Test]
        public async Task ProgressMarkerCommand_GetSummary_ReturnsExpected()
        {
            await CommonSetupAsync();

            var flow = new GameObject("PMC_Flow_Summary").AddComponent<Flowchart>();
            var block = CreatePlainBlock(flow, "PMC_Summary");

            var cmd = AddProgressMarkerCommand(flow, block,
                ProgressMarkerCommand.PMCAction.Register, "P_SUM", 42);

            string summary = cmd.GetSummary();
            Assert.AreEqual("Register | ID: P_SUM | Order: 42", summary);
            toDestroyInTearDown.Add(flow.gameObject);
        }

        // ---------- NEW TESTS: SaveManager Init Meta Handling ----------

        [Test]
        public async Task SaveManager_Init_LoadsMetas_RegistersInRegistry_And_FiresEvent()
        {
            await CommonSetupAsync();

            DeleteAllTestSaves();

            // Use concrete SaveManager to access overload with saveName
            var concreteManager = (SaveManager)saveManager;
            var registry = concreteManager.Registry;
            int firstSlot = 40;
            int secondSlot = 41;

            string firstSlotName = "FirstSlotMetaName";
            string secondSlotName = "SecondSlotMetaName";

            await concreteManager.SaveTo(firstSlot, firstSlotName);
            await concreteManager.SaveTo(secondSlot, secondSlotName);

            RegisterThoseForCleanup();
            void RegisterThoseForCleanup()
            {
                string pathToFirstSlot = concreteManager.SaveRepo.GetPathTo(firstSlot);
                string pathToSecondSlot = concreteManager.SaveRepo.GetPathTo(secondSlot);
                saveFilePathsForCleanup.Add(pathToFirstSlot);
                saveFilePathsForCleanup.Add(pathToSecondSlot);
            }

            // Capture original metas (deep copies) for later comparison
            var firstSlotOrigMeta = SaveMetaData.CreateFrom(registry.GetSaveMeta(firstSlot));
            var secondSlotOrigMeta = SaveMetaData.CreateFrom(registry.GetSaveMeta(secondSlot));

            // Clear in‑memory registry to simulate fresh startup
            concreteManager.Registry.Clear();
            Assert.AreEqual(0, concreteManager.Registry.GetAllSaveMetas().Count, "Registry should be empty before Init.");

            IList<ISaveMetaData> metasFromEvent = null;
            bool eventFired = false;

            void Handler(IList<ISaveMetaData> metas)
            {
                eventFired = true;
                metasFromEvent = metas;
            }

            SaveSysSignals.SaveMetasReadOnInit += Handler;
            try
            {
                await concreteManager.Init(); // Re-run init logic
            }
            finally
            {
                SaveSysSignals.SaveMetasReadOnInit -= Handler;
            }

            // Verify event fired
            Assert.IsTrue(eventFired, "SaveMetasReadOnInit event was not fired during Init.");
            Assert.IsNotNull(metasFromEvent, "Event metas list was null.");
            Assert.AreEqual(2, metasFromEvent.Count, "Unexpected number of metas returned by event.");

            // Verify metas registered in registry
            var registeredMetas = concreteManager.Registry.GetAllSaveMetas();
            Assert.AreEqual(2, registeredMetas.Count, "Registry did not contain expected number of metas after Init.");

            // Slot presence
            Assert.IsTrue(registeredMetas.Any(m => m.SlotNumber == firstSlot), "SlotA meta not registered.");
            Assert.IsTrue(registeredMetas.Any(m => m.SlotNumber == secondSlot), "SlotB meta not registered.");

            // Decryption / integrity: Compare key fields to originals
            var loadedA = concreteManager.Registry.GetSaveMeta(firstSlot) as SaveMetaData;
            var loadedB = concreteManager.Registry.GetSaveMeta(secondSlot) as SaveMetaData;
            Assert.NotNull(loadedA);
            Assert.NotNull(loadedB);

            Assert.AreEqual(firstSlotOrigMeta.SaveName, loadedA.SaveName, "Meta A SaveName mismatch after Init (possible decryption failure).");
            Assert.AreEqual(secondSlotOrigMeta.SaveName, loadedB.SaveName, "Meta B SaveName mismatch after Init (possible decryption failure).");
            Assert.AreEqual(firstSlotOrigMeta.SlotNumber, loadedA.SlotNumber, "Meta A SlotNumber mismatch.");
            Assert.AreEqual(secondSlotOrigMeta.SlotNumber, loadedB.SlotNumber, "Meta B SlotNumber mismatch.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(loadedA.SaveVersion), "Meta A SaveVersion not set.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(loadedB.SaveVersion), "Meta B SaveVersion not set.");
        }

        [Test]
        public async Task SaveManager_Init_RegistersOnlyUniqueMetas()
        {
            await CommonSetupAsync();
            DeleteAllTestSaves();
            var concreteManager = (SaveManager)saveManager;

            int slot = 42;
            await concreteManager.SaveTo(slot, "UniqueMeta");

            // Make a second save overwrite (simulate updated meta)
            await concreteManager.SaveTo(slot, "UniqueMeta_Updated");

            string pathToSlot = concreteManager.SaveRepo.GetPathTo(slot);
            saveFilePathsForCleanup.Add(pathToSlot);

            var registry = concreteManager.Registry;
            var originalMetaCopy = SaveMetaData.CreateFrom(registry.GetSaveMeta(slot));
            registry.Clear();

            int eventInvocationCount = 0;
            SaveSysSignals.SaveMetasReadOnInit += MetasHandler;
            void MetasHandler(IList<ISaveMetaData> metas)
            {
                eventInvocationCount++;
            }

            try
            {
                await concreteManager.Init();
            }
            finally
            {
                SaveSysSignals.SaveMetasReadOnInit -= MetasHandler;
            }

            Assert.AreEqual(1, eventInvocationCount, "SaveMetasReadOnInit should fire exactly once.");
            var metas = concreteManager.Registry.GetAllSaveMetas();
            Assert.AreEqual(1, metas.Count, "Registry should contain exactly one meta for the slot.");
            var loaded = metas[0] as SaveMetaData;
            Assert.NotNull(loaded);
            Assert.AreEqual(slot, loaded.SlotNumber, "Loaded meta slot mismatch.");
            Assert.AreEqual(originalMetaCopy.SaveName, loaded.SaveName, "Loaded meta SaveName mismatch.");
        }

        [Test]
        public async Task SaveManager_Init_Event_Passes_Same_Meta_Instances_As_Registry()
        {
            await CommonSetupAsync();
            var concreteManager = (SaveManager)saveManager;

            int firstSlot = 43;
            int secondSlot = 44;
            await concreteManager.SaveTo(firstSlot, "InstanceCheckA");
            await concreteManager.SaveTo(secondSlot, "InstanceCheckB");

            RegisterThoseForCleanup();
            void RegisterThoseForCleanup()
            {
                string pathToFirstSlot = concreteManager.SaveRepo.GetPathTo(firstSlot);
                string pathToSecondSlot = concreteManager.SaveRepo.GetPathTo(secondSlot);
                saveFilePathsForCleanup.Add(pathToFirstSlot);
                saveFilePathsForCleanup.Add(pathToSecondSlot);
            }

            concreteManager.Registry.Clear();

            IList<ISaveMetaData> metasFromEvent = null;
            SaveSysSignals.SaveMetasReadOnInit += Handler;
            void Handler(IList<ISaveMetaData> metas) => metasFromEvent = metas;
            try
            {
                await concreteManager.Init();
            }
            finally
            {
                SaveSysSignals.SaveMetasReadOnInit -= Handler;
            }

            Assert.IsNotNull(metasFromEvent, "Event provided null metas list.");
            var registryMetas = concreteManager.Registry.GetAllSaveMetas();

            // Ensure same count and referential equality (not just value equality)
            Assert.AreEqual(registryMetas.Count, metasFromEvent.Count, "Event meta count differs from registry.");
            foreach (var meta in registryMetas)
            {
                Assert.IsTrue(metasFromEvent.Contains(meta), "Event did not pass the same meta instance held in registry.");
            }
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

        private static Block CreatePlainBlock(Flowchart flow, string blockName)
        {
            var block = flow.CreateBlock(Vector2.zero);
            block.BlockName = blockName;
            return block;
        }

        private static ProgressMarkerCommand AddProgressMarkerCommand(
            Flowchart flow,
            Block block,
            ProgressMarkerCommand.PMCAction action,
            string id,
            int order)
        {
            var command = flow.gameObject.AddComponent<ProgressMarkerCommand>();
            command.ParentBlock = block;
            command.ItemId = flow.NextItemId();
            command.OnCommandAdded(block);
            block.CommandList.Add(command);

            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var type = typeof(ProgressMarkerCommand);

            // Set action
            var actionField = type.GetField("action", flags);
            actionField.SetValue(command, action);

            // Replace data objects directly to avoid ambiguous reflection on Value
            var markerIdField = type.GetField("markerID", flags);
            markerIdField.SetValue(command, new StringData(id));

            var markerOrderField = type.GetField("markerOrder", flags);
            markerOrderField.SetValue(command, new IntegerData(order));

            return command;
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