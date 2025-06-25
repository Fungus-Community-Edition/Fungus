using Amanita.SaveSys;
using Amanita.SaveSys.UI;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using AmanitaSaveManager = Amanita.SaveSys.SaveManager;
using Encoding = System.Text.Encoding;
using UnityObject = UnityEngine.Object;

namespace Amanita.SaveSystemTests
{

    public class SaveSlotUITests : CommonTestFunctionality
    {
        protected override string PathToTestScene => "ScenePrefabs/SaveSlotUITestScene";

        protected override void PrepScene()
        {
            base.PrepScene();
            viewController = GameObject.FindFirstObjectByType<SaveSlotUIViewController>();
            Assert.IsNotNull(viewController, "SaveSlotUIViewController not found in the scene.");

            // There's no guarantee that the view controller will have its views parented
            // to its game object, hence the GetView func
            playtimeView = viewController.GetView<SaveSlotPlaytimeView>();
            dateView = viewController.GetView<SaveSlotDateView>();
            numberView = viewController.GetView<SaveSlotNumberView>();
            
            metaData = new SaveMetaData
            {
                TimeStamp = DateTime.UtcNow,
                SaveVersion = "1.0.0",
                Playtime = expectedPlaytime
            };

            viewController.Meta = metaData;

        }

        protected SaveSlotUIViewController viewController;
        protected SaveSlotPlaytimeView playtimeView;
        protected SaveSlotDateView dateView;
        protected SaveSlotNumberView numberView;
        protected TimeSpan expectedPlaytime = TimeSpan.FromHours(1.5);

        protected override bool ReqFlowchart => false;
        public override void DoOneTimeTearDown()
        {
            base.DoOneTimeTearDown();
            if (viewController != null)
            {
                UnityObject.Destroy(viewController.gameObject);
            }
        }

        [Test]
        public virtual void UpdatesPlaytimeView_AllFormats()
        {
            // We have all the setup done in the OneTimeSetUp method,
            // so we can directly test the playtime view.
            Assert.IsNotNull(viewController);
            Assert.IsNotNull(playtimeView);

            // We want to test for each format
            foreach (PlaytimeFormat format in Enum.GetValues(typeof(PlaytimeFormat)))
            {
                playtimeView.Format = format;
                string playtimeStr = Playtime.ToFormattedString(format);
                string expectedText = $"{playtimeView.Prefix}{playtimeStr}";
                Assert.AreEqual(expectedText, playtimeView.Text);
            }

        }

        protected virtual TimeSpan Playtime { get => metaData.Playtime; }
    }
}