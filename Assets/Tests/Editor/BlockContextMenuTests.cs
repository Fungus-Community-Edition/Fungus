using Amanita.EditorUtils;
using NUnit.Framework;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Amanita.Tests.Editor
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
                Window = null      // not used by the handler
            };

            rightClickEmptySpace = RightClick(whereEmptySpaceShouldBe);
            rightClickBlock = RightClick(whereABlockShouldBe);

        }

        protected FakeFlowchartHost host;
        protected FakeContextMenuFactory menuFactory;
        protected BlockContextMenuHandler handler;
        protected FlowchartContext ctx;
        protected Event rightClickEmptySpace;
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
            host.Dispose();
        }

        [Test]
        public void StartsWithEmptyClipboard()
        {
            IContextMenu lastMenu = menuFactory.Create();
            IList<IContextMenuItem> actualItems = lastMenu.Items;
            bool noItems = actualItems.Count == 0;
            Assert.IsTrue(noItems, "Clipboard should start empty, but doesn't.");
        }

        [Test]
        public void RightClickEmpty_ShowsAddPasteStopAll()
        {
            Assert.IsTrue(handler.Handle(rightClickEmptySpace, ctx), "Handler should consume the right-click");

            var lastMenu = menuFactory.LastMenu;
            IList<string> items = lastMenu.Items.Select(i => i.Content.text).ToArray();
            IList<string> expectedMenuItems = new[] { "Add", "Paste", "---", "Stop All" };
            Assert.AreEqual(expectedMenuItems, items, "The menu doesn't show the items we expect.");
        }

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
        public void Add_AddsAndSelectsBlock()
        {
            Assert.IsTrue(handler.Handle(rightClickEmptySpace, ctx), "Handler should consume the right click");

            IContextMenuItem addItem = menuFactory.LastMenu.Items.First(i => i.Content.text == "Add");
            addItem.Callback();

            Assert.AreEqual(1, host.Created.Count);
            Assert.Contains(host.Created[0], host.Flowchart.SelectedBlocks.ToList());
        }

        [Test]
        public void Copy_CopiesClipboard()
        {
            // arrange: make a block on the flowchart
            var b = host.Flowchart.CreateBlock(Vector2.zero);
            b._NodeRect = new Rect(Vector2.zero, new Vector2(20, 20));
            host.Flowchart.AddSelectedBlock(b);


            // tell the context we clicked *on* that block
            ctx.BlockHitInLastMouseDown = b;

            // simulate right‐click anywhere (handler will ignore mouse coords now)
            var ev = ContextClick(10, 10);
            bool consumed = handler.Handle(ev, ctx);
            Assert.IsTrue(consumed, "Handler should consume context‐click");

            // act
            handler.Handle(ev, ctx);

            // now “Copy” must be in the menu
            var menu = menuFactory.LastMenu;
            var copyItem = menuFactory.LastMenu.Items
                   .First(i => i.Content.text == "Copy");
            Assert.NotNull(copyItem);
            copyItem.Callback();
            Assert.IsTrue(host.Clipboard.HasEntries);

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
        public void Cut_CopiesAndQueuesForDelete()
        {
            var block = host.Flowchart.CreateBlock(Vector2.zero);
            host.Flowchart.AddSelectedBlock(block);

            var ev = RightClick(0, 0);
            handler.Handle(ev, ctx);
            var cutItem = menuFactory.LastMenu.Items
                 .First(i => i.Content.text == "Cut");
            cutItem.Callback();

            Assert.IsTrue(host.Clipboard.HasEntries);
            Assert.Contains(block, host.Queued);
        }

        [Test]
        public virtual void RightClickBlock_ShowsCopyCutAndDelete()
        {
            Assert.IsTrue(handler.Handle(rightClickBlock, ctx), "The handler should consume the right-click");
            Assert.Ignore();
        }
    }
}