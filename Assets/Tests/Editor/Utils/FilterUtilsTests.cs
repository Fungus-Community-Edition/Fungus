using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using AtMycelia.Hyphlow.EditorExt;
using AtMycelia.Hyphlow;

namespace VScriptingTests.FCWindowOperations
{
    // Minimal stub for Command to supply searchable content
    class DummyCommand : Command
    {
        string content;

        public virtual void Init(string initContent)
        {
            content = initContent;
        }

        public DummyCommand(string content) { this.content = content; }
        public override string GetSearchableContent() => content;
    }

    // Minimal stub for Block to hold a name & list of commands
    class DummyBlock : Block
    {
        public override string BlockName { get; set; }
        public override IReadOnlyList<ICommand> CommandList => new List<ICommand>();

    }

    public class FilterUtilsTests
    {
        [SetUp]
        public virtual void Setup()
        {
            holder = new GameObject("Holder");
            holder.AddComponent<Flowchart>();
        }

        protected GameObject holder;

        [TearDown]
        public virtual void Teardown()
        {
            UnityObject.DestroyImmediate(holder);
        }


        [Test]
        public void EmptyQuery_ReturnsAllBlocksWithFullState()
        {
            DummyBlock firstBlock = holder.AddComponent<DummyBlock>();
            firstBlock.BlockName = "First";
            DummyBlock secondBlock = holder.AddComponent<DummyBlock>();
            secondBlock.BlockName = "Second";

            var blocks = new Block[]
            {
                firstBlock, secondBlock
            };

            var result = FilterUtils.FilterBlocks(blocks, "");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(FilteredState.Full, blocks[0].FilterState);
            Assert.AreEqual(FilteredState.Full, blocks[1].FilterState);
        }

        [Test]
        public void NameMatch_IsFullMatch()
        {
            var firstBlock = holder.AddComponent<DummyBlock>();
            firstBlock.BlockName = "MyBlockOne";

            var secondBlock = holder.AddComponent<DummyBlock>();
            secondBlock.BlockName = "Other";

            var bothBlocks = new Block[] { firstBlock, secondBlock };

            var result = FilterUtils.FilterBlocks(bothBlocks, "block");

            Assert.AreEqual(1, result.Count);
            bool containsIt = result.Contains(firstBlock);
            Assert.IsTrue(containsIt);
            Assert.AreEqual(FilteredState.Full, firstBlock.FilterState);
            Assert.AreEqual(FilteredState.None, secondBlock.FilterState);
        }

        [Test]
        public void ContentMatch_IsPartialMatch()
        {
            var firstBlock = holder.AddComponent<DummyBlock>();
            firstBlock.BlockName = "NoMatchName";
            var secondBlock = holder.AddComponent<DummyBlock>();
            secondBlock.BlockName = "Other";

            var dummyCommand = holder.AddComponent<DummyCommand>();
            dummyCommand.Init("containsFOOinside");

            firstBlock.Add(dummyCommand);
            var bothBlocks = new Block[] { firstBlock, secondBlock };
            var result = FilterUtils.FilterBlocks(bothBlocks, "foo");

            Assert.AreEqual(1, result.Count);
            bool containsIt = result.Contains(firstBlock);
            Assert.IsTrue(containsIt);
            Assert.AreEqual(FilteredState.Partial, firstBlock.FilterState);
            Assert.AreEqual(FilteredState.None, secondBlock.FilterState);
        }

        [Test]
        public void MixedMatches_ReturnsCorrectStates()
        {
            var full = holder.AddComponent<DummyBlock>();
            full.BlockName = "MatchName";
            var partial = holder.AddComponent<DummyBlock>();
            partial.BlockName = "NoName";
            var none = holder.AddComponent<DummyBlock>();
            none.BlockName ="NothingHere";

            var blocks = new Block[] { full, partial, none };

            var commandForFull = holder.AddComponent<DummyCommand>();
            commandForFull.Init("ignore");

            full.Add(commandForFull);

            var commandForPartial = holder.AddComponent<DummyCommand>();
            commandForPartial.Init("lookForTHIS");
            partial.Add(commandForPartial);

            var result = FilterUtils.FilterBlocks(blocks, "this");

            Assert.AreEqual(1, result.Count);
            bool containsIt = result.Contains(partial);
            Assert.IsTrue(containsIt, "The result does not contain the partial block alone.");
            // full name match because "this" in "MatchName"? No → full only if name contains.
            Assert.AreEqual(FilteredState.None, full.CommandList.Count > 0
                ? full.FilterState // stays none because no content match
                : full.FilterState);
            Assert.AreEqual(FilteredState.Partial, partial.FilterState);
            Assert.AreEqual(FilteredState.None, none.FilterState);
        }

        [Test]
        public void NoMatches_ReturnsEmptyList()
        {
            var firstBlock = holder.AddComponent<DummyBlock>();
            firstBlock.BlockName = "A";
            var secondBlock = holder.AddComponent<DummyBlock>();
            secondBlock.BlockName = "B";
            var blocks = new Block[]
            {
                firstBlock, secondBlock
            };

            var result = FilterUtils.FilterBlocks(blocks, "Z");

            Assert.IsEmpty(result);
            Assert.AreEqual(FilteredState.None, blocks[0].FilterState);
            Assert.AreEqual(FilteredState.None, blocks[1].FilterState);
        }
    }
}