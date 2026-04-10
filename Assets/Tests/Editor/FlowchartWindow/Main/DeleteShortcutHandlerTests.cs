using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using AtMycelia.Hyphlow.EditorUtils;

namespace VScriptingTests.FCWindowOperations
{
    [TestFixture]
    public class DeleteShortcutHandlerTests : FlowchartWindowTestsCommon
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _focus = new FakeFocusChecker();
            _focus.IsFocused = true;
            _handler = new DeleteShortcutHandler(
                new FcWindowBlockDeletion(),
                shortcutKey,
                _focus
            );

            _evt = new Event
            {
                type = EventType.KeyDown,
                keyCode = shortcutKey
            };
        }

        protected FakeFocusChecker _focus;
        protected DeleteShortcutHandler _handler;
        protected Event _evt;

        protected readonly KeyCode shortcutKey = KeyCode.Delete;

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            host = null;
            ctx = null;
            _focus = null;
            _handler = null;
            _evt = null;
        }

        [Test, TestCaseSource(nameof(WrongKeys))]
        public void Handle_ReturnsFalse_OnWrongKeyPressed(KeyCode key)
        {
            _focus.IsFocused = true;
            _evt.keyCode = key;

            bool result = _handler.Handle(_evt, ctx);

            Assert.IsFalse(result, "Should not delete anything in response to the wrong key");
            Assert.AreEqual(EventType.KeyDown, _evt.type, "The event type should be key down");
            Assert.AreEqual(0, host.QueuedForDeletion.Count, "Nothing should be queued for deletion");
        }

        static IEnumerable<KeyCode> WrongKeys()
        {
            yield return KeyCode.A;
            yield return KeyCode.B;
            yield return KeyCode.C;
        }

        [Test]
        public void Handle_ReturnsFalse_WhenNotFocused()
        {
            _focus.IsFocused = false;

            bool result = _handler.Handle(_evt, ctx);

            Assert.IsFalse(result, "Should not delete anything when the right stuff isn't focused");
            Assert.AreEqual(EventType.KeyDown, _evt.type, "The event type should be key down");
            Assert.AreEqual(0, host.QueuedForDeletion.Count, "Nothing should be queued for deletion");
        }

        [Test]
        public void Handle_ReturnsFalse_WhenNoBlocksSelected()
        {
            var selection = ctx.Selection;
            _focus.IsFocused = true;
            ctx.Flowchart.ClearSelectedBlocks();

            bool result = _handler.Handle(_evt, ctx);

            Assert.IsFalse(result, "Should not delete anything when there are no blocks selected");
            Assert.AreEqual(EventType.KeyDown, _evt.type, "The event type should be key down");
            Assert.AreEqual(0, host.QueuedForDeletion.Count, "Nothing should be queued for deletion");
        }

        [Test, TestCaseSource(nameof(DefaultAndAlternateKeys))]
        public void Handle_DeletesBlocks_WhenKeyAndFocused(KeyCode defaultOrAlternateKey)
        {
            _handler = new DeleteShortcutHandler(
                new FcWindowBlockDeletion(),
                defaultOrAlternateKey,
                _focus
            );
            _evt.keyCode = defaultOrAlternateKey;
            _focus.IsFocused = true;
            var block = host.CreateBlock(host.Flowchart, Vector2.zero);
            ctx.Flowchart.ClearSelectedBlocks();
            ctx.Flowchart.AddToSelection(block);

            bool result = _handler.Handle(_evt, ctx);

            Assert.IsTrue(result, "Blocks should've been deleted");
            Assert.AreEqual(EventType.Used, _evt.type, "Event should've been registered as used");
            Assert.AreEqual(0, host.QueuedForDeletion.Count, "Nothing should be queued for deletion by the time the deletion is done");
            Assert.AreEqual(1, ctx.ForceRepaintCount, "There should've been one force repaint scheduled");
        }

        static IEnumerable<KeyCode> DefaultAndAlternateKeys()
        {
            yield return KeyCode.Delete;
            yield return KeyCode.D;
            yield return KeyCode.Insert;
        }

    }
}