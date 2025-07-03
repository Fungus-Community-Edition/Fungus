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
using Amanita.UI;

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

            playtimeFormatVals = Enum.GetValues(typeof(PlaytimeFormatEnum));
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
        public virtual void UpdatesNumberView_WithFormat(string format)
        {
            IntegerFormatter testFormatter = ScriptableObject.CreateInstance<IntegerFormatter>();
            testFormatter.FormatString = format;
            numberView.Formatter = testFormatter;
            //typeof(IntegerFormatter).GetField("formatString", BindingFlags.NonPublic | BindingFlags.Instance)
            //                      ?.SetValue(testFormatter, format);

            string numStr;
            if (format.Equals("Roman", StringComparison.OrdinalIgnoreCase))
            {
                numStr = RomanNumeralConverter.ToRoman(SlotNumber);
            }
            else
            {
                numStr = SlotNumber.ToString(format);
            }
            string expectedText = $"{numberView.Prefix}{numStr}{numberView.Postfix}";
            Assert.AreEqual(expectedText, numberView.Text);
        }

        protected virtual int SlotNumber => metaData.SlotNumber;

        public static IEnumerable<string> ValidSlotNumFormats()
        {
            yield return "D1"; // Default format, one digit
            yield return "D2"; // Default format
            yield return "D3"; // Three digits
            yield return "Roman"; // Roman numeral format

        }

        [TestCaseSource(nameof(ValidPlaytimeFormats))]
        public void UpdatesPlaytimeView_WithFormat(string formatInTextForm)
        {
            PlaytimeFormatter testFormatter = ScriptableObject.CreateInstance<PlaytimeFormatter>();
            testFormatter.FormatString = formatInTextForm;
            playtimeView.Formatter = testFormatter;

            string playtimeStr = Playtime.ToString(formatInTextForm, false);
            string expectedText = $"{playtimeView.Prefix}{playtimeStr}{playtimeView.Postfix}";
            Assert.AreEqual(expectedText, playtimeView.Text);
        }

        public static IEnumerable<string> ValidPlaytimeFormats()
        {
            yield return "ss";
            yield return "mm:ss";
            yield return "hh:mm:ss";
            yield return "d.hh:mm:ss";

        }

        public static IEnumerable<TestCaseData> DateFormatTestCases()
        {
            var date = new DateTime(2025, 6, 25, 10, 45, 0);
            yield return new TestCaseData("yyyy-MM-dd", date, "2025-06-25");
            yield return new TestCaseData("MM/dd/yyyy", date, "06/25/2025");
            yield return new TestCaseData("MMMM dd, yyyy – hh:mm tt", date, "June 25, 2025 – 10:45 AM");
        }

        [TestCaseSource(nameof(DateFormatTestCases))]
        public void UpdatesDateView_WithFormat(string formatStr, DateTime date, string expected)
        {
            var formatter = ScriptableObject.CreateInstance<DateFormatter>();
            formatter.name = "TempDateFormat";
            formatter.FormatString = formatStr;
            dateView.Formatter = formatter;

            string formattedDate = formatter.FormatToText(date);
            string expectedResult = $"{dateView.Prefix}{formattedDate}{dateView.Postfix}";
            Assert.AreEqual(expected, expectedResult);
        }


    }
}