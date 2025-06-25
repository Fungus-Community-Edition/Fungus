using Amanita.SaveSys;
using Amanita.SaveSys.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            Assert.IsNotNull(playtimeView);
            Assert.IsNotNull(dateView);
            Assert.IsNotNull(numberView);

            metaData = new SaveMetaData
            {
                TimeStamp = DateTime.UtcNow,
                SaveVersion = "1.0.0",
                Playtime = expectedPlaytime,
                SlotNumber = 1 // For the sake of Roman Numeral support, we won't go with 0
            };

            viewController.Meta = metaData;

            playtimeFormatVals = Enum.GetValues(typeof(PlaytimeFormat));
            slotNumFormatVals = Enum.GetValues(typeof(SlotNumFormat));
        }

        protected SaveSlotUIViewController viewController;
        protected SaveSlotPlaytimeView playtimeView;
        protected SaveSlotDateView dateView;
        protected SaveSlotNumberView numberView;
        protected TimeSpan expectedPlaytime = TimeSpan.FromHours(1.5);
        protected Array playtimeFormatVals, slotNumFormatVals;


        protected override bool ReqFlowchart => false;
        public override void DoOneTimeTearDown()
        {
            base.DoOneTimeTearDown();
            if (viewController != null)
            {
                UnityObject.Destroy(viewController.gameObject);
            }
        }

        protected virtual TimeSpan Playtime { get => metaData.Playtime; }

        [TestCaseSource(nameof(ValidSlotNumFormats))]
        public virtual void UpdatesNumberView_Format(SlotNumFormat format)
        {
            numberView.Format = format;
            string numStr = SlotNumber.ToString(format);
            string expectedText = $"{numberView.Prefix}{numStr}";
            Assert.AreEqual(expectedText, numberView.Text);
        }

        protected virtual int SlotNumber => metaData.SlotNumber;

        public static IEnumerable<SlotNumFormat> ValidSlotNumFormats()
        {
            return Enum.GetValues(typeof(SlotNumFormat))
                       .Cast<SlotNumFormat>()
                       .Where(formatEl => formatEl != SlotNumFormat.Custom &&
                       formatEl != SlotNumFormat.Null);
        }

        [TestCaseSource(nameof(ValidPlaytimeFormats))]
        public void UpdatesPlaytimeView_Format(PlaytimeFormat format)
        {
            playtimeView.Format = format;
            string playtimeStr = Playtime.ToFormattedString(format);
            string expectedText = $"{playtimeView.Prefix}{playtimeStr}";
            Assert.AreEqual(expectedText, playtimeView.Text);
        }

        public static IEnumerable<PlaytimeFormat> ValidPlaytimeFormats()
        {
            return Enum.GetValues(typeof(PlaytimeFormat))
                       .Cast<PlaytimeFormat>()
                       .Where(f => f != PlaytimeFormat.Custom && f != PlaytimeFormat.Null);
        }

        public static IEnumerable<TestCaseData> DateFormatTestCases()
        {
            var date = new DateTime(2025, 6, 25, 10, 45, 0);
            yield return new TestCaseData("yyyy-MM-dd", date, "2025-06-25");
            yield return new TestCaseData("MM/dd/yyyy", date, "06/25/2025");
            yield return new TestCaseData("MMMM dd, yyyy – hh:mm tt", date, "June 25, 2025 – 10:45 AM");
        }

        [TestCaseSource(nameof(DateFormatTestCases))]
        public void DateFormat_StrategyOutputsExpectedString(string formatStr, DateTime date, string expected)
        {
            var formatter = ScriptableObject.CreateInstance<DateFormat>();
            formatter.name = "TempDateFormat";
            formatter.InTextForm = formatStr;
            typeof(DateFormat).GetField("inTextForm", BindingFlags.NonPublic | BindingFlags.Instance)
                              ?.SetValue(formatter, formatStr);

            Assert.AreEqual(expected, formatter.FormatDate(date));
        }


    }
}