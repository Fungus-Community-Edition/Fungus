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


        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            // Sanity checks: installer should have wired these already.
            Assert.IsNotNull(SaveSystemInstaller.S, "SaveSystemInstaller singleton not present.");
        }

    }
}