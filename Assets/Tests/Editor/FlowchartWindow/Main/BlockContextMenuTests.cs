using Amanita.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Amanita.VScripting.EditorUtils;
using Amanita.VScripting;

namespace VScriptingTests.FCWindowOperations
{
    [TestFixture]
    class BlockContextMenuHandlerTests
    {
        [SetUp]
        public virtual void Setup()
        {
            host = new FakeFlowchartHost();
            host.Init();
            host.CreateBlock(host.Flowchart, Vector2.zero);

            menuFactory = new FakeContextMenuFactory();
            handler = new BlockContextMenuHandler(host, menuFactory);

            ctx = new FlowchartContext
            {
                Flowchart = host.Flowchart,
                FcHost = null // not used by the handler
            };

            rightClickEmptySpace = RightClick(whereEmptySpaceShouldBe);
            rightClickBlock = RightClick(whereABlockShouldBe);

            _fcWindowEditing = host.GetComponent<FcWindowEditing>();

        }

        protected FakeFlowchartHost host;
        protected FakeContextMenuFactory menuFactory;
        protected BlockContextMenuHandler handler;
        protected FlowchartContext ctx;
        protected Event rightClickEmptySpace;
        protected FcWindowEditing _fcWindowEditing;
        Event RightClick(Vector2 pos) => RightClick(pos.x, pos.y);
        Event RightClick(float x, float y) => new Event
        {
            type = EventType.MouseDown,
            button = 1,
            mousePosition = new Vector2(x, y)
        };

        protected readonly Vector2 whereEmptySpaceShouldBe = new Vector2(50, 10);
        protected readonly Vector2 whereABlockShouldBe = Vector2.zero;

        protected Event rightClickBlock;

        [TearDown]
        public virtual void TearDown()
        {
            rightClickEmptySpace = rightClickBlock = null;
            _fcWindowEditing = null;
            host.Dispose();
        }

        [Test]
        public virtual void StartsWithEmptyClipboard()
        {
            IContextMenu lastMenu = menuFactory.Create();
            IList<IContextMenuItem> actualItems = lastMenu.Items;
            bool noItems = actualItems.Count == 0;
            Assert.IsTrue(noItems, "Clipboard should start empty, but doesn't.");
        }

        [TestCaseSource(nameof(ExpectedEmptyRightClickLabels))]
        public virtual void RightClickEmpty_MenuContainsExpectedItem(string expectedLabel)
        {
            expectedLabel = expectedLabel.ToLower();
            Assert.IsTrue(handler.Handle(rightClickEmptySpace, ctx), "Handler should consume the right-click");

            var lastMenu = menuFactory.LastMenu;
            IList<string> items = lastMenu.Items.Select(i => i.Content.text.ToLower()).ToList();
            bool hasTheItem = items.Contains(expectedLabel);
            Assert.IsTrue(hasTheItem, $"Menu should contain '{expectedLabel}'.");
        }

        static readonly string[] ExpectedEmptyRightClickLabels = 
        {
            "Add",
            "Paste",
            "---",
            "Stop All"
        };

        [Test]
        public virtual void RightClickEmpty_EmptyClipboard_PasteDisabled()
        {
            Assert.IsTrue(handler.Handle(rightClickEmptySpace, ctx), "Handler should consume the right-click");

            var lastMenu = menuFactory.LastMenu;
            const int pasteOptionIndex = 1;
            IContextMenuItem pasteOption = lastMenu.Items[pasteOptionIndex];
            bool itIsIndeedThat = pasteOption.Content.text.ToLower() == "paste";
            Assume.That(itIsIndeedThat, $"The item at index {pasteOptionIndex} should be Paste");

            Assert.IsTrue(pasteOption.Disabled, "Paste option should be disabled. It ain't.");
        }

        [Test]
        public virtual void Add_AddsAndSelectsSingleBlock()
        {
            Assert.IsTrue(handler.Handle(rightClickEmptySpace, ctx), "Handler should consume the right click");

            var lastMenu = menuFactory.LastMenu;
            IContextMenuItem addItem = (from elem in lastMenu.Items
                                        where elem.Content.text.ToLower() == "add"
                                        select elem).FirstOrDefault();
            Assume.That(addItem != null, "Right-clicking empty space should give the user the Add option");

            int blockCountBefore = host.Created.Count;
            addItem.Callback();
            int blockCountAfter = host.Created.Count;
            int amountAdded = blockCountAfter - blockCountBefore;
            bool addedJustOneBlock = amountAdded == 1;
            Assert.IsTrue(addedJustOneBlock, $"Did not add just one Block. Amount added: {amountAdded}");

            var selectedBlocks = host.Flowchart.SelectedBlocks;
            Block addedBlock = host.Created.Last();
            bool selectedTheAddedBlock = selectedBlocks.Contains(addedBlock);
            Assert.IsTrue(selectedTheAddedBlock, "The added block wasn't selected");
        }

        [Test]
        public virtual void Copy_SingleItem_CopiesClipboard()
        {
            SelectJustOneBlock();
            void SelectJustOneBlock()
            {
                host.Flowchart.ClearSelectedBlocks();
                Assert.IsTrue(handler.Handle(rightClickBlock, ctx), "The handler should consume the right-click");

                IList<Block> selectedBlocks = host.Flowchart.SelectedBlocks;
                string errorMessage = $"We should have one block selected after right-clicking on it. "
                    + $"Amount we have selected: {selectedBlocks.Count}. Might want to take a look at SingleSelectionHandler.";
                Assume.That(selectedBlocks.Count == 1, errorMessage);
            }

            IContextMenuItem copyItem;
            AssumeWeHaveTheOptionToCopy();
            void AssumeWeHaveTheOptionToCopy()
            {
                var lastMenu = menuFactory.LastMenu;
                copyItem = lastMenu.Items.First(i => i.Content.text.ToLower() == "copy");
                Assume.That(copyItem != null, "On right-clicking a block, the Copy option should show up");
            }

            CopyAndValidateTheEntry();
            void CopyAndValidateTheEntry()
            {
                copyItem.Callback();
                var clipboard = host.Clipboard;
                int entryCount = clipboard.EntryCount;
                bool justOneEntry = entryCount == 1;
                Assert.IsTrue(justOneEntry, $"Clipboard should have only 1 entry. Instead it has {entryCount}.");

                Block blockExpected = ctx.TopmostBlockOverlapping(rightClickBlock.mousePosition);
                bool entryIsForTheRightBlock = clipboard.HasEntryFor(blockExpected);
                Assert.IsTrue(entryIsForTheRightBlock, "Clipboard does not have an entry for the right Block");
            }
        }

        Event ContextClick(float x, float y)
        {
            return new Event
            {
                type = EventType.ContextClick,
                mousePosition = new Vector2(x, y)
            };
        }

        [Test]
        public virtual void Cut_DeletesTheRightOriginals()
        {
            string errorMessage = string.Empty;
            CommonCutTest(out IList<Block> selectedBlocks, out IList<ushort> blockIDsBeforeCut);
            
            bool allNulls = selectedBlocks.All(item => item == null);
            Assert.IsTrue(allNulls, "All of the original vers of the cut blocks should be null");

        }

        protected virtual void CommonCutTest(out IList<Block> selectedBlocks, out IList<ushort> selectedBlockIDsBeforeCut)
        {
            Assert.IsTrue(handler.Handle(rightClickBlock, ctx), "Handler should consume the right-click");
            IList<Block> localSelectedBlocks;
            IList<ushort> localBlockIDsBeforeCut;
            // ^So we can later pass to the out params with less hassle
            string errorMessage = string.Empty;

            CheckIfAtLeastOneIsSelected(out localSelectedBlocks);
            void CheckIfAtLeastOneIsSelected(out IList<Block> localSelectedBlocks)
            {
                localSelectedBlocks = host.Flowchart.SelectedBlocks;
                bool atLeastOneSelected = localSelectedBlocks.Count > 0;
                errorMessage = "At least one block should be selected upon right-clicking one." +
                    "\nThere might be an issue with SingleSelectionHandler.";
                Assume.That(atLeastOneSelected, errorMessage);
            }

            DoTheCutting();
            void DoTheCutting()
            {
                localBlockIDsBeforeCut = (from elem in localSelectedBlocks
                                     select elem.ItemId).ToList();
                // ^Given how cutting deletes the original blocks, we need to register the IDs
                // here so we can check that the right stuff gets queued and such
                
                var cutItem = menuFactory.LastMenu.Items
                     .First(i => i.Content.text.ToLower() == "cut");
                cutItem.Callback();
                _fcWindowEditing.OnGUI();
                Assert.IsTrue(host.Clipboard.HasEntries, "Clipboard has no entries after a Cut op");
            }

            selectedBlocks = localSelectedBlocks;
            selectedBlockIDsBeforeCut = localBlockIDsBeforeCut;
            
        }

        [Test]
        public virtual void Cut_RegistersCorrectCopies()
        {
            string errorMessage = string.Empty;
            CommonCutTest(out IList<Block> selectedBlocks, out IList<ushort> blockIDsBeforeCut);

            CheckThatTheRightStuffWasCut();
            void CheckThatTheRightStuffWasCut()
            {
                bool success = host.Clipboard.HasMultiEntriesWithIDs(blockIDsBeforeCut);
                Assert.IsTrue(success, "Not all the right stuff was cut.");
            }

        }

        [TestCaseSource(nameof(ExpectedBlockRightClickLabels))]
        public virtual void RightClickBlock_MenuContainsExpectedItem(string expectedLabel)
        {
            expectedLabel = expectedLabel.ToLower();
            Assert.IsTrue(handler.Handle(rightClickBlock, ctx), "The handler should consume the right-click");

            var lastMenu = menuFactory.LastMenu;
            IList<string> items = lastMenu.Items.Select(i => i.Content.text.ToLower()).ToArray();
            bool hasTheItem = items.Contains(expectedLabel);
            Assert.IsTrue(hasTheItem, $"Menu should contain '{expectedLabel}'.");
        }

        static readonly string[] ExpectedBlockRightClickLabels =
        {
            "Copy",
            "Cut",
            "Delete"
        };

        [Test]
        public void RightClickEmpty_MenuItemsAreInCorrectOrder()
        {
            // Act: simulate right‐click on empty space
            Assert.IsTrue(handler.Handle(rightClickEmptySpace, ctx),
                "Handler should consume the right‐click");

            var expectedLabelSequence = new List<string>
            {
                "Add",
                "Paste",
                "---",
                "Stop All"
            };

            var lastMenu = menuFactory.LastMenu;
            var actualSequence = lastMenu.Items
                .Select(item => item.Content.text)
                .ToList();

            // Assert that the two lists match exactly, in order
            CollectionAssert.AreEqual
            (
                expectedLabelSequence,
                actualSequence,
                "Context‐menu items are not in the expected order."
            );
        }

        [Test]
        public void RightClickEmpty_ClipboardHasStuff_PasteEnabled()
        {
            AddToTheClipboard();
            void AddToTheClipboard()
            {
                host.Clipboard.Copy(host.Created);
                Assume.That(host.HasClipboard, "Precondition failed: clipboard should have entries.");
            }

            CheckAccessToThePasteOption();
            void CheckAccessToThePasteOption()
            {
                Assert.IsTrue(handler.Handle(rightClickEmptySpace, ctx),
                    "Handler should consume the right‐click");

                // Assert: there is a Paste item and it is enabled
                var lastMenu = menuFactory.LastMenu;
                var pasteOption = lastMenu.Items
                    .First(item => item.Content.text.ToLower() == "paste");
                Assert.IsNotNull(pasteOption, "Menu should contain a Paste option");
                Assert.IsFalse(pasteOption.Disabled,
                    "Paste option should be enabled when clipboard has entries");
            }
        }

        [Test]
        public void RightClickEmpty_DropDownRectMatchesMousePosition()
        {
            // Act: simulate right‐click on empty space
            Assert.IsTrue(handler.Handle(rightClickEmptySpace, ctx),
                "Handler should consume the right‐click");

            // Inspect the rect used for the drop‐down
            var dropDownRect = menuFactory.LastMenu.DropDownRect;
            var expectedPos = rightClickEmptySpace.mousePosition;

            // Assert: the drop‐down origin matches the mouse position
            Assert.AreEqual(expectedPos.x, dropDownRect.x,
                "DropDownRect.x should match the mouse x position");
            Assert.AreEqual(expectedPos.y, dropDownRect.y,
                "DropDownRect.y should match the mouse y position");
        }

    }
}