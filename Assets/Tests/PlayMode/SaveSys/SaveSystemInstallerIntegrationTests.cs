using NUnit.Framework;
using Amanita.SaveSys;
using System.IO;
using System.Threading.Tasks;
using AmanitaSaveManager = Amanita.SaveSys.SaveManager;

namespace SaveSystemTests
{
    public class SaveSystemInstallerIntegrationTests : CommonTestFunctionality
    {
        [Test]
        public async Task SaveManager_FromInstaller_WritesSaveToDisk()
        {
            // Arrange: Get SaveManager from installer singleton
            var manager = SaveSystemInstaller.SaveManager;
            var metaFactory = SaveSystemInstaller.MetaFactory;
            var registry = SaveSystemInstaller.Registry;
            var testSlot = 9;
            var saveName = "InstallerIntegrationSave";
            var saveDirType = SaveSystemInstaller.SaveDirectoryType;
            var installerSaveReader = SaveSystemInstaller.S.SaveReader;

            // Make sure slot is clean
            var readReq = new SaveReadRequest { SlotNumber = testSlot };
            var path = installerSaveReader.GetSaveFilePath(saveDirType, testSlot);
            //var path = SaveSystemInstaller.SaveRepo.GetSavePath(readReq);
            if (File.Exists(path)) File.Delete(path);

            AmanitaSaveManager managerToUse = (AmanitaSaveManager)manager;
            // Act
            await managerToUse.SaveTo(testSlot, saveName, default);

            // Assert
            Assert.IsTrue(File.Exists(path), $"Save file at slot {testSlot} was not created");

            var meta = registry.GetSaveMeta(testSlot);
            Assert.AreEqual(testSlot, meta.SlotNumber);
            Assert.AreEqual(saveName, meta.SaveName);
        }
    }
}