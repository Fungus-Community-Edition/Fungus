using Amanita;
using Amanita.SaveSys;
using Amanita.SaveSys.UI;
using Amanita.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;

namespace SaveSystemTests
{
    public class SaveSlotUITests : CommonTestFunctionality
    {
        protected override string PathToTestScene => "ScenePrefabs/SaveSlotUITestScene";

        protected override void PrepScene()
        {
            base.PrepScene();
            viewController = GameObject.FindFirstObjectByType<SaveSlotViewComposer>();
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

            toDestroyInTearDown.Add(viewController.gameObject);
        }

        protected SaveSlotViewComposer viewController;
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
                UnityObj.Destroy(viewController.gameObject);
            }
        }

        protected virtual TimeSpan Playtime { get => metaData.Playtime; }

        [TestCaseSource(nameof(ValidSlotNumFormats))]
        public virtual void UpdatesNumberView_WithFormat(string format)
        {
            IntegerFormatter testFormatter = ScriptableObject.CreateInstance<IntegerFormatter>();
            toDestroyInTearDown.Add(testFormatter);

            testFormatter.FormatString = format;
            numberView.Formatter = testFormatter;

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
            toDestroyInTearDown.Add(testFormatter);
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
            toDestroyInTearDown.Add(formatter);
            formatter.name = "TempDateFormat";
            formatter.FormatString = formatStr;
            dateView.Formatter = formatter;

            string formattedDate = formatter.FormatToText(date);
            string expectedResult = $"{dateView.Prefix}{formattedDate}{dateView.Postfix}";
            Assert.AreEqual(expected, expectedResult);
        }

        //

        [Test]
        public void MetaPropagation_PassesMetaToAllViews()
        {
            // Arrange
            var testMeta = new SaveMetaData
            {
                TimeStamp = DateTime.UtcNow,
                SaveVersion = "2.0.0",
                Playtime = TimeSpan.FromMinutes(42),
                SlotNumber = 7
            };

            // Act
            viewController.Meta = testMeta;

            // Assert
            Assert.AreEqual(testMeta, playtimeView.Meta, "PlaytimeView did not receive meta");
            Assert.AreEqual(testMeta, dateView.Meta, "DateView did not receive meta");
            Assert.AreEqual(testMeta, numberView.Meta, "NumberView did not receive meta");
        }

        [Test]
        public void GetView_ReturnsCorrectViewType()
        {
            // Act
            var retrieved = viewController.GetView<SaveSlotPlaytimeView>();

            // Assert
            Assert.IsNotNull(retrieved, "GetView should return a valid PlaytimeView");
            Assert.AreSame(playtimeView, retrieved, "GetView did not return the expected instance");
        }

        [Test]
        public void GetView_ReturnsNull_WhenTypeNotPresent()
        {
            // Act
            var nonExistent = viewController.GetView<FakeSlotView>();

            // Assert
            Assert.IsNull(nonExistent, "GetView should return null when view type is not present");
        }

        //

        [Test]
        public void PassMetaToViews_SkipsNullViews_LogsError()
        {
            GameObject viewControllerGo = viewController.gameObject;
            UnityObj.Destroy(viewController);
            TestComposer testComposer = viewControllerGo.AddComponent<TestComposer>();
            
            var currentViews = new List<ISaveSlotView>
            {
                playtimeView,
                null, // simulate misconfigured prefab
                numberView
            };

            testComposer.InjectViews(currentViews);

            var testMeta = new SaveMetaData
            {
                TimeStamp = DateTime.UtcNow,
                SaveVersion = "3.0.0",
                Playtime = TimeSpan.FromMinutes(99),
                SlotNumber = 5
            };

            string expectedWarnMsg = "View at index 1 is null. Cannot pass meta data.";
            LogAssert.Expect(LogType.Warning, expectedWarnMsg);

            // Act + Assert: should not throw
            Assert.DoesNotThrow(() => testComposer.Meta = testMeta,
                "Composer should skip null views without throwing exceptions.");

            // Verify that non-null views still received the meta
            Assert.AreEqual(testMeta, playtimeView.Meta, "PlaytimeView did not receive meta");
            Assert.AreEqual(testMeta, numberView.Meta, "NumberView did not receive meta");

        }

        [Test]
        public void GetView_ReturnsNull_WhenViewIsNull()
        {
            // Arrange
            var viewsField = typeof(SaveSlotViewComposer)
                .GetField("views", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var currentViews = new List<ISaveSlotView>
            {
                null // simulate all views missing
            };

            viewsField.SetValue(viewController, currentViews);

            string expectedErrorMsg = "View at index 0 is null. This may indicate a misconfigured prefab.";
            LogAssert.Expect(LogType.Error, expectedErrorMsg);

            // Act
            var retrieved = viewController.GetView<SaveSlotPlaytimeView>();

            // Assert
            Assert.IsNull(retrieved, "GetView should return null when the stored view is null.");
        }


        // 

        [Test]
        public void HandlesExtremePlaytime()
        {
            var extremeMeta = new SaveMetaData
            {
                Playtime = TimeSpan.FromDays(999),
                SlotNumber = 42,
                TimeStamp = DateTime.MaxValue
            };

            viewController.Meta = extremeMeta;

            Assert.AreEqual(extremeMeta, playtimeView.Meta);
            Assert.AreEqual(extremeMeta, numberView.Meta);
            Assert.AreEqual(extremeMeta, dateView.Meta);
        }

        [Test]
        public void GetView_ReturnsNull_WhenViewsListEmpty()
        {
            ReplaceWithTestComposer();
            void ReplaceWithTestComposer()
            {
                GameObject viewControllerGo = viewController.gameObject;
                UnityObj.Destroy(viewController);
                viewController = viewControllerGo.AddComponent<TestComposer>();
            }

            var viewsField = typeof(SaveSlotViewComposer)
                .GetField("views", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            viewsField.SetValue(viewController, new List<ISaveSlotView>());

            LogAssert.Expect(LogType.Warning, "No views found. Ensure that SaveSlotViewComposer is properly initialized.");

            var retrieved = viewController.GetView<SaveSlotPlaytimeView>();
            Assert.IsNull(retrieved);
        }

        [Test]
        public void MultipleComposers_WorkIndependently()
        {
            var go1 = new GameObject("Composer1");
            var go2 = new GameObject("Composer2");
            toDestroyInTearDown.Add(go1);
            toDestroyInTearDown.Add(go2);
            var comp1 = go1.AddComponent<SaveSlotViewComposer>();
            var comp2 = go2.AddComponent<SaveSlotViewComposer>();

            var meta1 = new SaveMetaData { SlotNumber = 1 };
            var meta2 = new SaveMetaData { SlotNumber = 2 };

            comp1.Meta = meta1;
            comp2.Meta = meta2;

            Assert.AreEqual(meta1, comp1.Meta);
            Assert.AreEqual(meta2, comp2.Meta);
        }

        // Dummy interface implementation for negative test
        private class FakeSlotView : ISaveSlotView
        {
            public ISaveMetaData Meta { get; set; }

            public void Refresh()
            {
                // No implementation needed for this test
            }
        }
        private class TestComposer : SaveSlotViewComposer
        {
            public void InjectViews(IList<ISaveSlotView> injected) => views = injected;

            protected override void EnsureViews()
            {
                // No op to keep this from interfering with things
            }
        }




    }
}