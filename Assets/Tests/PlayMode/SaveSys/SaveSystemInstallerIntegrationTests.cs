using Amanita.SaveSys;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace SaveSystemTests
{
    public class SaveSystemInstallerIntegrationTests : CommonTestFunctionality
    {
        // Installer tests need the SaveSystem but not a scene or flowchart.
        protected override bool ReqSaveSystem => true;
        protected override bool ReqSceneLoad => false;
        protected override bool ReqFlowchart => false;
        protected override bool ShouldDeleteTestSavesAtEnd => true;

        protected SaveReader installerReader => SaveSystemInstaller.S.SaveReader;
        protected ISaveManager installerManager => SaveSystemInstaller.SaveManager;
        protected SaveDirectoryType installerDirType => SaveSystemInstaller.SaveDirectoryType;
        protected SaveRegistry installerRegistry => SaveSystemInstaller.Registry;

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            // Sanity checks: installer should have wired these already.
            Assert.IsNotNull(SaveSystemInstaller.S, "SaveSystemInstaller singleton not present.");
            Assert.IsNotNull(installerManager, "SaveManager from installer is null.");
            Assert.IsNotNull(installerReader, "SaveReader from installer is null.");
            Assert.IsNotNull(installerRegistry, "Registry from installer is null.");
        }

        [Test]
        public async Task SaveManager_FromInstaller_WritesSaveToDisk()
        {
            int slot = 9;
            string saveName = "InstallerIntegrationSave";

            string path = installerReader.GetSaveFilePath(installerDirType, slot);
            if (File.Exists(path))
                File.Delete(path);

            // Save
            await ((SaveManager)installerManager).SaveTo(slot, saveName);

            // Assert file
            Assert.IsTrue(File.Exists(path), $"Save file for slot {slot} was not created at {path}");

            // Assert meta registered
            var meta = installerRegistry.GetSaveMeta(slot);
            Assert.IsNotNull(meta, "Meta not registered in registry.");
            Assert.AreEqual(slot, meta.SlotNumber);
            Assert.AreEqual(saveName, meta.SaveName);
        }
    }
}